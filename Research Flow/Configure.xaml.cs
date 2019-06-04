using LogicService.Application;
using LogicService.Services;
using LogicService.Storage;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            tempParameter = e.Parameter;
            Login_Tapped(null, null);
        }

        private async void Login_Tapped(object sender, TappedRoutedEventArgs e)
        {
            accountStatu.Text = "Logging in";
            if (await GraphService.OneDriveLogin())
            {
                string name = await GraphService.GetDisplayName();
                string email = await GraphService.GetPrincipalName();
                BitmapImage image = new BitmapImage();
                image.UriSource = new Uri("ms-appx:///Images/ResearchFlow_logo.jpg");
                accountStatu.Text = email;
                accountIcon.ProfilePicture = image;

                ApplicationSetting.AccountName = email;

                if(ApplicationInfo.IsFirstUse || !ApplicationSetting.ContainKey("Configured"))
                    await ConfigureFile();
            }
            else
            {
                accountStatu.Text = "Please login again";
            }

            // make sure there will be an user folder and user data
            if (ApplicationSetting.ContainKey("AccountName") && ApplicationSetting.ContainKey("Configured"))
            {
                await Task.Delay(1000);
                this.Frame.Navigate(typeof(MainPage), tempParameter);
            }
        }

        private async Task ConfigureFile()
        {
            configState.Text = "\nSyncing files with OneDrive...\n";
            try
            {
                await Synchronization.DownloadAll();
                configState.Text += "\nSync successfully.\n";
                ApplicationSetting.Configured = "true";
            }
            catch (Exception ex)
            {
                // load default files
                configState.Text += "\nCan't make it, since " + ex.Message + "\n";
                configState.Text += "\nPlease login again.\n";
            }
        }

    }

}
