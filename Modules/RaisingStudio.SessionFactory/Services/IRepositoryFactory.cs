using Orchard;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaisingStudio.SessionFactory.Services
{
    public interface IRepositoryFactory : IDependency
    {
        IRepository<T> GetRepository<T>(string provider, string connectionString) where T : class;
        IRepository<T> GetRepository<T>(string name = null) where T : class;
    }
}
