using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace RaisingStudio.PackageManager
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission DownloadPackages = new Permission { Description = "Download Packages", Name = "DownloadPackages" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { DownloadPackages };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                             new PermissionStereotype {
                                                          Name = "Administrator",
                                                          Permissions = new[] {DownloadPackages}
                                                      }
                         };
        }
    }
}