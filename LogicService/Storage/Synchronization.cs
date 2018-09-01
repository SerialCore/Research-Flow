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

        }

        public static void UploadAll()
        {

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
