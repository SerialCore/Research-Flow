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
                foreach (var item in await (await LocalStorage.GetFeedsAsync()).GetFilesAsync())
                {
                    await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetFeedsAsync(), item);
                }
                foreach (var item in await (await LocalStorage.GetLearingAsync()).GetFilesAsync())
                {
                    await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetLearingAsync(), item);
                }
                foreach (var item in await (await LocalStorage.GetSettingsAsync()).GetFilesAsync())
                {
                    await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetSettingsAsync(), item);
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
            try
            {
                foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetPhotosAsync()))
                {
                    await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetPhotosAsync(),
                        await LocalStorage.GetFeedsAsync(), item.Name);
                }
                foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetFeedsAsync()))
                {
                    await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetFeedsAsync(),
                        await LocalStorage.GetFeedsAsync(), item.Name);
                }
                foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetLearingAsync()))
                {
                    await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetLearingAsync(),
                        await LocalStorage.GetFeedsAsync(), item.Name);
                }
                foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetSettingsAsync()))
                {
                    await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetSettingsAsync(),
                        await LocalStorage.GetFeedsAsync(), item.Name);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
