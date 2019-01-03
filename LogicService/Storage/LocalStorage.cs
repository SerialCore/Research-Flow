using LogicService.Helper;
using LogicService.Security;
using LogicService.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
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

        // synchronization
        public static async Task<StorageFolder> GetPhotoAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Photo", CreationCollisionOption.OpenIfExists);
        }

        // synchronization
        public static async Task<StorageFolder> GetDataAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetFeedAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Feed", CreationCollisionOption.OpenIfExists);
        }

        #endregion

        #region Operator

        public static async Task<StorageFile> WriteObjectAsync(StorageFolder folder, string name, object o, string key = null)
        {
            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            string content = JsonHelper.SerializeObject(o);
            await FileIO.WriteTextAsync(file, TripleDES.Encrypt(content, key));
            ApplicationService.LocalDateModified = DateTime.Now.ToBinary();
            return file;
        }

        public static async Task<object> ReadObjectAsync<T>(StorageFolder folder, string name, string key = null) where T : class
        {
            StorageFile file = await folder.GetFileAsync(name);
            string content = await FileIO.ReadTextAsync(file);
            return JsonHelper.DeserializeJsonToObject<T>(TripleDES.Decrypt(content, key));
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

