using ICSharpCode.SharpZipLib.Zip;
using LogicService.Application;
using LogicService.Helper;
using LogicService.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace LogicService.Storage
{
    public class LocalStorage
    {

        #region folder

        public static StorageFolder GetAppFolder()
        {
            return ApplicationData.Current.LocalFolder;
        }

        public static StorageFolder GetRoamingFolder()
        {
            return ApplicationData.Current.RoamingFolder;
        }

        public static StorageFolder GetTemporaryFolder()
        {
            return ApplicationData.Current.TemporaryFolder;
        }

        public async static Task<StorageFolder> GetUserFolderAsync()
        {
            return await GetAppFolder().CreateFolderAsync(ApplicationSetting.AccountName,
                    CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetFolderAsync(string folder)
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetCrawAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Craw", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetFeedAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Feed", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetLogAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Log", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetNoteAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Note", CreationCollisionOption.OpenIfExists);
        }

        #endregion

        #region general / core

        /// <summary>
        /// General read process should not be record
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<string> GeneralReadAsync(StorageFolder folder, string name)
        {
            StorageFile file = await folder.GetFileAsync(name);
            return await FileIO.ReadTextAsync(file);
        }

        /// <summary>
        /// General write process should be record
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name">there must be a file or exception</param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task<StorageFile> GeneralWriteAsync(StorageFolder folder, string name, string content)
        {
            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(file, content);
            // record
            AddFileTrace(folder.Name, name);

            return file;
        }

        /// <summary>
        /// General delete process should be record
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        public static async void GeneralDeleteAsync(StorageFolder folder, string name)
        {
            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            await file.DeleteAsync();
            // record
            AddFileTrace(folder.Name, name);
            AddRemoveList(folder.Name, name);
        }

        /// <summary>
        /// Log append process should not be record
        /// </summary>
        /// <typeparam name="T">this.GetType();</typeparam>
        /// <param name="name"></param>
        /// <param name="line">just what you did without param</param>
        /// <returns></returns>
        public static async Task<StorageFile> GeneralLogAsync<T>(string name, string line) where T : class
        {
            StorageFile file = await (await GetLogAsync()).CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            await FileIO.AppendTextAsync(file,
                                "[" + DateTime.Now.ToString() + "]" + typeof(T).FullName + " : " + line + "\n");

            return file;
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

        public static async Task<StorageFile> WriteJsonAsync(StorageFolder folder, string name, object o)
        {
            string json = SerializeHelper.SerializeToJson(o);
            return await GeneralWriteAsync(folder, name, json);
        }

        public static async Task<T> ReadJsonAsync<T>(StorageFolder folder, string name) where T : class
        {
            return SerializeHelper.DeserializeJsonToObject<T>(await GeneralReadAsync(folder, name));
        }

        public static async void AddFileTrace(string position, string name)
        {
            List<FileTrace> trace;
            StorageFile file = await (await GetLogAsync()).CreateFileAsync("filetrace",
                CreationCollisionOption.OpenIfExists);
            trace = SerializeHelper.DeserializeJsonToObject<List<FileTrace>>(await FileIO.ReadTextAsync(file));
            if (trace == null)
                trace = new List<FileTrace>();

            // check the existing item
            int traceIndex = -1;
            foreach (FileTrace item in trace)
            {
                if (item.FileName == name && item.FilePosition == position)
                    traceIndex = trace.IndexOf(item);
            }
            if (traceIndex >= 0)
            {
                trace[traceIndex].DateModified = DateTime.Now;
                trace[traceIndex].IsSynced = false;
            }
            else
                trace.Add(new FileTrace
                {
                    FileName = name,
                    FilePosition = position,
                    DateModified = DateTime.Now,
                    IsSynced = false
                });

            await FileIO.WriteTextAsync(file, SerializeHelper.SerializeToJson(trace));
        }

        public static async void AddRemoveList(string position, string name)
        {
            List<RemoveList> remove;
            StorageFile file = await GetRoamingFolder().CreateFileAsync("removelist",
                CreationCollisionOption.OpenIfExists);
            remove = SerializeHelper.DeserializeJsonToObject<List<RemoveList>>(await FileIO.ReadTextAsync(file));
            if (remove == null)
                remove = new List<RemoveList>();

            // check the existing item
            int removeIndex = -1;
            foreach (RemoveList item in remove)
            {
                if (item.FileName == name && item.FilePosition == position)
                    removeIndex = remove.IndexOf(item);
            }
            if (removeIndex >= 0)
                remove[removeIndex].Checked++;
            else
                remove.Add(new RemoveList
                {
                    FileName = name,
                    FilePosition = position,
                    Checked = 0
                });

            await FileIO.WriteTextAsync(file, SerializeHelper.SerializeToJson(remove));
        }

        #endregion

    }

}

