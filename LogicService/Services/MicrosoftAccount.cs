using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;

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

            string content;
            using (var client = new HttpClient())
            {
                var infoResult = await client.GetAsync(new Uri(@"https://apis.live.net/v5.0/me?access_token=" + token));
                content = await infoResult.Content.ReadAsStringAsync();
            }

            return content;
        }

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
