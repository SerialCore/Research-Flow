using LogicService.Application;
using LogicService.Services;
using System;
using Windows.System;
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
            applicationName.Text = ApplicationInfo.ApplicationName;

            applicationVersion.Text = ApplicationInfo.ApplicationVersion;

            cultureInfo.Text = ApplicationInfo.Culture.DisplayName;

            oSVersion.Text = ApplicationInfo.OperatingSystemVersion.ToString();

            deviceModel.Text = ApplicationInfo.DeviceModel;

            availableMemory.Text = ApplicationInfo.AvailableMemory.ToString();

            firstVersionInstalled.Text = ApplicationInfo.FirstVersionInstalled;

            firstUseTime.Text = ApplicationInfo.FirstUseTime.ToString();

            launchTime.Text = ApplicationInfo.LaunchTime.ToString();

            lastLaunchTime.Text = ApplicationInfo.LastLaunchTime.ToString();

            lastResetTime.Text = ApplicationInfo.LastResetTime.ToString();

            launchCount.Text = ApplicationInfo.LaunchCount.ToString();

            totalLaunchCount.Text = ApplicationInfo.TotalLaunchCount.ToString();

            appUptime.Text = ApplicationInfo.AppUptime.ToString("G");
        }

        private async void Give_Rate(object sender, Windows.UI.Xaml.RoutedEventArgs e)
            => await ApplicationInfo.ShowRatingReviewDialog();

    }

}
