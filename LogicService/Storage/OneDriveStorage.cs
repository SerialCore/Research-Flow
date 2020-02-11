using Microsoft.Toolkit.Services.OneDrive;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using static Microsoft.Toolkit.Services.MicrosoftGraph.MicrosoftGraphEnums;

namespace LogicService.Storage
{
    public class OneDriveStorage
    {

        #region folder

        public static async Task<OneDriveStorageFolder> GetAppFolderAsync()
        {
            return await OneDriveService.Instance.AppRootFolderAsync();
        }

        public static async Task<OneDriveStorageFolder> GetFolderAsync(string folder)
        {
            try
            {
                return await RetrieveSubFolderAsync(await GetAppFolderAsync(), folder);
            }
            catch
            {
                return await CreateFolderAsync(await GetAppFolderAsync(), folder);
            }
        }

        public static async Task<OneDriveStorageFolder> GetDataAsync()
        {
            try
            {
                return await RetrieveSubFolderAsync(await GetAppFolderAsync(), "Data");
            }
            catch
            {
                return await CreateFolderAsync(await GetAppFolderAsync(), "Data");
            }
        }

        public static async Task<OneDriveStorageFolder> GetNoteAsync()
        {
            try
            {
                return await RetrieveSubFolderAsync(await GetAppFolderAsync(), "Note");
            }
            catch
            {
                return await CreateFolderAsync(await GetAppFolderAsync(), "Note");
            }
        }

        public static async Task<OneDriveStorageFolder> GetPictureAsync()
        {
            try
            {
                return await RetrieveSubFolderAsync(await GetAppFolderAsync(), "Picture");
            }
            catch
            {
                return await CreateFolderAsync(await GetAppFolderAsync(), "Picture");
            }
        }

        public static async Task<OneDriveStorageFolder> GetPaperAsync()
        {
            try
            {
                return await RetrieveSubFolderAsync(await GetAppFolderAsync(), "Paper");
            }
            catch
            {
                return await CreateFolderAsync(await GetAppFolderAsync(), "Paper");
            }
        }

        public static async Task<OneDriveStorageFolder> CreateFolderAsync(OneDriveStorageFolder folder, string foldername)
        {
            return await folder.StorageFolderPlatformService.CreateFolderAsync(foldername, CreationCollisionOption.OpenIfExists);
        }

        public static async Task<OneDriveStorageFolder> RetrieveSubFolderAsync(OneDriveStorageFolder folder, string path)
        {
            return await folder.GetFolderAsync(path);
        }

        public static async Task<bool> MoveFolderAsync(OneDriveStorageFolder folder, OneDriveStorageFolder target, string newName = null)
        {
            return await folder.MoveAsync(target, newName);
        }

        public static async Task<bool> CopyFolderAsync(OneDriveStorageFolder folder, OneDriveStorageFolder target, string newName = null)
        {
            return await folder.CopyAsync(target, newName);
        }

        public static async Task<OneDriveStorageFolder> RenameFolderAsync(OneDriveStorageFolder folder, string newName)
        {
            return await folder.RenameAsync(newName);
        }

        #endregion

        #region read / write

        public static async Task<BitmapImage> GetFileThumbnails(OneDriveStorageFile file)
        {
            var stream = await file.StorageItemPlatformService.GetThumbnailAsync(ThumbnailSize.Large);
            BitmapImage bmp = new BitmapImage();
            await bmp.SetSourceAsync(stream as IRandomAccessStream);

            return bmp;
        }

        public static async Task<OneDriveStorageFile> CreateFileAsync(OneDriveStorageFolder folder, StorageFile file)
        {
            OneDriveStorageFile fileCreated = null;
            // Create new file
            if (file != null)
            {
                using (var localStream = await file.OpenReadAsync())
                {
                    var prop = await file.GetBasicPropertiesAsync();
                    if (prop.Size >= 4 * 1024 * 1024)
                        fileCreated = await folder.StorageFolderPlatformService.UploadFileAsync(file.Name, localStream, CreationCollisionOption.ReplaceExisting, 320 * 1024);
                    else
                        fileCreated = await folder.StorageFolderPlatformService.CreateFileAsync(file.Name, CreationCollisionOption.ReplaceExisting, localStream);
                }
            }
            return fileCreated;
        }

        public static async Task<StorageFile> DownloadFileAsync(OneDriveStorageFolder folder, StorageFolder target, string fileName)
        {
            // Download a file and save the content in a local file
            var remoteFile = await folder.GetFileAsync(fileName);
            
            StorageFile myLocalFile = null;
            using (var remoteStream = await remoteFile.StorageFilePlatformService.OpenAsync() as IRandomAccessStream)
            {
                byte[] buffer = new byte[remoteStream.Size];
                var localBuffer = await remoteStream.ReadAsync(buffer.AsBuffer(), (uint)remoteStream.Size, InputStreamOptions.ReadAhead);
                myLocalFile = await target.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                using (var localStream = await myLocalFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await localStream.WriteAsync(localBuffer);
                    await localStream.FlushAsync();
                }
            }

            return myLocalFile;
        }

        #endregion

    }

}
