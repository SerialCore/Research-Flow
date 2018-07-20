using LogicService.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LogicService.Storage
{
    public class LocalStorage
    {

        private static readonly LocalObjectStorageHelper localObject = new LocalObjectStorageHelper();

        public static StorageFolder GetAppFolderAsync()
        {
            return ApplicationData.Current.LocalFolder;
        }

        public async static Task<StorageFolder> GetUserFolderAsync()
        {
            return await GetAppFolderAsync().CreateFolderAsync(ApplicationData.Current.LocalSettings.Values["AccountName"] as string, 
                CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetPhotosAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Photos",CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetFeedsAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Feeds", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetSettingsAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);
        }


        public static async Task<StorageFile> WriteObjectAsync(string path, Type o)
        {
            return await localObject.SaveFileAsync<Type>(path, o);
        }

        public static async Task<Type> ReadObjectAsync(string path, Type t)
        {
            return await localObject.ReadFileAsync<Type>(path, t);
        }

    }
}
