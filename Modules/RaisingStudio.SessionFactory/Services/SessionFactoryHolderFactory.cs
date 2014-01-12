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
    public class SessionFactoryHolderFactory : ISessionFactoryHolderFactory
    {
        private readonly ShellSettings _shellSettings;
        private readonly ShellBlueprint _shellBlueprint;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IDatabaseCacheConfiguration _cacheConfiguration;
        private readonly Func<IEnumerable<ISessionConfigurationEvents>> _configurers;
        private readonly IDataServicesProviderFactory _dataServicesProviderFactory;
        private readonly IAppDataFolder _appDataFolder;
        private readonly ISessionConfigurationCache _sessionConfigurationCache;
        private readonly IRepository<ConnectionsRecord> _connectionsRecordRepository;

        public SessionFactoryHolderFactory(
            ShellSettings shellSettings,
            ShellBlueprint shellBlueprint,
            IDataServicesProviderFactory dataServicesProviderFactory,
            IAppDataFolder appDataFolder,
            ISessionConfigurationCache sessionConfigurationCache,
            IHostEnvironment hostEnvironment,
            IDatabaseCacheConfiguration cacheConfiguration,
            Func<IEnumerable<ISessionConfigurationEvents>> configurers,
            IRepository<ConnectionsRecord> connectionsRecordRepository)
        {
            _shellSettings = shellSettings;
            _shellBlueprint = shellBlueprint;
            _dataServicesProviderFactory = dataServicesProviderFactory;
            _appDataFolder = appDataFolder;
            _sessionConfigurationCache = sessionConfigurationCache;
            _hostEnvironment = hostEnvironment;
            _cacheConfiguration = cacheConfiguration;
            _configurers = configurers;
            _connectionsRecordRepository = connectionsRecordRepository;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }


        public ICustomSessionFactoryHolder CreateSessionFactoryHolder(string provider, string connectionString)
        {
            return new CustomSessionFactoryHolder(
                _shellSettings,
                _shellBlueprint,
                _dataServicesProviderFactory,
                _appDataFolder,
                _sessionConfigurationCache,
                _hostEnvironment,
                _cacheConfiguration,
                _configurers,
                provider,
                connectionString);
        }

        public ICustomSessionFactoryHolder CreateSessionFactoryHolder(string name)
        {
            var connectionsRecord = _connectionsRecordRepository.Fetch(c => c.Name == name).SingleOrDefault();
            return CreateSessionFactoryHolder(connectionsRecord.Provider, connectionsRecord.ConnectionString);
        }
    }
}