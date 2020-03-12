using LogicService.Application;
using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Configure : Page
    {
        public Configure()
        {
            this.InitializeComponent();
        }

        private object tempParameter;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            tempParameter = e.Parameter;
            await ConfigurePath();
            ConfigureDB();
            ApplicationSetting.Updated = ApplicationInfo.ApplicationVersion;
        }

        private async void Login_Tapped(object sender, TappedRoutedEventArgs e)
        {
            accountIcon.IsTapEnabled = false;
            accountStatu.Text = "Logging in";
            if (await GraphService.OneDriveLogin())
            {
                string name = await GraphService.GetDisplayName();
                string email = await GraphService.GetPrincipalName();
                BitmapImage image = new BitmapImage();
                image.UriSource = new Uri("ms-appx:///Images/ResearchFlow_logo.jpg");
                accountName.Text = name;
                accountStatu.Text = email;
                accountIcon.ProfilePicture = image;

                ApplicationSetting.AccountName = email;

                if (await ConfigureFile())
                    this.Frame.Navigate(typeof(MainPage), tempParameter);
            }
            else
            {
                accountStatu.Text = "Please login again";
                accountIcon.IsTapEnabled = true;
            }
        }

        private async Task ConfigurePath()
        {
            await LocalStorage.GetDataFolderAsync();
            await LocalStorage.GetLogFolderAsync();
            await LocalStorage.GetNoteFolderAsync();
            await LocalStorage.GetPaperFolderAsync();
            configState.Text += "\nAll of the folders are prepared.\n";
        }

        private void ConfigureDB()
        {
            FileList.DBInitializeTrace();
            FileList.DBInitializeList();
            Crawlable.DBInitialize();
            Feed.DBInitialize();
            Paper.DBInitialize();
            configState.Text += "\nAll of the databases are initialized.\n";
        }

        private async Task<bool> ConfigureFile()
        {
            configState.Text += "\nSyncing files with OneDrive...\n";
            try
            {
                syncStatu.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Synchronization.SyncProgressChanged += Synchronization_SyncProgressChanged;
                await Synchronization.DownloadAll();
                Synchronization.SyncProgressChanged -= Synchronization_SyncProgressChanged;
                configState.Text += "\nSync successfully.\n";
                ApplicationSetting.Configured = "true";         
                return true;
            }
            catch (Exception ex)
            {
                Synchronization.SyncProgressChanged -= Synchronization_SyncProgressChanged;
                configState.Text += "\nCan't make it, since " + ex.Message + "\n";
                configState.Text += "\nPlease sync again.\n";
                return false;
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

        private void SkipAccount(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), tempParameter);
        }
    }

}
