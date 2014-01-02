using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaisingStudio.Data.RepositoryFactory.Services
{
    public interface IDataRepositoryFactory : IDependency
    {
        IDataRepository<T> GetRepository<T>(string provider, string connectionString) where T : class, new();
        IDataRepository<T> GetRepository<T>(string name = null) where T : class, new();
    }
}