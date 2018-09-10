using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Storage
{
    public class Synchronization
    {

        public static void ScanChanges()
        {
            // especially for the case when onedrive files were deleted
        }

        public static async Task<bool> UploadAll()
        {
            try
            {
                foreach(var item in await (await LocalStorage.GetPhotosAsync()).GetFilesAsync())
                {
                    await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetPhotosAsync(), item);
                }
                foreach (var item in await (await LocalStorage.GetDataAsync()).GetFilesAsync())
                {
                    await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetDataAsync(), item);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async static Task<bool> DownloadAll()
        {
            bool sign = false;
            try
            {
                foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetPhotosAsync()))
                {
                    await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetPhotosAsync(),
                        await LocalStorage.GetPhotosAsync(), item.Name);
                    sign = true;
                }
                foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetDataAsync()))
                {
                    await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetDataAsync(),
                        await LocalStorage.GetDataAsync(), item.Name);
                    sign = true;
                }
                return sign;
            }
            catch
            {
                return sign;
            }
        }

    }
}
