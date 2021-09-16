using ICSharpCode.SharpZipLib.Zip;
using LogicService.Application;
using LogicService.Data;
using LogicService.Helper;
using LogicService.Security;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace LogicService.Storage
{
    public class LocalStorage
    {

        #region folder

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

        public static async Task<StorageFolder> GetPictureLibrary()
        {
            return await KnownFolders.PicturesLibrary.CreateFolderAsync("Research Flow", CreationCollisionOption.OpenIfExists); ;
        }

        public static async Task<StorageFolder> GetDocumentLibrary()
        {
            return await KnownFolders.DocumentsLibrary.CreateFolderAsync("Research Flow", CreationCollisionOption.OpenIfExists); ;
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
            return await (await GetDocumentLibrary()).CreateFolderAsync("Note", CreationCollisionOption.OpenIfExists);
        }

        public async static Task<StorageFolder> GetPaperFolderAsync()
        {
            return await (await GetDocumentLibrary()).CreateFolderAsync("Paper", CreationCollisionOption.OpenIfExists);
        }

        #endregion

        #region general / core

        public static async Task<string> GeneralReadAsync(StorageFolder folder, string name)
        {
            StorageFile file = await folder.GetFileAsync(name);
            return await FileIO.ReadTextAsync(file);
        }

        public static async void GeneralWriteAsync(StorageFolder folder, string name, string content, bool record = true)
        {
            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(file, content);
            if (record)
            {
                FileList.DBInsertList(folder.Name, name);
                FileList.DBInsertTrace(folder.Name, name);
            }
        }

        public static async void GeneralDeleteAsync(StorageFolder folder, string name, bool record = true)
        {
            // create then delete, equils to get and delete
            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            await file.DeleteAsync();
            if (record)
                FileList.DBDeleteList(folder.Name, name);
        }

        public static async void GeneralLogAsync<T>(string name, string line) where T : class
        {
            try
            {
                StorageFile file = await (await GetLogFolderAsync()).CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
                await FileIO.AppendTextAsync(file,
                                    "[" + DateTime.Now.ToString() + "]" + typeof(T).Name + " : " + line + "\n");
            }
            catch (Exception ex)
            {
                ApplicationMessage.SendMessage(new ShortMessage { Title = "StorageException", Content = ex.Message, Time = DateTimeOffset.Now},
                    ApplicationMessage.MessageType.InApp);
            }
        }

        public async static void Compression(StorageFolder origin, StorageFolder target)
        {
            string temp = HashEncode.GetRandomValue();
            using (ZipFile zip = ZipFile.Create(GetTemporaryFolder().Path + "\\" + temp))
            {
                zip.BeginUpdate();
                foreach (StorageFile file in await origin.GetFilesAsync())
                {
                    zip.Add(origin.Path + "\\" + file.Name, file.Name);
                }
                zip.CommitUpdate();
            }

            await (await GetTemporaryFolder().GetFileAsync(temp)).CopyAsync(target, "Research Flow " + origin.Name + ".zip", NameCollisionOption.GenerateUniqueName);
        }

        public async static void UnCompression(StorageFile origin, StorageFolder target)
        {
            StorageFile temp = await origin.CopyAsync(GetTemporaryFolder(), HashEncode.GetRandomValue());
            new FastZip().ExtractZip(temp.Path, target.Path, "");
        }

        #endregion

        #region custom

        public static void WriteJson(StorageFolder folder, string name, object o, bool record = true)
        {
            string json = SerializeHelper.SerializeToJson(o);
            GeneralWriteAsync(folder, name, json, record);
        }

        public static async Task<T> ReadJsonAsync<T>(StorageFolder folder, string name) where T : class
        {
            return SerializeHelper.DeserializeJsonToObject<T>(await GeneralReadAsync(folder, name));
        }

        #endregion

    }

}

