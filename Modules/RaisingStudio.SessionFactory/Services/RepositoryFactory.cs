using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.FileSystems.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaisingStudio.SessionFactory.Services
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly ShellSettings _shellSettings;
        private readonly ShellBlueprint _shellBlueprint;
        private readonly IAppDataFolder _appDataFolder;
#if JsonDataRepository
        private readonly IJsonDataRepositoryFactoryHolder _jsonDataRepositoryFactoryHolder;
#endif
        private readonly ISessionFactoryHolderFactory _sessionFactoryHolderFactory;

        public RepositoryFactory(
            ShellSettings shellSettings,
            ShellBlueprint shellBlueprint,
            IAppDataFolder appDataFolder,
#if JsonDataRepository
            IJsonDataRepositoryFactoryHolder jsonDataRepositoryFactoryHolder,
#endif
            ISessionFactoryHolderFactory sessionFactoryHolderFactory,
            ISessionLocator sessionLocator)
        {
            _shellSettings = shellSettings;
            _shellBlueprint = shellBlueprint;
            _appDataFolder = appDataFolder;
#if JsonDataRepository
            _jsonDataRepositoryFactoryHolder = jsonDataRepositoryFactoryHolder;
#endif
            _sessionFactoryHolderFactory = sessionFactoryHolderFactory;
            _sessionLocator = sessionLocator;
        }

        private IRepository<T> CreateRepository<T>(ICustomSessionFactoryHolder sessionFactoryHolder) where T : class
        {
            ISessionLocator sessionLocator = new CustomSessionLocator(sessionFactoryHolder, false);
#if JsonDataRepository
            IRepository<T> repository = new Repository<T>(sessionLocator, _shellSettings, _shellBlueprint, _appDataFolder, _jsonDataRepositoryFactoryHolder);
#else
            IRepository<T> repository = new Repository<T>(sessionLocator);
#endif
            return repository;
        }

        public IRepository<T> GetRepository<T>(string provider, string connectionString) where T : class
        {
            var sessionFactoryHolder = _sessionFactoryHolderFactory.CreateSessionFactoryHolder(provider, connectionString);
            return CreateRepository<T>(sessionFactoryHolder);
        }

        public IRepository<T> GetRepository<T>(string name = null) where T : class
        {
            if (name == null)
            {
#if JsonDataRepository
                IRepository<T> repository = new Repository<T>(_sessionLocator, _shellSettings, _shellBlueprint, _appDataFolder, _jsonDataRepositoryFactoryHolder);
#else
                IRepository<T> repository = new Repository<T>(_sessionLocator);
#endif
                return repository;
            }
            else
            {
                var sessionFactoryHolder = _sessionFactoryHolderFactory.CreateSessionFactoryHolder(name);
                return CreateRepository<T>(sessionFactoryHolder);
            }
        }
    }
}