using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.Services.OneDrive;
using static Microsoft.Toolkit.Uwp.Services.OneDrive.OneDriveEnums;

#pragma warning disable 618

namespace LogicService.Storage
{
    public class OneDriveStorage
    {

        /// <summary>
        /// Login
        /// </summary>
        public async static Task<bool> OneDriveLogin()
        {
            // a specific id used for any microsoft account
            //OneDriveService.Instance.Initialize("000000004420C07D", new string[] { "onedrive.readwrite offline_access" });
            //OneDriveService.Instance.Initialize("000000004420C07D", AccountProviderType.Msa, OneDriveScopes.OfflineAccess | OneDriveScopes.ReadWrite);

            // OnLineID means microsoft account associated with current Windows device
            OneDriveService.Instance.Initialize(Microsoft.OneDrive.Sdk.OnlineIdAuthenticationProvider.PromptType.DoNotPrompt);
            try
            {
                return await OneDriveService.Instance.LoginAsync() ? true : false;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Logout
        /// </summary>
        public async static void OneDriveLogout()
        {
            await OneDriveService.Instance.LogoutAsync();
        }

        #region Get folder

        /// <summary>
        /// Get the app root folder
        /// </summary>
        /// <returns>AppRoot</returns>
        public static async Task<OneDriveStorageFolder> GetAppFolderAsync()
        {
            return await OneDriveService.Instance.AppRootFolderAsync();
        }

        /// <summary>
        /// Get the photos folder in app root
        /// </summary>
        /// <returns>Photos</returns>
        public static async Task<OneDriveStorageFolder> GetAppPhotosAsync()
        {
            try
            {
                return await RetrieveSubFolderAsync(await GetAppFolderAsync(), "Photos");
            }
            catch
            {
                return await CreateFolderAsync(await GetAppFolderAsync(), "Photos");
            }
        }

        /// <summary>
        /// Get the feed folder in app root
        /// </summary>
        /// <returns>Feed</returns>
        public static async Task<OneDriveStorageFolder> GetFeedsAsync()
        {
            try
            {
                return await RetrieveSubFolderAsync(await GetAppFolderAsync(), "Feeds");
            }
            catch
            {
                return await CreateFolderAsync(await GetAppFolderAsync(), "Feeds");
            }
        }

        /// <summary>
        /// Get the settings folder in app root
        /// </summary>
        /// <returns>Feed</returns>
        public static async Task<OneDriveStorageFolder> GetSettingsAsync()
        {
            try
            {
                return await RetrieveSubFolderAsync(await GetAppFolderAsync(), "Settings");
            }
            catch
            {
                return await CreateFolderAsync(await GetAppFolderAsync(), "Settings");
            }
        }

        #endregion

        #region Operation

        /// <summary>
        /// List the Items from the current folder
        /// </summary>
        /// <param name="folder">current folder</param>
        /// <param name="top">top</param>
        /// <param name="order">order type</param>
        /// /// <param name="filter">filter</param>
        /// <returns></returns>
        public static async Task<IReadOnlyList<OneDriveStorageItem>> RetrieveFilesAsync(OneDriveStorageFolder folder, int top, OrderBy order, string filter = null)
        {
            // List the Items from the current folder
            var OneDriveItems = await folder.GetItemsAsync(top, order, filter);
            do
            {
                // Get the next page of items
                OneDriveItems = await folder.NextItemsAsync();
            }
            while (OneDriveItems != null);

            return OneDriveItems;
        }

        /// <summary>
        /// Create a folder in current folder
        /// </summary>
        /// <param name="folder">current folder</param>
        /// <param name="foldername">folder name</param>
        /// <returns>new folder</returns>
        public static async Task<OneDriveStorageFolder> CreateFolderAsync(OneDriveStorageFolder folder, string foldername)
        {
            return await folder.CreateFolderAsync(foldername, CreationCollisionOption.GenerateUniqueName);
        }

        /// <summary>
        /// Retrieve a sub folder in current folder
        /// </summary>
        /// <param name="folder">current folder</param>
        /// <param name="path">relative path</param>
        /// <returns>sub folder</returns>
        public static async Task<OneDriveStorageFolder> RetrieveSubFolderAsync(OneDriveStorageFolder folder, string path)
        {
            return await folder.GetFolderAsync(path);
        }

        /// <summary>
        /// Move folder
        /// </summary>
        /// <param name="folder">current folder</param>
        /// <param name="target">target</param>
        /// <param name="newName">new name</param>
        /// <returns>bool</returns>
        public static async Task<bool> MoveFolderAsync(OneDriveStorageFolder folder, OneDriveStorageFolder target, string newName = null)
        {
            return await folder.MoveAsync(target, newName);
        }

        /// <summary>
        /// Copy folder
        /// </summary>
        /// <param name="folder">current folder</param>
        /// <param name="target">target</param>
        /// <param name="newName">new name</param>
        /// <returns>bool</returns>
        public static async Task<bool> CopyFolderAsync(OneDriveStorageFolder folder, OneDriveStorageFolder target, string newName = null)
        {
            return await folder.CopyAsync(target, newName);
        }

        /// <summary>
        /// Rename folder
        /// </summary>
        /// <param name="folder">current folder</param>
        /// <param name="newName">new name</param>
        /// <returns>named folder</returns>
        public static async Task<OneDriveStorageFolder> RenameFolderAsync(OneDriveStorageFolder folder, string newName)
        {
            return await folder.RenameAsync(newName);
        }

        /// <summary>
        /// Create new file
        /// </summary>
        /// <param name="folder">current folder</param>
        /// <param name="file">created file</param>
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
                        fileCreated = await folder.UploadFileAsync(file.Name, localStream, CreationCollisionOption.GenerateUniqueName, 320 * 1024);
                    else
                        fileCreated = await folder.CreateFileAsync(file.Name, CreationCollisionOption.GenerateUniqueName, localStream);
                }
            }
            return fileCreated;
        }

        /// <summary>
        /// Move a file
        /// </summary>
        /// <param name="file">file</param>
        /// <param name="target">target folder</param>
        /// <param name="newName">new name</param>
        /// <returns></returns>
        public static async Task<bool> MoveFileAsync(OneDriveStorageFile file,OneDriveStorageFolder target, string newName = null)
        {
            return await file.MoveAsync(target, newName);
        }

        /// <summary>
        /// Copy a file
        /// </summary>
        /// <param name="file">file</param>
        /// <param name="target">target folder</param>
        /// <param name="newName">new name</param>
        /// <returns></returns>
        public static async Task<bool> CopyFileAsync(OneDriveStorageFile file, OneDriveStorageFolder target, string newName = null)
        {
            return await file.CopyAsync(target, newName);
        }

        /// <summary>
        /// Rename a file
        /// </summary>
        /// <param name="file">file</param>
        /// <param name="newName">new name</param>
        /// <returns></returns>
        public static async Task<OneDriveStorageFile> RenameFileAsync(OneDriveStorageFile file, string newName)
        {
            return await file.RenameAsync(newName);
        }

        /// <summary>
        /// Download a file and save the content in a local file
        /// </summary>
        /// <param name="folder">source folder</param>
        /// <param name="target">target folder</param>
        /// <param name="fileName">file name</param>
        public static async Task<StorageFile> DownloadFileAsync(OneDriveStorageFolder folder, StorageFolder target, string fileName)
        {
            // Download a file and save the content in a local file
            var remoteFile = await folder.GetFileAsync(fileName);

            StorageFile myLocalFile = null;
            using (var remoteStream = await remoteFile.OpenAsync() as IRandomAccessStream)
            {
                byte[] buffer = new byte[remoteStream.Size];
                var localBuffer = await remoteStream.ReadAsync(buffer.AsBuffer(), (uint)remoteStream.Size, InputStreamOptions.ReadAhead);
                myLocalFile = await target.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                using (var localStream = await myLocalFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await localStream.WriteAsync(localBuffer);
                    await localStream.FlushAsync();
                }
            }

            return myLocalFile;
        }

        /// <summary>
        /// Get the thumbnail of a file
        /// </summary>
        /// <param name="file"></param>
        /// <returns>BitmapImage instance for Image source</returns>
        public static async Task<BitmapImage> RetrieveFileThumbnails(OneDriveStorageFile file)
        {
            var stream = await file.GetThumbnailAsync(ThumbnailSize.Large);
            BitmapImage bmp = new BitmapImage();
            await bmp.SetSourceAsync(stream as IRandomAccessStream);

            return bmp;
        }

        #endregion

    }

}
