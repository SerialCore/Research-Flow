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
            Login_Tapped(null, null);
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
                configState.Text = "\nFail to log in, please try again.\n";
            }
        }

        private async void ConfigureFile()
        {
            configState.Text = "Acquiring files from OneDrive...\n";
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    bool IsSynced = await Synchronization.ScanChanges();
                    if (IsSynced)
                    {
                        configState.Text += "\nSync files successfully.\n";
                        configState.Text += "\nNow enjoy this application.\n";
                    }
                    else
                    {
                        configState.Text += "\nCan't make it, but still try using.\n";
                    }

                    finish_config.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                configState.Text += "\nFail: " + ex.Message + "\n";
            }
        }

        private void Finish_config_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

    }
}
