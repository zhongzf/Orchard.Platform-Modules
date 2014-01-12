using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaisingStudio.SessionFactory.Services
{
    public interface ISessionFactoryHolderFactory : IDependency
    {
        ICustomSessionFactoryHolder CreateSessionFactoryHolder(string provider, string connectionString);
        ICustomSessionFactoryHolder CreateSessionFactoryHolder(string name);
    }
}
