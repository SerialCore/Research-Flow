using LogicService.Application;
using Microsoft.Toolkit.Services.MicrosoftGraph;
using Microsoft.Toolkit.Services.OneDrive;
using Microsoft.Toolkit.Services.Services.MicrosoftGraph;
using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace LogicService.Services
{
    public class GraphService
    {

        #region profile

        private static MicrosoftGraphUserService User = null;

        public static bool IsSignedIn = false;

        public async static Task<bool> ServiceLogin()
        {
            // a specific id used for any microsoft account
            OneDriveService.Instance.Initialize("3bd1af71-d8ad-41f8-b1c9-22bef7a7028a",
                new string[] { MicrosoftGraphScope.FilesReadWriteAll });
            MicrosoftGraphService.Instance.Initialize("3bd1af71-d8ad-41f8-b1c9-22bef7a7028a", MicrosoftGraphEnums.ServicesToInitialize.OneDrive | MicrosoftGraphEnums.ServicesToInitialize.UserProfile,
                new string[] { MicrosoftGraphScope.UserRead });
            
            try
            {
                IsSignedIn = await MicrosoftGraphService.Instance.LoginAsync() && await OneDriveService.Instance.LoginAsync();
                if (IsSignedIn) User = MicrosoftGraphService.Instance.User;
                return IsSignedIn;
            }
            catch
            {
                return false;
            }
        }

        public async static void ServiceLogout()
        {
            await OneDriveService.Instance.LogoutAsync();
            ApplicationSetting.KeyRemove("AccountName");
            User = null;
            IsSignedIn = false;
        }

        public async static Task<IRandomAccessStream> GetUserPhoto()
        {
            return (IRandomAccessStream)(await User.PhotosService.GetPhotoAsync());
        }

        public async static Task<string> GetDisplayName()
        {
            return (await User.GetProfileAsync()).DisplayName;
        }

        public async static Task<string> GetPrincipalName()
        {
            return (await User.GetProfileAsync()).UserPrincipalName;
        }

        #endregion

    }
}
