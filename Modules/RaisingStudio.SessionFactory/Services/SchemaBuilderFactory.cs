using Orchard.Data;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Configuration;
using Orchard.Reports.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaisingStudio.SessionFactory.Services
{
    public class SchemaBuilderFactory : ISchemaBuilderFactory
    {
        private readonly ShellSettings _shellSettings;
        private readonly IReportsCoordinator _reportsCoordinator;
        private readonly ISessionFactoryHolderFactory _sessionFactoryHolderFactory;
        private readonly ISessionLocator _sessionLocator;
        private readonly ISessionFactoryHolder _sessionFactoryHolder;


        public SchemaBuilderFactory(
            ShellSettings shellSettings,
            IReportsCoordinator reportsCoordinator,
            ISessionFactoryHolderFactory sessionFactoryHolderFactory,
            ISessionLocator sessionLocator,
            ISessionFactoryHolder sessionFactoryHolder)
        {
            _shellSettings = shellSettings;
            _reportsCoordinator = reportsCoordinator;
            _sessionFactoryHolderFactory = sessionFactoryHolderFactory;
            _sessionLocator = sessionLocator;
            _sessionFactoryHolder = sessionFactoryHolder;
        }

        private SchemaBuilder CreateSchemaBuilder(ICustomSessionFactoryHolder sessionFactoryHolder, string featurePrefix = null, Func<string, string> formatPrefix = null)
        {
            ISessionLocator sessionLocator = new CustomSessionLocator(sessionFactoryHolder, false);
            var dataMigrationInterpreter = new DefaultDataMigrationInterpreter(_shellSettings, sessionLocator, new List<ICommandInterpreter>(), sessionFactoryHolder, _reportsCoordinator);
            return new SchemaBuilder(dataMigrationInterpreter, featurePrefix, formatPrefix);
        }

        public SchemaBuilder CreateSchemaBuilder(string provider, string connectionString, string featurePrefix = null, Func<string, string> formatPrefix = null)
        {
            var sessionFactoryHolder = _sessionFactoryHolderFactory.CreateSessionFactoryHolder(provider, connectionString);
            return CreateSchemaBuilder(sessionFactoryHolder, featurePrefix, formatPrefix);
        }

        public SchemaBuilder CreateSchemaBuilder(string name = null, string featurePrefix = null, Func<string, string> formatPrefix = null)
        {
            if (name == null)
            {
                var dataMigrationInterpreter = new DefaultDataMigrationInterpreter(_shellSettings, _sessionLocator, new List<ICommandInterpreter>(), _sessionFactoryHolder, _reportsCoordinator);
                return new SchemaBuilder(dataMigrationInterpreter, featurePrefix, formatPrefix);
            }
            else
            {
                var sessionFactoryHolder = _sessionFactoryHolderFactory.CreateSessionFactoryHolder(name);
                return CreateSchemaBuilder(sessionFactoryHolder, featurePrefix, formatPrefix);
            }
        }
    }
}