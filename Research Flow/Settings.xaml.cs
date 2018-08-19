using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using LogicService.Services;
using Research_Flow.Pages;
using Windows.Data.Json;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
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
            applicationName.Text = ApplicationService.ApplicationName;

            applicationVersion.Text = ApplicationService.ApplicationVersion;

            cultureInfo.Text = ApplicationService.Culture.DisplayName;

            oSVersion.Text = ApplicationService.OperatingSystemVersion.ToString();

            deviceModel.Text = ApplicationService.DeviceModel;

            availableMemory.Text = ApplicationService.AvailableMemory.ToString();

            firstVersionInstalled.Text = ApplicationService.FirstVersionInstalled;

            firstUseTime.Text = ApplicationService.FirstUseTime.ToString();

            launchTime.Text = ApplicationService.LaunchTime.ToString();

            lastLaunchTime.Text = ApplicationService.LastLaunchTime.ToString();

            lastResetTime.Text = ApplicationService.LastResetTime.ToString();

            launchCount.Text = ApplicationService.LaunchCount.ToString();

            totalLaunchCount.Text = ApplicationService.TotalLaunchCount.ToString();

            appUptime.Text = ApplicationService.AppUptime.ToString("G");
        }
        
    }

}
