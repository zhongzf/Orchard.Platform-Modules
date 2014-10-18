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
using Orchard.Security;
using RaisingStudio.SessionFactory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Orchard.Utility.Extensions;
using System.IO;
using System.Configuration;
using Orchard.Caching;
using System.Collections.Concurrent;

namespace RaisingStudio.SessionFactory.Services
{
    public class SessionFactoryHolderFactory : ISessionFactoryHolderFactory
    {
        public const string CacheName = "RaisingStudio.SessionFactory.SessionFactoryHolder.Cache";

        private readonly ShellSettings _shellSettings;
        private readonly ShellBlueprint _shellBlueprint;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IDatabaseCacheConfiguration _cacheConfiguration;
        private readonly Func<IEnumerable<ISessionConfigurationEvents>> _configurers;
        private readonly IDataServicesProviderFactory _dataServicesProviderFactory;
        private readonly IAppDataFolder _appDataFolder;
        private readonly ISessionConfigurationCache _sessionConfigurationCache;
        private readonly IRepository<ConnectionsRecord> _connectionsRecordRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly ICacheManager _cacheManager;

        public SessionFactoryHolderFactory(
            ShellSettings shellSettings,
            ShellBlueprint shellBlueprint,
            IDataServicesProviderFactory dataServicesProviderFactory,
            IAppDataFolder appDataFolder,
            ISessionConfigurationCache sessionConfigurationCache,
            IHostEnvironment hostEnvironment,
            IDatabaseCacheConfiguration cacheConfiguration,
            Func<IEnumerable<ISessionConfigurationEvents>> configurers,
            IRepository<ConnectionsRecord> connectionsRecordRepository,
            IEncryptionService encryptionService,
            ICacheManager cacheManager)
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
            _encryptionService = encryptionService;
            _cacheManager = cacheManager;

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
            // cache
            var sessionFactoryHolderCache = _cacheManager.Get(CacheName, ctx =>
            {
                return new ConcurrentDictionary<string, ICustomSessionFactoryHolder>();
            });
            var sessionFactoryHolder = sessionFactoryHolderCache.GetOrAdd(CacheName, key =>
            {
                return InternalCreateSessionFactoryHolder(name);
            });
            return sessionFactoryHolder;
        }

        protected virtual ICustomSessionFactoryHolder InternalCreateSessionFactoryHolder(string name)
        {
            var connectionsRecord = _connectionsRecordRepository.Fetch(c => c.Name == name).SingleOrDefault();
            return CreateSessionFactoryHolder(connectionsRecord.Provider, connectionsRecord.ConnectionString);
        }
    }
}