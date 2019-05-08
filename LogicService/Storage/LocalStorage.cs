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
            return await WriteTextAsync(folder, name, json);
        }

        public static async Task<object> ReadJsonAsync<T>(StorageFolder folder, string name) where T : class
        {
            return SerializeHelper.DeserializeJsonToObject<T>(await ReadTextAsync(folder, name));
        }

        public static async Task<string> ReadTextAsync(StorageFolder folder, string name)
        {
            StorageFile file = await folder.GetFileAsync(name);
            return await FileIO.ReadTextAsync(file);
        }

        /// <summary>
        /// General write process should be record
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task<StorageFile> WriteTextAsync(StorageFolder folder, string name, string content)
        {
            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            if (file != null)
            {
                await FileIO.WriteTextAsync(file, content);
                // record
            }
            return file;
        }

        /// <summary>
        /// General delete process should be record
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        public static async void DeleteFileAsync(StorageFolder folder, string name)
        {
            StorageFile file = await folder.GetFileAsync(name);
            if (file != null)
            {
                await file.DeleteAsync();
                // record
            }
        }

        #endregion

        #region Logging  file

        /// <summary>
        /// General append process should be record
        /// </summary>
        /// <typeparam name="T">this.GetType();</typeparam>
        /// <param name="name"></param>
        /// <param name="line">just what you did without param</param>
        /// <returns></returns>
        public static async Task<StorageFile> WriteLogAsync<T>(string name, string line) where T : class
        {
            StorageFile file = await (await LocalStorage.GetLogAsync()).CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            if (file != null)
            {
                await FileIO.AppendTextAsync(file,
                    "[" + DateTime.Now.ToString() + "]" + typeof(T).FullName + " : " + line);
                // record
            }
            return file;
        }

        #endregion

    }

}

