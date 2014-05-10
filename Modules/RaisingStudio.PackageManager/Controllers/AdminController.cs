using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data.Migration;
using Orchard.DisplayManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Modules.Events;
using Orchard.Modules.Models;
using Orchard.Modules.Services;
using Orchard.Modules.ViewModels;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Reports.Services;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Packaging.Events;
using RaisingStudio.PackageManager.Interfaces;
using System.Web.Hosting;
using System.IO;
using Orchard.FileSystems.Media;
using Orchard;
using Orchard.Themes.Models;
using Orchard.Themes.Services;
using Orchard.Themes.ViewModels;

namespace RaisingStudio.PackageManager.Controllers
{
    public class AdminController : Controller {
        private readonly Orchard.Modules.Events.IExtensionDisplayEventHandler _extensionDisplayEventHandler;
        private readonly IModuleService _moduleService;
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IReportsCoordinator _reportsCoordinator;
        private readonly IExtensionManager _extensionManager;
        private readonly IFeatureManager _featureManager;
        private readonly IRecipeHarvester _recipeHarvester;
        private readonly IRecipeManager _recipeManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly ShellSettings _shellSettings;
        private readonly IPackageService _packageService;

        private readonly ISiteThemeService _siteThemeService;
        private readonly IThemeService _themeService;
                
        private readonly Lazy<string> _tempPackageStoragePath;

        private readonly IMimeTypeProvider _mimeTypeProvider;


        public AdminController(
            IEnumerable<Orchard.Modules.Events.IExtensionDisplayEventHandler> extensionDisplayEventHandlers,
            IOrchardServices services,
            IModuleService moduleService,
            IDataMigrationManager dataMigrationManager,
            IReportsCoordinator reportsCoordinator,
            IExtensionManager extensionManager,
            IFeatureManager featureManager,
            IRecipeHarvester recipeHarvester,
            IRecipeManager recipeManager,
            ShellDescriptor shellDescriptor,
            ShellSettings shellSettings,
            IShapeFactory shapeFactory,
            IPackageService packageService,
            IMimeTypeProvider mimeTypeProvider,
            ISiteThemeService siteThemeService,
            IThemeService themeService)
        {
            Services = services;
            _extensionDisplayEventHandler = extensionDisplayEventHandlers.FirstOrDefault();
            _moduleService = moduleService;
            _dataMigrationManager = dataMigrationManager;
            _reportsCoordinator = reportsCoordinator;
            _extensionManager = extensionManager;
            _featureManager = featureManager;
            _recipeHarvester = recipeHarvester;
            _recipeManager = recipeManager;
            _shellDescriptor = shellDescriptor;
            _shellSettings = shellSettings;
            Shape = shapeFactory;
            _packageService = packageService;
            _mimeTypeProvider = mimeTypeProvider;

            _siteThemeService = siteThemeService;
            _themeService = themeService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

            _tempPackageStoragePath = new Lazy<string>(() =>
            {
                var path = HostingEnvironment.MapPath("~/App_Data/Packages");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            });
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public dynamic Shape { get; set; }


        public ActionResult Index(ModulesIndexOptions options, PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage modules")))
                return new HttpUnauthorizedResult();

            Pager pager = new Pager(Services.WorkContext.CurrentSite, pagerParameters);

            IEnumerable<ModuleEntry> modules = _extensionManager.AvailableExtensions()
                .Where(extensionDescriptor => DefaultExtensionTypes.IsModule(extensionDescriptor.ExtensionType) &&
                                              (string.IsNullOrEmpty(options.SearchText) || extensionDescriptor.Name.ToLowerInvariant().Contains(options.SearchText.ToLowerInvariant())))
                .OrderBy(extensionDescriptor => extensionDescriptor.Name)
                .Select(extensionDescriptor => new ModuleEntry { Descriptor = extensionDescriptor });

            int totalItemCount = modules.Count();

            if (pager.PageSize != 0)
            {
                modules = modules.Skip((pager.Page - 1) * pager.PageSize).Take(pager.PageSize);
            }

            modules = modules.ToList();
            foreach (ModuleEntry moduleEntry in modules)
            {
                moduleEntry.IsRecentlyInstalled = _moduleService.IsRecentlyInstalled(moduleEntry.Descriptor);

                if (_extensionDisplayEventHandler != null)
                {
                    foreach (string notification in _extensionDisplayEventHandler.Displaying(moduleEntry.Descriptor, ControllerContext.RequestContext))
                    {
                        moduleEntry.Notifications.Add(notification);
                    }
                }
            }

            return View(new ModulesIndexViewModel
            {
                Modules = modules,
                InstallModules = _featureManager.GetEnabledFeatures().FirstOrDefault(f => f.Id == "PackagingServices") != null,
                Options = options,
                Pager = Shape.Pager(pager).TotalItemCount(totalItemCount)
            });
        }
        
        public ActionResult Themes()
        {
            bool installThemes =
                _featureManager.GetEnabledFeatures().FirstOrDefault(f => f.Id == "PackagingServices") != null
                && Services.Authorizer.Authorize(StandardPermissions.SiteOwner) // only site owners
                && _shellSettings.Name == ShellSettings.DefaultName; // of the default tenant

            var featuresThatNeedUpdate = _dataMigrationManager.GetFeaturesThatNeedUpdate();

            ThemeEntry currentTheme = null;
            ExtensionDescriptor currentThemeDescriptor = _siteThemeService.GetSiteTheme();
            if (currentThemeDescriptor != null)
            {
                currentTheme = new ThemeEntry(currentThemeDescriptor);
            }

            IEnumerable<ThemeEntry> themes = _extensionManager.AvailableExtensions()
                .Where(extensionDescriptor =>
                {
                    bool hidden = false;
                    string tags = extensionDescriptor.Tags;
                    if (tags != null)
                    {
                        hidden = tags.Split(',').Any(t => t.Trim().Equals("hidden", StringComparison.OrdinalIgnoreCase));
                    }

                    // is the theme allowed for this tenant ?
                    bool allowed = _shellSettings.Themes.Length == 0 || _shellSettings.Themes.Contains(extensionDescriptor.Id);

                    return !hidden && allowed &&
                            DefaultExtensionTypes.IsTheme(extensionDescriptor.ExtensionType) &&
                            (currentTheme == null ||
                            !currentTheme.Descriptor.Id.Equals(extensionDescriptor.Id));
                })
                .Select(extensionDescriptor =>
                {
                    ThemeEntry themeEntry = new ThemeEntry(extensionDescriptor)
                    {
                        NeedsUpdate = featuresThatNeedUpdate.Contains(extensionDescriptor.Id),
                        IsRecentlyInstalled = _themeService.IsRecentlyInstalled(extensionDescriptor),
                        Enabled = _shellDescriptor.Features.Any(sf => sf.Name == extensionDescriptor.Id),
                        CanUninstall = installThemes
                    };

                    if (_extensionDisplayEventHandler != null)
                    {
                        foreach (string notification in _extensionDisplayEventHandler.Displaying(themeEntry.Descriptor, ControllerContext.RequestContext))
                        {
                            themeEntry.Notifications.Add(notification);
                        }
                    }

                    return themeEntry;
                })
                .ToArray();

            return View(new ThemesIndexViewModel
            {
                CurrentTheme = currentTheme,
                InstallThemes = installThemes,
                Themes = themes
            });
        }
        
        public ActionResult Download(string p)
        {
            if (!Services.Authorizer.Authorize(Permissions.DownloadPackages, T("Not allowed to download packages")))
                return new HttpUnauthorizedResult();
            
            string fileName = _packageService.CreatePackage(p, _tempPackageStoragePath.Value);
            if (!string.IsNullOrWhiteSpace(fileName) && System.IO.File.Exists(fileName))
            {
                var filePathResult = new FilePathResult(fileName, _mimeTypeProvider.GetMimeType(fileName));
                filePathResult.FileDownloadName = Path.GetFileName(fileName);
                return filePathResult;
            }
            return new HttpNotFoundResult();
        }
    }
}