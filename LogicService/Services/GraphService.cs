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

        private static MicrosoftGraphUserService User = null;

        public async static Task<bool> OneDriveLogin()
        {
            // a specific id used for any microsoft account
            OneDriveService.Instance.Initialize("3bd1af71-d8ad-41f8-b1c9-22bef7a7028a", new string[] { MicrosoftGraphScope.FilesReadWriteAll });

            try
            {
                bool login = await OneDriveService.Instance.LoginAsync();
                if (login) User = OneDriveService.Instance.Provider.User;
                return login;
            }
            catch
            {
                return false;
            }
        }

        public async static void OneDriveLogout()
        {
            await OneDriveService.Instance.LogoutAsync();
            User = null;
        }

        public async static Task<IRandomAccessStream> GetUserPhoto()
        {
            return await User.GetPhotoAsync();
        }

        public async static Task<string> GetDisplayName()
        {
            return (await User.GetProfileAsync()).DisplayName;
        }
    }
}
