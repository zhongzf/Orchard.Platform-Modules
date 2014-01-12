using NHibernate;
using NHibernate.Cfg;
using Orchard;
using Orchard.Data;
using Orchard.Data.Providers;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.FileSystems.AppData;
using Orchard.Localization;
using Orchard.Logging;
using RaisingStudio.SessionFactory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaisingStudio.SessionFactory.Services
{
    public class CustomSessionFactoryHolder : ICustomSessionFactoryHolder, IDisposable
    {
        private readonly ShellSettings _shellSettings;
        private readonly ShellBlueprint _shellBlueprint;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IDatabaseCacheConfiguration _cacheConfiguration;
        private readonly Func<IEnumerable<ISessionConfigurationEvents>> _configurers;
        private readonly IDataServicesProviderFactory _dataServicesProviderFactory;
        private readonly IAppDataFolder _appDataFolder;
        private readonly ISessionConfigurationCache _sessionConfigurationCache;

        private ISessionFactory _sessionFactory;
        private Configuration _configuration;

        private readonly string _provider;
        private readonly string _connectionString;

        public CustomSessionFactoryHolder(
            ShellSettings shellSettings,
            ShellBlueprint shellBlueprint,
            IDataServicesProviderFactory dataServicesProviderFactory,
            IAppDataFolder appDataFolder,
            ISessionConfigurationCache sessionConfigurationCache,
            IHostEnvironment hostEnvironment,
            IDatabaseCacheConfiguration cacheConfiguration,
            Func<IEnumerable<ISessionConfigurationEvents>> configurers,
            string provider,
            string connectionString
            )
        {
            _shellSettings = shellSettings;
            _shellBlueprint = shellBlueprint;
            _dataServicesProviderFactory = dataServicesProviderFactory;
            _appDataFolder = appDataFolder;
            _sessionConfigurationCache = sessionConfigurationCache;
            _hostEnvironment = hostEnvironment;
            _cacheConfiguration = cacheConfiguration;
            _configurers = configurers;

            _provider = provider;
            _connectionString = connectionString;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void Dispose()
        {
            if (_sessionFactory != null)
            {
                _sessionFactory.Dispose();
                _sessionFactory = null;
            }
        }

        public ISessionFactory GetSessionFactory()
        {
            lock (this)
            {
                if (_sessionFactory == null)
                {
                    _sessionFactory = BuildSessionFactory();
                }
            }
            return _sessionFactory;
        }

        public Configuration GetConfiguration()
        {
            lock (this)
            {
                if (_configuration == null)
                {
                    _configuration = BuildConfiguration();
                }
            }
            return _configuration;
        }

        private ISessionFactory BuildSessionFactory()
        {
            Logger.Debug("Building session factory");

            if (!_hostEnvironment.IsFullTrust)
                NHibernate.Cfg.Environment.UseReflectionOptimizer = false;

            Configuration config = GetConfiguration();
            var result = config.BuildSessionFactory();
            Logger.Debug("Done building session factory");
            return result;
        }

        private Configuration BuildConfiguration()
        {
            Logger.Debug("Building configuration");
            var parameters = GetSessionFactoryParameters();

            // TODO: Cache.
            var config = //_sessionConfigurationCache.GetConfiguration(() =>
                _dataServicesProviderFactory
                    .CreateProvider(parameters)
                    .BuildConfiguration(parameters)
                //.Cache(c => _cacheConfiguration.Configure(c)))
            ;

            #region NH specific optimization
            // cannot be done in fluent config
            // the IsSelectable = false prevents unused ContentPartRecord proxies from being created 
            // for each ContentItemRecord or ContentItemVersionRecord.
            // done for perf reasons - has no other side-effect

            foreach (var persistentClass in config.ClassMappings)
            {
                if (persistentClass.EntityName.StartsWith("Orchard.ContentManagement.Records."))
                {
                    foreach (var property in persistentClass.PropertyIterator)
                    {
                        if (property.Name.EndsWith("Record") && !property.IsBasicPropertyAccessor)
                        {
                            property.IsSelectable = false;
                        }
                    }
                }
            }
            #endregion

            parameters.Configurers.Invoke(c => c.Finished(config), Logger);

            Logger.Debug("Done Building configuration");
            return config;
        }

        public SessionFactoryParameters GetSessionFactoryParameters()
        {
            var shellPath = _appDataFolder.Combine("Sites", _shellSettings.Name);
            _appDataFolder.CreateDirectory(shellPath);

            var shellFolder = _appDataFolder.MapPath(shellPath);

            return new SessionFactoryParameters
            {
                Configurers = _configurers(),
                Provider = _provider,
                DataFolder = shellFolder,
                ConnectionString = _connectionString,
                RecordDescriptors = _shellBlueprint.Records,
            };
        }
    }
}