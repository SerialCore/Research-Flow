using ICSharpCode.SharpZipLib.Zip;
using LogicService.Application;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace LogicService.Storage
{
    public class Synchronization
    {

        public static void ScanChanges()
        {

        }

        public async static void UploadAll()
        {
            foreach(var item in await (await LocalStorage.GetFeedAsync()).GetFilesAsync())
            {
                await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetFeedAsync(), item);
            }
            //foreach (var item in await (await LocalStorage.GetLogAsync()).GetFilesAsync())
            //{
            //    await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetLogAsync(), item);
            //}
            foreach (var item in await (await LocalStorage.GetNoteAsync()).GetFilesAsync())
            {
                await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetNoteAsync(), item);
            }
            foreach (var item in await (await LocalStorage.GetPictureAsync()).GetFilesAsync())
            {
                await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetPictureAsync(), item);
            }
        }

        public async static void DownloadAll()
        {
            foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetFeedAsync()))
            {
                await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetFeedAsync(),
                    await LocalStorage.GetFeedAsync(), item.Name);
            }
            //foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetLogAsync()))
            //{
            //    await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetLogAsync(),
            //        await LocalStorage.GetLogAsync(), item.Name);
            //}
            foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetNoteAsync()))
            {
                await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetNoteAsync(),
                    await LocalStorage.GetNoteAsync(), item.Name);
            }
            foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetPictureAsync()))
            {
                await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetPictureAsync(),
                    await LocalStorage.GetPictureAsync(), item.Name);
            }
        }

        //private static void Compression(StorageFolder origin)
        //{
        //    using (ZipFile zip = ZipFile.Create((await LocalStorage.GetDataAsync()).Path + "\\" + origin.Name))
        //    {
        //        zip.BeginUpdate();
        //        foreach (StorageFile file in await origin.GetFilesAsync())
        //        {
        //            zip.Add(origin.Path + "\\" + file.Name, file.Name);
        //        }
        //        zip.CommitUpdate();
        //    }
        //}

        //private static void UnCompression(StorageFolder target)
        //{
        //    new FastZip().ExtractZip((await LocalStorage.GetDataAsync()).Path + "\\" + target.Name, target.Path, "");
        //}

    }
}
