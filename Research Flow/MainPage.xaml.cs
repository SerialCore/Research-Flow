using LogicService.Helper;
using LogicService.Services;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Research_Flow.Pages;
using Windows.Data.Json;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += BuildPaneAsync;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested -= BuildPaneAsync;
        }

        #region NavView

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // you can also add items in code behind
            //NavView.MenuItems.Add(new NavigationViewItemSeparator());

            // set the initial SelectedItem
            foreach (NavigationViewItemBase item in NavView.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == "Overview")
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(Settings));
            }
            else
            {
                // find NavigationViewItem with Content that equals InvokedItem
                var item = sender.MenuItems.OfType<NavigationViewItem>().First(x => (string)x.Content == (string)args.InvokedItem);
                NavView_Navigate(item as NavigationViewItem);

            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(Settings));
            }
            else
            {
                NavigationViewItem item = args.SelectedItem as NavigationViewItem;
                NavView_Navigate(item);
            }
        }

        private void NavView_Navigate(NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "Overview":
                    ContentFrame.Navigate(typeof(Overview));
                    break;

                case "Contact":
                    ContentFrame.Navigate(typeof(Contact));
                    break;

                case "Topic":
                    ContentFrame.Navigate(typeof(Topic));
                    break;

                case "Search":
                    ContentFrame.Navigate(typeof(Search));
                    break;

                case "Learn":
                    ContentFrame.Navigate(typeof(Learn));
                    break;

                case "Compose":
                    ContentFrame.Navigate(typeof(Compose));
                    break;

                case "Tool":
                    ContentFrame.Navigate(typeof(Tool));
                    break;

            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (NavView.SelectedItem == NavView.SettingsItem)
            {
                ContentFrame.Navigate(typeof(Settings));
            }
            else
            {
                NavView_Navigate(NavView.SelectedItem as NavigationViewItem);
            }
        }

        private async void ScreenShot_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = new RenderTargetBitmap();
            // cache for being deleted
            StorageFile file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("TaskFlow-ScreenShot.png", CreationCollisionOption.GenerateUniqueName);
            await bitmap.RenderAsync(FullPage);
            var buffer = await bitmap.GetPixelsAsync();
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encode = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encode.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)bitmap.PixelWidth,
                    (uint)bitmap.PixelHeight,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    buffer.ToArray()
                   );
                await encode.FlushAsync();
            }

            if (file != null)
            {
                string token = await MicrosoftAccount.GetMsaTokenSilentlyAsync(MsaScope.Basic);
                if (token != null)
                {
                    // copy to OneDrive
                }
                else
                {
                    await file.CopyAsync(KnownFolders.PicturesLibrary, "TaskFlow-ScreenShot.png", NameCollisionOption.GenerateUniqueName);
                    ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(ToastGenerator.ScreenShotSaved("Pictures Library").GetXml()));
                }
            }
        }

        #endregion

        #region Msa log in or out

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
            WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, MsaScope.Basic, "000000004420C07D");
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
                    accountEmail.Text = jsonObject["emails"].GetString();

                    HttpResponseMessage response = await client.GetAsync(new Uri(jsonObject["picture"].GetString()));
                    if (response != null && response.StatusCode == HttpStatusCode.Ok)
                    {
                        using (IRandomAccessStream stream = new InMemoryRandomAccessStream())
                        {
                            await response.Content.WriteToStreamAsync(stream);
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.SetSource(stream);
                            accountPhoto.ProfilePicture = bitmap;
                        }
                    }
                }
            }
            else
            {
                try
                {
                    InAppNotification.Show(result.ResponseError.ErrorMessage);
                }
                catch
                {
                    InAppNotification.Show("Extra exception");
                }
            }
        }

        private void StoreWebAccount(WebAccount account)
        {
            ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"] = account.WebAccountProvider.Id;
            ApplicationData.Current.LocalSettings.Values["CurrentUserId"] = account.Id;
        }

        private async void SignOutAccountAsync(WebAccount account)
        {
            ApplicationData.Current.LocalSettings.Values.Remove("CurrentUserProviderId");
            ApplicationData.Current.LocalSettings.Values.Remove("CurrentUserId");
            await account.SignOutAsync();
        }

        private async void Account_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            string token = await MicrosoftAccount.GetMsaTokenSilentlyAsync(MsaScope.Basic);

            if (token != null)
            {
                string providerId = ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"]?.ToString();
                string accountId = ApplicationData.Current.LocalSettings.Values["CurrentUserId"]?.ToString();

                WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId);
                WebAccount account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountId);
                SignOutAccountAsync(account);
            }
            else
            {
                AccountsSettingsPane.Show();
            }
        }

        #endregion

    }

}
