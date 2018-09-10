using LogicService.Helper;
using LogicService.Security;
using LogicService.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;

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
            return await GetAppFolderAsync().CreateFolderAsync((ApplicationData.Current.LocalSettings.Values["AccountName"] as string), 
                CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetPhotosAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Photos", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetFeedsAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Feeds", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetLearingAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Learing", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetSettingsAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);
        }

        #endregion

        #region Operator

        private static readonly LocalObjectStorageHelper localObject = new LocalObjectStorageHelper();

        public static async Task<StorageFile> WriteTypeAsync(string path, Type o)
        {
            return await localObject.SaveFileAsync<Type>(path, o);
        }

        public static async Task<Type> ReadTypeAsync(string path, Type t)
        {
            return await localObject.ReadFileAsync<Type>(path, t);
        }

        public static async Task<StorageFile> WriteObjectAsync(StorageFolder folder, string name, object o, string key = null)
        {
            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            string content = JsonHelper.SerializeObject(o);
            await FileIO.WriteTextAsync(file, TripleDES.Encrypt(content, key));
            await ThreadPool.RunAsync(async e =>
            {
                try
                {
                    if (folder.Name.Equals("Feeds"))
                        await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetFeedsAsync(), file);
                    else
                        await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetSettingsAsync(), file);
                }
                catch
                {
                    // write log
                }
            });
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
                await file.DeleteAsync();
            await ThreadPool.RunAsync(async e =>
            {
                try
                {
                    if (folder.Name.Equals("Feeds"))
                        OneDriveStorage.DeleteFileAsync(await OneDriveStorage.GetFeedsAsync(),
                            await OneDriveStorage.RetrieveFileAsync(await OneDriveStorage.GetFeedsAsync(), file.Name));
                    else
                        OneDriveStorage.DeleteFileAsync(await OneDriveStorage.GetSettingsAsync(),
                            await OneDriveStorage.RetrieveFileAsync(await OneDriveStorage.GetSettingsAsync(), file.Name));
                }
                catch
                {
                    // write log
                }
            });
        }

        #endregion

    }
}
