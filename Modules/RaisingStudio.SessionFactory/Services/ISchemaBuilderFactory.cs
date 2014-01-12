using Orchard;
using Orchard.Data.Migration.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaisingStudio.SessionFactory.Services
{
    public interface ISchemaBuilderFactory : IDependency
    {
        SchemaBuilder CreateSchemaBuilder(string provider, string connectionString, string featurePrefix = null, Func<string, string> formatPrefix = null);
        SchemaBuilder CreateSchemaBuilder(string name, string featurePrefix = null, Func<string, string> formatPrefix = null);
    }
}
