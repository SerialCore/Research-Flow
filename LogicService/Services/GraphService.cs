using LogicService.Application;
using Microsoft.Toolkit.Services.MicrosoftGraph;
using Microsoft.Toolkit.Services.OneDrive;
using Microsoft.Toolkit.Services.Services.MicrosoftGraph;
using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace LogicService.Services
{
    public class GraphService
    {

        private static MicrosoftGraphUserService User = null;

        public static bool IsConnected => OneDriveService.Instance.Provider.IsAuthenticated;

        public static bool IsNetworkAvailable => NetworkInterface.GetIsNetworkAvailable();

        public async static Task<bool> OneDriveLogin()
        {
            OneDriveService.Instance.Initialize("3bd1af71-d8ad-41f8-b1c9-22bef7a7028a",
                new string[] { MicrosoftGraphScope.FilesReadWriteAll });

            try
            {
                bool signed = await OneDriveService.Instance.LoginAsync();
                if (signed) User = OneDriveService.Instance.Provider.User;
                return signed;
            }
            catch
            {
                return false;
            }
        }

        public async static void OneDriveLogout()
        {
            await OneDriveService.Instance.LogoutAsync();
            ApplicationSetting.RemoveKey("AccountName");
            User = null;
        }

        public async static Task<bool> GraphLogin()
        {
            MicrosoftGraphService.Instance.Initialize("3bd1af71-d8ad-41f8-b1c9-22bef7a7028a", MicrosoftGraphEnums.ServicesToInitialize.OneDrive | MicrosoftGraphEnums.ServicesToInitialize.UserProfile,
                new string[] { MicrosoftGraphScope.UserRead });

            try
            {
                bool signed = await MicrosoftGraphService.Instance.LoginAsync();
                if (signed) User = MicrosoftGraphService.Instance.User;
                return signed;
            }
            catch
            {
                return false;
            }
        }

        public async static void GraphLogout()
        {
            await MicrosoftGraphService.Instance.Logout();
            User = null;
        }

        public async static Task<string> GetDisplayName()
        {
            return (await User.GetProfileAsync()).GivenName;
        }

        public async static Task<string> GetPrincipalName()
        {
            return (await User.GetProfileAsync()).UserPrincipalName;
        }

    }
}
