using LogicService.Application;
using LogicService.Services;
using LogicService.Storage;
using System;
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

        private object tempParameter;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            tempParameter = e.Parameter;
            Login_Tapped(null, null);
        }

        private async void Login_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (await GraphService.ServiceLogin())
            {
                accountStatu.Text = await GraphService.GetPrincipalName();
                configState.Text = "\nHi\t" + await GraphService.GetDisplayName() + "\n";
                ApplicationSetting.AccountName = await GraphService.GetPrincipalName();
                ConfigureFile();
            }
            else
            {
                configState.Text = "\nFail to log in, please try again.\n";
            }

            // make sure there will be an user folder
            if (ApplicationSetting.KeyContain("AccountName"))
                finish_config.IsEnabled = true;
        }

        private async void ConfigureFile()
        {
            configState.Text += "\nScanning files with OneDrive...\n";
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    bool IsSynced = await Synchronization.ScanChanges();
                    if (IsSynced)
                    {
                        configState.Text += "\nSync files successfully.\n";
                    }
                    else
                    {
                        configState.Text += "\nCan't make it, but still try using.\n";
                    }
                });
            }
            catch (Exception ex)
            {
                configState.Text += "\nFail: " + ex.Message + "\n";
            }
        }

        private void ConfigureBD()
        {

        }

        private void ConfigureSetting()
        {

        }

        private void Finish_config_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), tempParameter);
        }

    }
}
