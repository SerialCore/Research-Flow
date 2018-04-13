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
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += BuildPaneAsync;

            ConfigureLocalStorage();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested -= BuildPaneAsync;

            // get configured in term of navigation
            ApplicationData.Current.LocalSettings.Values["Configured"] = 1;
        }

        private void NavigateMainPage()
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        #region Microsoft account log in

        private async void BuildPaneAsync(AccountsSettingsPane s, AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();

            e.HeaderText = "TaskFlow works best if you're signed in.";

            var msaProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", "consumers");

            var command = new WebAccountProviderCommand(msaProvider, GetMsaTokenAsync);

            e.WebAccountProviderCommands.Add(command);

            var settingsCmd = new SettingsCommand("settings_privacy", "Privacy policy", async (x) => await Launcher.LaunchUriAsync(new Uri(@"https://privacy.microsoft.com/en-US/")));

            e.Commands.Add(settingsCmd);

            deferral.Complete();
        }

        private async void GetMsaTokenAsync(WebAccountProviderCommand command)
        {
            WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, MsaScope.Common, "000000004420C07D");
            WebTokenRequestResult result = await WebAuthenticationCoreManager.RequestTokenAsync(request);

            if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                string token = result.ResponseData[0].Token;
                WebAccount account = result.ResponseData[0].WebAccount;
                StoreWebAccount(account);

                var restApi = new Uri(@"https://apis.live.net/v5.0/me?access_token=" + token);

                using (var client = new HttpClient())
                {
                    var infoResult = await client.GetAsync(restApi);
                    string content = await infoResult.Content.ReadAsStringAsync();

                    var jsonObject = JsonObject.Parse(content);
                    accountEmail.Text = jsonObject["emails"].GetObject()["account"].ToString().Replace("\"", "");

                    string photo = "https://apis.live.net/v5.0/" + jsonObject["id"].GetString() + "/picture";
                    HttpResponseMessage response = await client.GetAsync(new Uri(photo));
                    if (response != null && response.StatusCode == HttpStatusCode.Ok)
                    {
                        BitmapImage bitmap = new BitmapImage();
                        using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                        {
                            await response.Content.WriteToStreamAsync(stream);
                            stream.Seek(0UL);
                            bitmap.SetSource(stream);
                        }
                        accountPhoto.ProfilePicture = bitmap;
                    }
                    NavigateMainPage();
                }

                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    OneDriveStorage.OneDriveLogin();
                });
                
                signPane.Visibility = Visibility.Visible;
            }
            else
            {
                try
                {
                    InAppNotification.Show(result.ResponseError.ErrorMessage);
                }
                catch { }
            }
        }

        private void StoreWebAccount(WebAccount account)
        {
            ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"] = account.WebAccountProvider.Id;
            ApplicationData.Current.LocalSettings.Values["CurrentUserId"] = account.Id;
        }

        private void AuthenticateMicrosoft(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AccountsSettingsPane.Show();
        }

        #endregion

        #region local and cloud Storage

        private async void ConfigureLocalStorage()
        {
            await LocalStorage.GetAppPhotosAsync();
            await LocalStorage.GetFeedsAsync();
        }

        #endregion

        #region Task



        #endregion

    }
}
