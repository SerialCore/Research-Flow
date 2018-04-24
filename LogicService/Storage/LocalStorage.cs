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
            // discrete folders for different users
            return ApplicationData.Current.LocalFolder;
        }

        public static async Task<StorageFolder> GetAppPhotosAsync()
        {
            return await GetAppFolderAsync().CreateFolderAsync("Photos",CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetFeedsAsync()
        {
            return await GetAppFolderAsync().CreateFolderAsync("Feeds", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetSettingsAsync()
        {
            return await GetAppFolderAsync().CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);
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
