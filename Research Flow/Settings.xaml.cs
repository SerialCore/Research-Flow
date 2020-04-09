using LogicService.Application;
using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using Microsoft.Services.Store.Engagement;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
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
            // ms-windows-store://pdp/?productid=9P8CHR55RS31
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DisplaySystemInfo();
        }

        private void DisplaySystemInfo()
        {
            applicationName.Text = ApplicationInfo.AppName;

            applicationVersion.Text = ApplicationInfo.AppVersion;

            firstVersion.Text = ApplicationInfo.FirstVersionInstalled;

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

        private async void Give_Feedback(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
            {
                var launcher = StoreServicesFeedbackLauncher.GetDefault();
                await launcher.LaunchAsync();
            }
            else
            {
                ApplicationMessage.SendMessage("Your device doesn't support Feedback Hub", ApplicationMessage.MessageType.InApp);
            }
        }

        private async void Give_Rate(object sender, Windows.UI.Xaml.RoutedEventArgs e)
            => await ApplicationInfo.ShowRatingReviewDialog();

        #region Settings

        private async void AccountDownload_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (GraphService.IsConnected && ApplicationInfo.IsNetworkAvailable)
            {
                var button = sender as Button;
                button.IsEnabled = false;
                waitSync1.IsActive = true;
                syncStatu.Visibility = Visibility.Visible;

                Synchronization.SyncProgressChanged += Synchronization_SyncProgressChanged;
                await Synchronization.DownloadAll();
                Synchronization.SyncProgressChanged -= Synchronization_SyncProgressChanged;

                syncStatu.Visibility = Visibility.Collapsed;
                waitSync1.IsActive = false;
                button.IsEnabled = true;
                syncCount.Text = "";
            }
        }

        private async void AccountSync_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (GraphService.IsConnected && ApplicationInfo.IsNetworkAvailable)
            {
                var button = sender as Button;
                button.IsEnabled = false;
                waitSync2.IsActive = true;
                syncStatu.Visibility = Visibility.Visible;

                Synchronization.SyncProgressChanged += Synchronization_SyncProgressChanged;
                await Synchronization.ScanFiles();
                Synchronization.SyncProgressChanged -= Synchronization_SyncProgressChanged;

                syncStatu.Visibility = Visibility.Collapsed;
                waitSync2.IsActive = false;
                button.IsEnabled = true;
                syncCount.Text = "";
            }
        }

        private async void Synchronization_SyncProgressChanged(object sender, SyncEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                syncStatu.Maximum = e.TotalCount;
                syncStatu.Value = e.SyncedCount;
                syncCount.Text = e.SyncedCount + "/" + e.TotalCount;
            });
        }

        private async void CompressPaper_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add(".zip");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
                LocalStorage.Compression(await LocalStorage.GetPaperFolderAsync(), folder);
        }

        private async void ImportPaper_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".zip");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
                LocalStorage.UnCompression(file, await LocalStorage.GetPaperFolderAsync());
        }

        private async void CompressPicture_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add(".zip");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
                LocalStorage.Compression(await LocalStorage.GetPictureFolderAsync(), folder);
        }

        private async void ImportPicture_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".zip");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
                LocalStorage.UnCompression(file, await LocalStorage.GetPictureFolderAsync());
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
                    dataviewer.ItemsSource = PaperFile.DBSelectByLimit(100);
                    break;
                case 4:
                    dataviewer.ItemsSource = FileList.DBSelectAllTrace();
                    break;
                case 5:
                    dataviewer.ItemsSource = FileList.DBSelectAllList();
                    break;
            }
        }

        #endregion

    }

}
