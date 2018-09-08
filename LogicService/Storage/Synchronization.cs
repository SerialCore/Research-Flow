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
            // especially for the case when onedrive files were deleted manually
        }

        public static async Task<bool> UploadAll()
        {
            try
            {
                foreach(var item in await (await LocalStorage.GetFeedsAsync()).GetFilesAsync())
                {
                    await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetFeedsAsync(), item);
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
                foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetFeedsAsync()))
                {
                    await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetFeedsAsync(),
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
