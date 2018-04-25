using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace LogicService.Services
{
    /// <summary>
    /// Get MsaToken silently after logging in
    /// </summary>
    public class MicrosoftAccount
    {

        public static async Task<string> GetMsaTokenSilentlyAsync(string scope)
        {
            string providerId = ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"]?.ToString();
            string accountId = ApplicationData.Current.LocalSettings.Values["CurrentUserId"]?.ToString();

            if (null == providerId || null == accountId)
            {
                return null;
            }

            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId);
            WebAccount account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountId);

            WebTokenRequest request = new WebTokenRequest(provider, scope, "000000004420C07D");
            WebTokenRequestResult result = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request, account);

            if (result.ResponseStatus == WebTokenRequestStatus.UserInteractionRequired)
            {
                // Unable to get a token silently - you'll need to show the UI
                return null;
            }
            else if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                // Success
                return result.ResponseData[0].Token;
            }
            else
            {
                // Other error 
                return null;
            }
        }

        public static async Task<WebTokenRequestResult> GetMsaResultSilentlyAsync(string scope)
        {
            string providerId = ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"]?.ToString();
            string accountId = ApplicationData.Current.LocalSettings.Values["CurrentUserId"]?.ToString();

            if (null == providerId || null == accountId)
            {
                return null;
            }

            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId);
            WebAccount account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountId);

            WebTokenRequest request = new WebTokenRequest(provider, scope, "000000004420C07D");
            return await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request, account);
        }

        public static async Task<string> GetMsaJsonSilentlyAsync(string scope)
        {
            string token = await GetMsaTokenSilentlyAsync(scope);
            if (token == null)
            {
                return null;
            }

            try
            {
                string content;
                using (var client = new HttpClient())
                {
                    var infoResult = await client.GetAsync(new Uri(@"https://apis.live.net/v5.0/me?access_token=" + token));
                    content = await infoResult.Content.ReadAsStringAsync();
                }
                return content;
            }
            catch
            {
                return null;
            }
        }

        #region Msa log in or out

        private async void BuildPaneAsync(AccountsSettingsPane s, AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("CurrentUserProviderId"))
            {
                e.HeaderText = "Signing in lets you sync your preferences and settings across devices and helps us personalize your experience.";

                string providerId = ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"]?.ToString();
                string accountId = ApplicationData.Current.LocalSettings.Values["CurrentUserId"]?.ToString();

                WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId);
                WebAccount account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountId);

                WebAccountCommand command = new WebAccountCommand(account, WebAccountInvoked, SupportedWebAccountActions.Remove);

                e.WebAccountCommands.Add(command);
            }
            else
            {
                e.HeaderText = "TaskFlow works best if you're signed in.";

                var msaProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", "consumers");

                var command = new WebAccountProviderCommand(msaProvider, GetMsaTokenAsync);

                e.WebAccountProviderCommands.Add(command);

                var settingsCmd = new SettingsCommand("settings_privacy", "Privacy policy", async (x) => await Launcher.LaunchUriAsync(new Uri(@"https://privacy.microsoft.com/en-US/")));

                e.Commands.Add(settingsCmd);
            }

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
                    string accountEmail = jsonObject["emails"].GetObject()["account"].ToString().Replace("\"", "");

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
                    }
                }

                try
                {
                    await OneDriveStorage.OneDriveLogin();
                }
                catch
                {
                }
            }
        }

        private void WebAccountInvoked(WebAccountCommand command, WebAccountInvokedArgs args)
        {
            if (args.Action == WebAccountAction.Remove)
            {
                SignOutAccountAsync(command.WebAccount);
                OneDriveStorage.OneDriveLogout();
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

        #endregion

    }

    public class MsaScope
    {

        public static string Basic
        {
            get
            {
                return "wl.basic";
            }
        }

        public static string Common
        {
            get
            {
                return "wl.basic wl.emails onedrive.readwrite offline_access";
            }
        }

        public static string OneNote
        {
            get
            {
                return "wl.office.onenote_create";
            }
        }

        public static string SkyDrive
        {
            get
            {
                return "wl.skydrive_update";
            }
        }

        public static string OneDrive
        {
            get
            {
                return "onedrive.readwrite offline_access";
            }
        }

    }
}
