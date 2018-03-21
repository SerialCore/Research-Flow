using System;
using System.Net.Http;
using System.Threading.Tasks;
using LogicService.Services;
using Windows.Data.Json;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DisplaySystemInfo();
        }

        private void DisplaySystemInfo()
        {
            applicationName.Text = SystemInfo.ApplicationName;

            applicationVersion.Text = SystemInfo.ApplicationVersion;

            cultureInfo.Text = SystemInfo.Culture.DisplayName;

            oSVersion.Text = SystemInfo.OperatingSystemVersion.ToString();

            deviceModel.Text = SystemInfo.DeviceModel;

            availableMemory.Text = SystemInfo.AvailableMemory.ToString();

            firstVersionInstalled.Text = SystemInfo.FirstVersionInstalled;

            firstUseTime.Text = SystemInfo.FirstUseTime.ToString();

            launchTime.Text = SystemInfo.LaunchTime.ToString();

            lastLaunchTime.Text = SystemInfo.LastLaunchTime.ToString();

            lastResetTime.Text = SystemInfo.LastResetTime.ToString();

            launchCount.Text = SystemInfo.LaunchCount.ToString();

            totalLaunchCount.Text = SystemInfo.TotalLaunchCount.ToString();

            appUptime.Text = SystemInfo.AppUptime.ToString("G");
        }

    }
}
