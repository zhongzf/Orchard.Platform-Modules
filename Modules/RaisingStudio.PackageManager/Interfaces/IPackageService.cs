using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaisingStudio.PackageManager.Interfaces
{
    public interface IPackageService : IDependency
    {
        string CreatePackage(string extensionName, string path);
    }
}