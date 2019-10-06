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

        public static async Task<StorageFolder> GetDataAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
        }

        public static string TryGetDataPath()
        {
            return ApplicationData.Current.LocalFolder.Path + "\\" + ApplicationSetting.AccountName + "\\Data";
        }

        // for general logs and filetrace
        public static async Task<StorageFolder> GetLogAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Log", CreationCollisionOption.OpenIfExists);
        }

        public static string TryGetLogPath()
        {
            return ApplicationData.Current.LocalFolder.Path + "\\" + ApplicationSetting.AccountName + "\\Log";
        }

        // for drawable notes
        public static async Task<StorageFolder> GetNoteAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Note", CreationCollisionOption.OpenIfExists);
        }

        public static string TryGetNotePath()
        {
            return ApplicationData.Current.LocalFolder.Path + "\\" + ApplicationSetting.AccountName + "\\Note";
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
            //AddFileList(folder.Name, name);
            //AddFileTrace(folder.Name, name);
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
            //AddFileTrace(folder.Name, name);
            //RemoveFileList(folder.Name, name);
            // or FileList.DBUpdateTrace
            FileList.DBInsertTrace(folder.Name, name);
            FileList.DBDeleteList(folder.Name, name);

        }

        public static async void GeneralLogAsync<T>(string name, string line) where T : class
        {
            StorageFile file = await (await GetLogAsync()).CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
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

        public static async void AddFileTrace(string position, string name)
        {
            List<FileList> trace;
            StorageFile file = await (await GetLogAsync()).CreateFileAsync("filetrace",
                CreationCollisionOption.OpenIfExists);
            trace = SerializeHelper.DeserializeJsonToObject<List<FileList>>(await FileIO.ReadTextAsync(file));
            if (trace == null)
                trace = new List<FileList>();

            // check the existing item
            int traceIndex = -1;
            foreach (FileList item in trace)
            {
                if (item.FileName == name && item.FilePosition == position)
                    traceIndex = trace.IndexOf(item);
            }
            if (traceIndex >= 0)
            {
                trace[traceIndex].DateModified = DateTime.Now;
            }
            else
                trace.Add(new FileList
                {
                    FileName = name,
                    FilePosition = position,
                    DateModified = DateTime.Now,
                });

            await FileIO.WriteTextAsync(file, SerializeHelper.SerializeToJson(trace));
        }

        public static async void AddFileList(string position, string name)
        {
            List<FileList> list;
            // multi-users
            StorageFile file = await GetRoamingFolder().CreateFileAsync(ApplicationSetting.AccountName + ".filelist",
                CreationCollisionOption.OpenIfExists);
            list = SerializeHelper.DeserializeJsonToObject<List<FileList>>(await FileIO.ReadTextAsync(file));
            if (list == null)
                list = new List<FileList>();

            // check the existing item
            int listIndex = -1;
            foreach (FileList item in list)
            {
                if (item.FileName == name && item.FilePosition == position)
                    listIndex = list.IndexOf(item);
            }
            if (listIndex < 0)
            {
                list.Add(new FileList
                {
                    FileName = name,
                    FilePosition = position,
                    DateModified = DateTime.Now
                }); ;
            }

            await FileIO.WriteTextAsync(file, SerializeHelper.SerializeToJson(list));
        }

        public static async void RemoveFileList(string position, string name)
        {
            List<FileList> list;
            // multi-users
            StorageFile file = await GetRoamingFolder().CreateFileAsync(ApplicationSetting.AccountName + ".filelist",
                CreationCollisionOption.OpenIfExists);
            list = SerializeHelper.DeserializeJsonToObject<List<FileList>>(await FileIO.ReadTextAsync(file));
            if (list == null)
                list = new List<FileList>();

            // check the existing item
            int listIndex = -1;
            foreach (FileList item in list)
            {
                if (item.FileName == name && item.FilePosition == position)
                    listIndex = list.IndexOf(item);
            }
            if (listIndex >= 0)
            {
                list.RemoveAt(listIndex);
            }

            await FileIO.WriteTextAsync(file, SerializeHelper.SerializeToJson(list));
        }

        #endregion

    }

}

