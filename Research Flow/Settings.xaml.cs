using LogicService.Application;
using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using System;
using Windows.System;
using Windows.UI.Xaml;
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
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            // https://www.microsoft.com/store/apps/9P8CHR55RS31
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ApplicationSetting.ContainKey("IsDeveloper"))
                developerPanel.Visibility = Visibility.Visible;
            else
                developerPanel.Visibility = Visibility.Collapsed;

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

        #region Settings

        private async void AccountSync_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (GraphService.IsConnected && GraphService.IsNetworkAvailable)
            {
                var button = sender as Button;
                button.IsEnabled = false;
                waitSync.IsActive = true;

                await Synchronization.ScanFiles();

                waitSync.IsActive = false;
                button.IsEnabled = true;
            }
        }

        #endregion

        #region Developer

        private async void Show_FeedTaskLog(object sender, RoutedEventArgs e)
        {
            try
            {
                var folder = await LocalStorage.GetLogFolderAsync();
                var file = await folder.GetFileAsync("FeedTask.log");
                await Launcher.LaunchFileAsync(file);
            }
            catch (Exception exception)
            {
                ApplicationMessage.SendMessage("SettingException: " + exception, ApplicationMessage.MessageType.InApp);
            }
        }

        private async void Show_RSSList(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                var folder = await LocalStorage.GetDataFolderAsync();
                var file = await folder.GetFileAsync("rss.list");
                await Launcher.LaunchFileAsync(file);
            }
            catch (Exception exception)
            {
                ApplicationMessage.SendMessage("SettingException: " + exception, ApplicationMessage.MessageType.InApp);
            }
        }

        private async void Show_TopicList(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                var folder = await LocalStorage.GetDataFolderAsync();
                var file = await folder.GetFileAsync("topic.list");
                await Launcher.LaunchFileAsync(file);
            }
            catch (Exception exception)
            {
                ApplicationMessage.SendMessage("SettingException: " + exception, ApplicationMessage.MessageType.InApp);
            }
        }

        private void Show_BackgroundTask(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            tasklist.ItemsSource = ApplicationTask.ListBackgroundTask();
        }

        private void Show_AlarmToast(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            toastlist.ItemsSource = ApplicationNotification.ListAlarmToast();
        }

        private async void Cancel_AlarmToast(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await ApplicationNotification.CancelAllToast();
            toastlist.ItemsSource = ApplicationNotification.ListAlarmToast();
        }

        private void Show_Database(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            switch (databaselist.SelectedIndex)
            {
                case 0:
                    dataviewer.ItemsSource = Feed.DBSelectByLimit(100);
                    break;
                case 1:
                    dataviewer.ItemsSource = Crawlable.DBSelectByLimit(100);
                    break;
                case 2:
                    dataviewer.ItemsSource = Paper.DBSelectByLimit(100);
                    break;
                case 3:
                    dataviewer.ItemsSource = FileList.DBSelectAllTrace();
                    break;
                case 4:
                    dataviewer.ItemsSource = FileList.DBSelectAllList();
                    break;
            }
        }

        #endregion

    }

}
