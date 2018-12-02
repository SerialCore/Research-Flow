using LogicService.Objects;
using LogicService.Security;
using LogicService.Services;
using LogicService.Storage;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ApplicationService.ContainsKey("AccountName"))
                Login_Tapped(null, null);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ApplicationService.Configured = 1;
        }

        private async void Login_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (await GraphService.ServiceLogin())
            {
                accountStatu.Text = await GraphService.GetPrincipalName();
                ApplicationService.AccountName = await GraphService.GetPrincipalName();
                ConfigureFile();
            }
            else
            {
                configState.Text = "Fail to log in, please try again.";
            }
        }

        private async void ConfigureFile()
        {
            configState.Text = "Acquiring files from OneDrive...";
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    bool IsDownloaded = await Synchronization.DownloadAll();
                    if (IsDownloaded)
                    {
                        configState.Text += "\nAcquire files successfully.";
                        configState.Text += "\nNow enjoy this application.";
                    }
                    else
                    {
                        configState.Text += "\nCan't make it, but still try using.";
                    }

                    finish_config.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                configState.Text += "\nFail: " + ex.Message;
            }
        }

        private void Finish_config_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

    }
}
