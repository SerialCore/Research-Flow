using ICSharpCode.SharpZipLib.Zip;
using LogicService.Data;
using LogicService.Helper;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace LogicService.Storage
{
    public class LocalStorage
    {

        #region folder

        public static StorageFolder GetLocalFolder()
        {
            return ApplicationData.Current.LocalFolder;
        }

        public static StorageFolder GetLocalCacheFolder()
        {
            return ApplicationData.Current.LocalCacheFolder;
        }

        public static StorageFolder GetRoamingFolder()
        {
            return ApplicationData.Current.RoamingFolder;
        }

        public static StorageFolder GetTemporaryFolder()
        {
            return ApplicationData.Current.TemporaryFolder;
        }

        public static async Task<StorageFolder> GetCacheSubFolderAsync(string folder)
        {
            return await GetLocalCacheFolder().CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetDataFolderAsync()
        {
            return await GetLocalCacheFolder().CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
        }

        public static string TryGetDataPath()
        {
            return ApplicationData.Current.LocalCacheFolder.Path + "\\Data";
        }

        public static async Task<StorageFolder> GetLogFolderAsync()
        {
            return await GetLocalCacheFolder().CreateFolderAsync("Log", CreationCollisionOption.OpenIfExists);
        }

        public static string TryGetLogPath()
        {
            return ApplicationData.Current.LocalCacheFolder.Path + "\\Log";
        }

        public static async Task<StorageFolder> GetNoteFolderAsync()
        {
            return await GetLocalCacheFolder().CreateFolderAsync("Note", CreationCollisionOption.OpenIfExists);
        }

        public async static Task<StorageFolder> GetPaperFolderAsync()
        {
            return await GetLocalCacheFolder().CreateFolderAsync("Paper", CreationCollisionOption.OpenIfExists);
        }

        public async static Task<StorageFolder> GetPictureFolderAsync()
        {
            return await GetLocalCacheFolder().CreateFolderAsync("Picture", CreationCollisionOption.OpenIfExists);
        }

        public async static Task<StorageFolder> GetPictureCacheAsync()
        {
            return await GetTemporaryFolder().CreateFolderAsync("Picture", CreationCollisionOption.OpenIfExists);
        }

        #endregion

        #region general / core

        /// <summary>
        /// will not be recorded
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name">there must be a file or exception</param>
        /// <returns></returns>
        public static async Task<string> GeneralReadAsync(StorageFolder folder, string name)
        {
            // FileNotFoundException will be catched externally for some reasons
            StorageFile file = await folder.GetFileAsync(name);
            return await FileIO.ReadTextAsync(file);
        }

        /// <summary>
        /// will be recorded
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async void GeneralWriteAsync(StorageFolder folder, string name, string content)
        {
            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(file, content);
            // record
            FileList.DBInsertList(folder.Name, name);
            FileList.DBInsertTrace(folder.Name, name);
        }

        /// <summary>
        /// will be recorded
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        public static async void GeneralDeleteAsync(StorageFolder folder, string name)
        {
            // create then delete, equils to get and delete
            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            await file.DeleteAsync();
            // record
            FileList.DBDeleteList(folder.Name, name);
        }

        public static async void GeneralLogAsync<T>(string name, string line) where T : class
        {
            StorageFile file = await (await GetLogFolderAsync()).CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            await FileIO.AppendTextAsync(file,
                                "[" + DateTime.Now.ToString() + "]" + typeof(T).Name + " : " + line + "\n");
        }

        public async static void Compression(StorageFolder origin)
        {
            using (ZipFile zip = ZipFile.Create(LocalStorage.GetTemporaryFolder().Path + "\\" + origin.Name + ".zip"))
            {
                zip.BeginUpdate();
                foreach (StorageFile file in await origin.GetFilesAsync())
                {
                    zip.Add(origin.Path + "\\" + file.Name, file.Name);
                }
                zip.CommitUpdate();
            }
        }

        public static void UnCompression(StorageFolder target)
        {
            new FastZip().ExtractZip(LocalStorage.GetTemporaryFolder().Path + "\\" + target.Name + ".zip", target.Path, "");
        }

        #endregion

        #region custom

        /// <summary>
        /// will be recorded
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <param name="o"></param>
        public static void WriteJson(StorageFolder folder, string name, object o)
        {
            string json = SerializeHelper.SerializeToJson(o);
            GeneralWriteAsync(folder, name, json);
        }

        public static async Task<T> ReadJsonAsync<T>(StorageFolder folder, string name) where T : class
        {
            return SerializeHelper.DeserializeJsonToObject<T>(await GeneralReadAsync(folder, name));
        }

        #endregion

    }

}

