using ICSharpCode.SharpZipLib.Zip;
using LogicService.Application;
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

        public static StorageFolder GetPictureLibrary()
        {
            return KnownFolders.SavedPictures;
        }

        public static async Task<StorageFolder> GetNoteFolderAsync()
        {
            return await GetLocalFolder().CreateFolderAsync("note", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetPaperFolderAsync()
        {
            return await GetLocalFolder().CreateFolderAsync("paper", CreationCollisionOption.OpenIfExists);
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

        public static async Task<StorageFile> GeneralCreateAsync(StorageFolder folder, string name)
        {
            return await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
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
                StorageFile file = await GetTemporaryFolder().CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
                await FileIO.AppendTextAsync(file, "[" + DateTime.Now.ToString() + "]" + typeof(T).Name + "\t" + line + "\n");
            }
            catch (Exception ex)
            {
                ApplicationMessage.SendMessage(new MessageEventArgs { Title = "StorageException", Content = ex.Message, Type = MessageType.InApp, Time = DateTimeOffset.Now });
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

