using LogicService.Services;
using LogicService.Storage;
using System;
using Windows.Data.Json;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

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
            ConfigureLocalStorage();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // get configured in term of navigation
            ApplicationData.Current.LocalSettings.Values["Configured"] = 1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        #region local and cloud Storage

        private async void ConfigureLocalStorage()
        {
            await LocalStorage.GetAppPhotosAsync();
            await LocalStorage.GetFeedsAsync();
            await LocalStorage.GetSettingsAsync();
        }

        #endregion

        #region Task



        #endregion

    }
}
