using LogicService.Application;
using LogicService.Helper;
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

        public static async Task<StorageFolder> GetFolderAsync(string folder)
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);
        }

        public async static Task<StorageFolder> GetUserFolderAsync()
        {
            return await GetAppFolderAsync().CreateFolderAsync(ApplicationSetting.AccountName,
                    CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetPictureAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Picture", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetFeedAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Feed", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetNoteAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Note", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetLogAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Log", CreationCollisionOption.OpenIfExists);
        }

        #endregion

        #region Common file

        public static async Task<StorageFile> WriteJsonAsync(StorageFolder folder, string name, object o)
        {
            string json = SerializeHelper.SerializeToJson(o);
            return await WriteText(folder, name, json);
        }

        public static async Task<object> ReadJsonAsync<T>(StorageFolder folder, string name) where T : class
        {
            return SerializeHelper.DeserializeJsonToObject<T>(await ReadText(folder, name));
        }

        public static async Task<StorageFile> WriteText(StorageFolder folder, string name, string content)
        {
            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            if (file != null)
            {
                await FileIO.WriteTextAsync(file, content);
                // record
                ApplicationSetting.LocalDateModified = DateTime.Now.ToBinary();
            }
            return file;
        }

        public static async Task<string> ReadText(StorageFolder folder, string name)
        {
            StorageFile file = await folder.GetFileAsync(name);
            return await FileIO.ReadTextAsync(file);
        }

        public static async void DeleteFile(StorageFolder folder, string name)
        {
            StorageFile file = await folder.GetFileAsync(name);
            if (file != null)
            {
                await file.DeleteAsync();
                // record
                ApplicationSetting.LocalDateModified = DateTime.Now.ToBinary();
            }
        }

        #endregion

        #region Logging  file



        #endregion

    }

}

