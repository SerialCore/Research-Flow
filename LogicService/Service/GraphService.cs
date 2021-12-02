namespace LogicService.Service
{
    public class GraphService
    {

        //private static MicrosoftGraphUserService User = null;

        //public static bool IsConnected => OneDriveService.Instance.Provider.IsAuthenticated;

        //public async static Task<bool> OneDriveLogin()
        //{
        //    OneDriveService.Instance.Initialize("3bd1af71-d8ad-41f8-b1c9-22bef7a7028a",
        //        new string[] { MicrosoftGraphScope.FilesReadWriteAll });

        //    try
        //    {
        //        if (await OneDriveService.Instance.LoginAsync())
        //        {
        //            User = OneDriveService.Instance.Provider.User;
        //            return true;
        //        }
        //        else
        //            return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public async static void OneDriveLogout()
        //{
        //    await OneDriveService.Instance.LogoutAsync();
        //    ApplicationSetting.RemoveKey("AccountName");
        //    User = null;
        //}

        //public async static Task<bool> GraphLogin()
        //{
        //    MicrosoftGraphService.Instance.Initialize("3bd1af71-d8ad-41f8-b1c9-22bef7a7028a", MicrosoftGraphEnums.ServicesToInitialize.OneDrive | MicrosoftGraphEnums.ServicesToInitialize.UserProfile,
        //        new string[] { MicrosoftGraphScope.UserRead });

        //    try
        //    {
        //        bool signed = await MicrosoftGraphService.Instance.LoginAsync();
        //        if (signed) User = MicrosoftGraphService.Instance.User;
        //        return signed;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public async static void GraphLogout()
        //{
        //    await MicrosoftGraphService.Instance.Logout();
        //    User = null;
        //}

        //public async static Task<string> GetDisplayName()
        //{
        //    try
        //    {
        //        return (await User.GetProfileAsync()).DisplayName;
        //    }
        //    catch
        //    {
        //        return "No Name";
        //    }
        //}

        //public async static Task<string> GetPrincipalName()
        //{
        //    try
        //    {
        //        return (await User.GetProfileAsync()).UserPrincipalName;
        //    }
        //    catch
        //    {
        //        if (ApplicationSetting.ContainKey("AccountName"))
        //            return ApplicationSetting.AccountName;
        //        else
        //            return "No Email";
        //    }
        //}

    }
}
