using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace RaisingStudio.PackageManager
{
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }

        public string MenuName
        {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("modules")
                .Add(T("Modules"), "9", menu => menu.Add(T("Download"), "5", item => item.Action("Index", "Admin", new { area = "RaisingStudio.PackageManager" }).Permission(Permissions.DownloadPackages).LocalNav())
                    );
            builder.AddImageSet("themes")
                .Add(T("Themes"), "10", menu => menu.Add(T("Download"), "5", item => item.Action("Themes", "Admin", new { area = "RaisingStudio.PackageManager" }).Permission(Permissions.DownloadPackages).LocalNav()));

        }
    }
}
