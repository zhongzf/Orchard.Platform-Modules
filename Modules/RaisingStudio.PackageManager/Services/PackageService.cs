using Orchard.Localization;
using Orchard.Packaging.Services;
using Orchard.UI.Notify;
using RaisingStudio.PackageManager.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace RaisingStudio.PackageManager.Services
{
    public class PackageService : IPackageService
    {
        private static readonly string ApplicationPath = HostingEnvironment.MapPath("~/");

        private readonly IPackageManager _packageManager;
        private readonly INotifier _notifier;

        public PackageService(IPackageManager packageManager, INotifier notifier)
        {
            _packageManager = packageManager;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }


        public string CreatePackage(string extensionName, string path)
        {
            var packageData = _packageManager.Harvest(extensionName);
            if (packageData == null)
            {
                Debug.WriteLine(T("Module or Theme \"{0}\" does not exist in this Orchard installation.", extensionName));
                return null;
            }

            // append "Orchard.[ExtensionType]" to prevent conflicts with other packages (e.g, TinyMce, jQuery, ...)
            var filename = string.Format("{0}{1}.{2}.nupkg",
                PackagingSourceManager.GetExtensionPrefix(packageData.ExtensionType),
                packageData.ExtensionName,
                packageData.ExtensionVersion);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // packages are created in a specific folder otherwise they are in /bin, which crashed the current shell
            filename = Path.Combine(path, filename);

            using (var stream = File.Create(filename))
            {
                packageData.PackageStream.CopyTo(stream);
            }

            var fileInfo = new FileInfo(filename);
            Debug.WriteLine(T("Package \"{0}\" successfully created", fileInfo.FullName));
            return fileInfo.FullName;
        }

    }
}