using LogicService.Application;
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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InitializeSetting();
            DisplaySystemInfo();
        }

        private void InitializeSetting()
        {
            if (ApplicationSetting.ContainKey("Theme"))
                applicationTheme.IsOn = ApplicationSetting.EqualKey("Theme", "Dark");
            if (ApplicationSetting.ContainKey("InkInput"))
                inkInput.IsOn = ApplicationSetting.EqualKey("InkInput", "Touch");
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

        private async void Give_Rate(object sender, Windows.UI.Xaml.RoutedEventArgs e)
            => await ApplicationInfo.ShowRatingReviewDialog();

        #region Settings

        private void ApplicationTheme_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (applicationTheme.IsOn)
            {
                ApplicationSetting.Theme = "Dark";
            }
            else
            {
                ApplicationSetting.Theme = "Light";
            }

            if (ApplicationSetting.EqualKey("Theme", "Light"))
                (Window.Current.Content as Frame).RequestedTheme = ElementTheme.Light;
            if (ApplicationSetting.EqualKey("Theme", "Dark"))
                (Window.Current.Content as Frame).RequestedTheme = ElementTheme.Dark;
        }

        private void InkInput_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (inkInput.IsOn)
            {
                ApplicationSetting.InkInput = "Touch";
            }
            else
            {
                ApplicationSetting.InkInput = "Pen";
            }
        }

        //private async void Paper_UnCompress(object sender, RoutedEventArgs e)
        //{
        //    FileOpenPicker picker = new FileOpenPicker();
        //    picker.FileTypeFilter.Add(".zip");
        //    StorageFile file = await picker.PickSingleFileAsync();
        //    if (file != null)
        //        LocalStorage.UnCompression(file, LocalStorage.GetPaperFolderAsync());
        //}

        //private async void Pdf_Import(object sender, RoutedEventArgs e)
        //{
        //    FileOpenPicker picker = new FileOpenPicker();
        //    picker.FileTypeFilter.Add(".pdf");
        //    var files = await picker.PickMultipleFilesAsync();
        //    if (files != null)
        //    {
        //        foreach (var file in files)
        //        {
        //            await file.CopyAsync(LocalStorage.GetPaperFolderAsync());
        //            pdfs.Add(file.Name);
        //        }
        //    }
        //}

        //private async void Pdf_Export(object sender, RoutedEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(currentfile))
        //        return;

        //    try
        //    {
        //        var file = await LocalStorage.GetPaperFolderAsync().GetFileAsync(currentfile);
        //        FolderPicker picker = new FolderPicker();
        //        picker.FileTypeFilter.Add(".pdf");
        //        StorageFolder folder = await picker.PickSingleFolderAsync();
        //        if (folder != null)
        //            await file.CopyAsync(folder);
        //    }
        //    catch (Exception ex)
        //    {
        //        ApplicationMessage.SendMessage(new ShortMessage { Title = "PdfException", Content = ex.Message, Time = DateTimeOffset.Now },
        //            ApplicationMessage.MessageType.InApp);
        //    }
        //}

        //private void AccountDownload_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    if (GraphService.IsConnected && ApplicationInfo.IsNetworkAvailable)
        //    {
        //        var button = sender as Button;
        //        button.IsEnabled = false;
        //        waitSync1.IsActive = true;
        //        syncStatu.Visibility = Visibility.Visible;

        //        Synchronization.SyncProgressChanged += Synchronization_SyncProgressChanged;
        //        await Synchronization.DownloadAll();
        //        Synchronization.SyncProgressChanged -= Synchronization_SyncProgressChanged;

        //        syncStatu.Visibility = Visibility.Collapsed;
        //        waitSync1.IsActive = false;
        //        button.IsEnabled = true;
        //        syncCount.Text = "";
        //    }
        //}

        //private void AccountSync_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    if (GraphService.IsConnected && ApplicationInfo.IsNetworkAvailable)
        //    {
        //        var button = sender as Button;
        //        button.IsEnabled = false;
        //        waitSync2.IsActive = true;
        //        syncStatu.Visibility = Visibility.Visible;

        //        //Synchronization.SyncProgressChanged += Synchronization_SyncProgressChanged;
        //        //await Synchronization.ScanFiles();
        //        //Synchronization.SyncProgressChanged -= Synchronization_SyncProgressChanged;

        //        syncStatu.Visibility = Visibility.Collapsed;
        //        waitSync2.IsActive = false;
        //        button.IsEnabled = true;
        //        syncCount.Text = "";
        //    }
        //}

        //private async void Synchronization_SyncProgressChanged(object sender, SyncEventArgs e)
        //{
        //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //    {
        //        syncStatu.Maximum = e.TotalCount;
        //        syncStatu.Value = e.SyncedCount;
        //        syncCount.Text = e.SyncedCount + "/" + e.TotalCount;
        //    });
        //}

        #endregion

        #region Developer

        //private async void Show_FeedTaskLog(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var folder = await LocalStorage.GetLogFolderAsync();
        //        var file = await folder.GetFileAsync("FeedTask.log");
        //        await Launcher.LaunchFileAsync(file);
        //    }
        //    catch (Exception exception)
        //    {
        //        ApplicationMessage.SendMessage(new ShortMessage { Title = "SettingException", Content = exception.Message, Time = DateTimeOffset.Now },
        //            ApplicationMessage.MessageType.InApp);
        //    }
        //}

        //private async void Show_RSSList(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var folder = await LocalStorage.GetDataFolderAsync();
        //        var file = await folder.GetFileAsync("rss.list");
        //        await Launcher.LaunchFileAsync(file);
        //    }
        //    catch (Exception exception)
        //    {
        //        ApplicationMessage.SendMessage(new ShortMessage { Title = "SettingException", Content = exception.Message, Time = DateTimeOffset.Now },
        //            ApplicationMessage.MessageType.InApp);
        //    }
        //}

        //private async void Show_TopicList(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var folder = await LocalStorage.GetDataFolderAsync();
        //        var file = await folder.GetFileAsync("topic.list");
        //        await Launcher.LaunchFileAsync(file);
        //    }
        //    catch (Exception exception)
        //    {
        //        ApplicationMessage.SendMessage(new ShortMessage { Title = "SettingException", Content = exception.Message, Time = DateTimeOffset.Now }, 
        //            ApplicationMessage.MessageType.InApp);
        //    }
        //}

        //private void Show_BackgroundTask(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    tasklist.ItemsSource = ApplicationTask.ListBackgroundTask();
        //}

        //private void Show_AlarmToast(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    toastlist.ItemsSource = ApplicationNotification.ListAlarmToast();
        //}

        //private async void Cancel_AlarmToast(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    await ApplicationNotification.CancelAllToast();
        //    toastlist.ItemsSource = ApplicationNotification.ListAlarmToast();
        //}

        //private void Show_Database(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    switch (databaselist.SelectedIndex)
        //    {
        //        case 0:
        //            dataviewer.ItemsSource = Feed.DBSelectByLimit(100);
        //            break;
        //        case 1:
        //            dataviewer.ItemsSource = Crawlable.DBSelectByLimit(100);
        //            break;
        //        case 2:
        //            dataviewer.ItemsSource = Paper.DBSelectByLimit(100);
        //            break;
        //        case 3:
        //            dataviewer.ItemsSource = PaperFile.DBSelectByLimit(100);
        //            break;
        //        case 4:
        //            dataviewer.ItemsSource = FileList.DBSelectAllTrace();
        //            break;
        //        case 5:
        //            dataviewer.ItemsSource = FileList.DBSelectAllList();
        //            break;
        //    }
        //}

        #endregion

    }

}
