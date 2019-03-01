using LogicService.Helper;
using LogicService.Security;
using LogicService.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace LogicService.Storage
{
    public class LocalStorage
    {

        #region Folders

        public static StorageFolder GetAppFolderAsync()
        {
            return ApplicationData.Current.LocalFolder;
        }

        public static StorageFolder GetRoamingFolderAsync()
        {
            return ApplicationData.Current.RoamingFolder;
        }

        public async static Task<StorageFolder> GetUserFolderAsync()
        {
            return await GetAppFolderAsync().CreateFolderAsync(ApplicationService.AccountName,
                CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetFolderAsync(string folder)
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);
        }

        // OneDrive
        public static async Task<StorageFolder> GetPhotoAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Photo", CreationCollisionOption.OpenIfExists);
        }

        // OneDrive (Feed)
        public static async Task<StorageFolder> GetDataAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetFeedAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Feed", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetLogAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Log", CreationCollisionOption.OpenIfExists);
        }

        #endregion

        #region Operator

        public static async Task<StorageFile> WriteJsonAsync(StorageFolder folder, string name, object o)
        {
            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, SerializeHelper.SerializeToJson(o));
            ApplicationService.LocalDateModified = DateTime.Now.ToBinary();
            return file;
        }

        public static async Task<object> ReadJsonAsync<T>(StorageFolder folder, string name) where T : class
        {
            StorageFile file = await folder.GetFileAsync(name);
            return SerializeHelper.DeserializeJsonToObject<T>(await FileIO.ReadTextAsync(file));
        }

        public static async void DeleteFile(StorageFolder folder, string name)
        {
            StorageFile file = await folder.GetFileAsync(name);
            if (file != null)
            {
                await file.DeleteAsync();
                ApplicationService.LocalDateModified = DateTime.Now.ToBinary();
            }
        }

    }

    #endregion

}

