using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.FileSystems.AppData;
using RaisingStudio.SessionFactory.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaisingStudio.Data.RepositoryFactory.Services
{
    public class DataRepositoryFactory : IDataRepositoryFactory
    {
        private readonly ShellSettings _shellSettings;
        private readonly ShellBlueprint _shellBlueprint;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IJsonDataRepositoryFactoryHolder _jsonDataRepositoryFactoryHolder;
        private readonly ISessionFactoryHolderFactory _sessionFactoryHolderFactory;
        private readonly ISessionLocator _sessionLocator;

        public DataRepositoryFactory(
            ShellSettings shellSettings,
            ShellBlueprint shellBlueprint,
            IAppDataFolder appDataFolder,
            IJsonDataRepositoryFactoryHolder jsonDataRepositoryFactoryHolder,
            ISessionFactoryHolderFactory sessionFactoryHolderFactory,
            ISessionLocator sessionLocator)
        {
            _shellSettings = shellSettings;
            _shellBlueprint = shellBlueprint;
            _appDataFolder = appDataFolder;
            _jsonDataRepositoryFactoryHolder = jsonDataRepositoryFactoryHolder;
            _sessionFactoryHolderFactory = sessionFactoryHolderFactory;
            _sessionLocator = sessionLocator;
        }

        private IDataRepository<T> CreateRepository<T>(ICustomSessionFactoryHolder sessionFactoryHolder) where T : class, new()
        {
            ISessionLocator sessionLocator = new CustomSessionLocator(sessionFactoryHolder, false);
            IDataRepository<T> repository = new DataRepository<T>(sessionLocator, _shellSettings, _shellBlueprint, _appDataFolder, _jsonDataRepositoryFactoryHolder);
            return repository;
        }

        public IDataRepository<T> GetRepository<T>(string provider, string connectionString) where T : class, new()
        {
            var sessionFactoryHolder = _sessionFactoryHolderFactory.CreateSessionFactoryHolder(provider, connectionString);
            return CreateRepository<T>(sessionFactoryHolder);
        }

        public IDataRepository<T> GetRepository<T>(string name = null) where T : class, new()
        {
            if (name == null)
            {
                return new DataRepository<T>(_sessionLocator, _shellSettings, _shellBlueprint, _appDataFolder, _jsonDataRepositoryFactoryHolder); ;
            }
            else
            {
                var sessionFactoryHolder = _sessionFactoryHolderFactory.CreateSessionFactoryHolder(name);
                return CreateRepository<T>(sessionFactoryHolder);
            }
        }
    }
}