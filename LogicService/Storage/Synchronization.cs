using ICSharpCode.SharpZipLib.Zip;
using LogicService.Application;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace LogicService.Storage
{
    public class Synchronization
    {

        public static async Task<bool> ScanChanges()
        {
            try
            {
                if ((await OneDriveStorage.GetDataAsync()).DateModified > DateTime.FromBinary((long)ApplicationSetting.LocalDateModified))
                    return await DownloadAll();
                else
                    return await UploadAll();
            }
            catch
            {
                // network
                return false;
            }
        }

        public static async Task<bool> UploadAll()
        {
            try
            {
                await Task.Run(async () =>
                {
                    Compression(await LocalStorage.GetFeedAsync());
                    await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetDataAsync(), 
                        await (await LocalStorage.GetDataAsync()).GetFileAsync("Feed"));
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async void Compression(StorageFolder origin)
        {
            using (ZipFile zip = ZipFile.Create((await LocalStorage.GetDataAsync()).Path + "\\" + origin.Name))
            {
                zip.BeginUpdate();
                foreach (StorageFile file in await origin.GetFilesAsync())
                {
                    zip.Add(origin.Path + "\\" + file.Name, file.Name);
                }
                zip.CommitUpdate();
            }
        }

        public async static Task<bool> DownloadAll()
        {
            bool sign = false;
            try
            {
                await Task.Run(async () =>
                {
                    foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetDataAsync()))
                    {
                        await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetDataAsync(),
                            await LocalStorage.GetDataAsync(), item.Name);
                        UnCompression(await LocalStorage.GetFolderAsync(item.Name));
                        sign = true;
                    }
                    if (sign == true)
                        ApplicationSetting.LocalDateModified = DateTime.Now.ToBinary();
                });
                return sign;
            }
            catch
            {
                return sign;
            }
        }

        private static async void UnCompression(StorageFolder target)
        {
            new FastZip().ExtractZip((await LocalStorage.GetDataAsync()).Path + "\\" + target.Name, target.Path, "");
        }

    }
}
