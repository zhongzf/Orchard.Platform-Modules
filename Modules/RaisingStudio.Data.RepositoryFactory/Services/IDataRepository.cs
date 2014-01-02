using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaisingStudio.Data.RepositoryFactory.Services
{
    public interface IDataRepository<T> : IRepository<T>
    {
    }
}