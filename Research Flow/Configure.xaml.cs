﻿using LogicService.Application;
using LogicService.Objects;
using LogicService.Services;
using LogicService.Storage;
using System;
using System.Threading.Tasks;
using Windows.Storage;
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
            accountIcon.IsTapEnabled = false;
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

                if (ApplicationInfo.IsFirstUse || !ApplicationSetting.ContainKey("Configured"))
                {
                    if (await ConfigureFile()) // and then, check db
                        ConfigureDB();
                }

                // make sure there will be an user folder and user data
                if (ApplicationSetting.ContainKey("AccountName") && ApplicationSetting.ContainKey("Configured"))
                {
                    await Task.Delay(1000);
                    this.Frame.Navigate(typeof(MainPage), tempParameter);
                }
            }
            else
            {
                accountStatu.Text = "Please login again";
                accountIcon.IsTapEnabled = true;
                // then navigate or not, give an option
                this.Frame.Navigate(typeof(MainPage), tempParameter);
            }
        }

        private async Task<bool> ConfigureFile()
        {
            configState.Text = "\nSyncing files with OneDrive...\n";
            try
            {
                await Synchronization.DownloadAll();
                configState.Text += "\nSync successfully.\n";
                ApplicationSetting.Configured = "true";
                return true;
            }
            catch (Exception ex)
            {
                // load default files
                configState.Text += "\nCan't make it, since " + ex.Message + "\n";
                configState.Text += "\nPlease login again.\n";
                return false;
            }
        }

        private void ConfigureDB()
        {
            Crawlable.DBInitialize();
            FeedItem.DBInitialize();
            // in future updates, some alter commands may be wrote here
        }

    }

}
