using System;
using System.Collections.Generic;
using System.Linq;
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

            WebTokenRequest request = new WebTokenRequest(provider, scope, "000000004C209B64");
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

        public static string OneDrive
        {
            get
            {
                return "onedrive.readwrite offline_access";
            }
        }

        public static string Common
        {
            get
            {
                return "wl.basic wl.emails wl.photos onedrive.readwrite offline_access";
            }
        }

    }
}
