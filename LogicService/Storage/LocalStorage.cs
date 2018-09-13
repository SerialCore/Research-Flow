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
using ICSharpCode.SharpZipLib.Zip;

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

        // synchronization
        public static async Task<StorageFolder> GetPhotoAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Photo", CreationCollisionOption.OpenIfExists);
        }

        // synchronization
        public static async Task<StorageFolder> GetDataAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetFeedAsync()
        {
            return await (await GetUserFolderAsync()).CreateFolderAsync("Feed", CreationCollisionOption.OpenIfExists);
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
        }

        // files package will be with synchronization together
        public static async Task<bool> CombineFiles(StorageFolder origin, List<StorageFile> files, StorageFolder target, string name)
        {
            ////压缩文件名为空时使用文件夹名＋.zip
            //if (zipFilePath == string.Empty)
            //{
            //    if (dirPath.EndsWith("\\"))
            //    {
            //        dirPath = dirPath.Substring(0, dirPath.Length - 1);
            //    }
            //    zipFilePath = dirPath + ".zip";
            //}

            //try
            //{
            //    string[] filenames = Directory.GetFiles(dirPath);
            //    using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFilePath)))
            //    {
            //        s.SetLevel(9);
            //        byte[] buffer = new byte[4096];
            //        foreach (string file in filenames)
            //        {
            //            ZipEntry entry = new ZipEntry(Path.GetFileName(file));
            //            entry.DateTime = DateTime.Now;
            //            s.PutNextEntry(entry);
            //            using (FileStream fs = File.OpenRead(file))
            //            {
            //                int sourceBytes;
            //                do
            //                {
            //                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
            //                    s.Write(buffer, 0, sourceBytes);
            //                } while (sourceBytes > 0);
            //            }
            //        }
            //        s.Finish();
            //        s.Close();
            //    }
            //}
            //catch { }



            StorageFile file = await target.CreateFileAsync(name);
            await ThreadPool.RunAsync(async e =>
            {
                try
                {
                    if (target.Name.Equals("Data"))
                        await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetDataAsync(), file);
                }
                catch
                {
                    // write log
                }
            });

            return true;
        }

        public static bool SeparateFiles(StorageFolder origin, string name, StorageFolder target)
        {
            //if (zipFilePath == string.Empty)
            //{
            //    err = "压缩文件不能为空！";
            //    return false;
            //}
            //if (!File.Exists(zipFilePath))
            //{
            //    err = "压缩文件不存在！";
            //    return false;
            //}
            ////解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹
            //if (unZipDir == string.Empty)
            //    unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
            //if (!unZipDir.EndsWith("\\"))
            //    unZipDir += "\\";
            //if (!Directory.Exists(unZipDir))
            //    Directory.CreateDirectory(unZipDir);

            //try
            //{
            //    using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath)))
            //    {

            //        ZipEntry theEntry;
            //        while ((theEntry = s.GetNextEntry()) != null)
            //        {
            //            string directoryName = Path.GetDirectoryName(theEntry.Name);
            //            string fileName = Path.GetFileName(theEntry.Name);
            //            if (directoryName.Length > 0)
            //            {
            //                Directory.CreateDirectory(unZipDir + directoryName);
            //            }
            //            if (!directoryName.EndsWith("\\"))
            //                directoryName += "\\";
            //            if (fileName != String.Empty)
            //            {
            //                using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
            //                {

            //                    int size = 2048;
            //                    byte[] data = new byte[2048];
            //                    while (true)
            //                    {
            //                        size = s.Read(data, 0, data.Length);
            //                        if (size > 0)
            //                        {
            //                            streamWriter.Write(data, 0, size);
            //                        }
            //                        else
            //                        {
            //                            break;
            //                        }
            //                    }
            //                }
            //            }
            //        }//while
            //    }
            //}
            //catch { }
            return true;
        }

        #endregion

    }
}
