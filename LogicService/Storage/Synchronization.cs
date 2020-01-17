using LogicService.Application;
using LogicService.Helper;
using LogicService.Data;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace LogicService.Storage
{
    public class Synchronization
    {
        public async static Task ScanFiles()
        {
            // some rules:
            // trace entry could be modified by sync,       but list entry only by sync
            // trace entry could be added by sync,          but list entry only by general function
            // trace entry could only be deleted by sync,   but list entry only by general function
            // remove file, and remove local trace
            // download file, and update trace datetime
            // the files in cloud may be deleted by user, not by app, so double check all the mirrors
            // roaming info lists files that SHOULD exist in cloud and all the clients
            // the files in local may be deleted by user, if someone did this, it's not our fault

            // load file trace
            HashSet<FileList> traceAll = FileList.DBSelectAllTrace();

            // load file list
            HashSet<FileList> listAll = FileList.DBSelectAllList();

            // generate temp
            //List<FileList> deleteTrace = new List<FileList>();
            HashSet<FileList> list = new HashSet<FileList>(listAll);
            HashSet<FileList> trace = new HashSet<FileList>(traceAll);
            list.ExceptWith(traceAll);
            trace.ExceptWith(listAll);

            foreach (var item in listAll) // both & modify
            {
                foreach (var twin in traceAll)
                {
                    if (item.Equals(twin))
                    {
                        if (twin.DateModified > item.DateModified)
                        {
                            try
                            {
                                var local = await (await LocalStorage.GetCacheSubFolderAsync(item.FilePosition)).GetFileAsync(item.FileName);
                                var mirrorfolder = await OneDriveStorage.GetFolderAsync(item.FilePosition);
                                await OneDriveStorage.CreateFileAsync(mirrorfolder, local);
                                //
                                FileList.DBUpdateList(item.FilePosition, item.FileName);
                            }
                            catch (ServiceException)
                            {

                            }
                            catch (FileNotFoundException)
                            {

                            }
                        }
                        else if (twin.DateModified < item.DateModified)
                        {
                            try
                            {
                                var mirrorfolder = await OneDriveStorage.GetFolderAsync(item.FilePosition);
                                // check if other clients have uploaded their newest file
                                if ((await mirrorfolder.GetFileAsync(item.FileName)).DateModified.Value > twin.DateModified)
                                {
                                    await OneDriveStorage.DownloadFileAsync(mirrorfolder,
                                        await LocalStorage.GetCacheSubFolderAsync(item.FilePosition), item.FileName);
                                    //
                                    FileList.DBUpdateTrace(item.FilePosition, item.FileName);
                                }
                            }
                            catch (ServiceException)
                            {

                            }
                        }
                    }
                }
            }
            // this section serves multi-devices sync
            foreach (var item in list) // down way & download file
            {
                try
                {
                    var mirrorfolder = await OneDriveStorage.GetFolderAsync(item.FilePosition);
                    await OneDriveStorage.DownloadFileAsync(mirrorfolder,
                        await LocalStorage.GetCacheSubFolderAsync(item.FilePosition), item.FileName);
                    // 
                    FileList.DBUpdateList(item.FilePosition, item.FileName);
                    FileList.DBInsertTrace(item.FilePosition, item.FileName);
                }
                catch (ServiceException)
                {

                }
            }
            foreach (var item in trace) // delete file
            {
                try
                {
                    var localfolder = await LocalStorage.GetCacheSubFolderAsync(item.FilePosition);
                    var local = await localfolder.GetFileAsync(item.FileName);
                    await local.DeleteAsync();
                    //
                    var mirrorfolder = await OneDriveStorage.GetFolderAsync(item.FilePosition);
                    var mirror = await mirrorfolder.GetFileAsync(item.FileName);
                    await mirror.DeleteAsync();
                    // 
                    FileList.DBDeleteTrace(item.FilePosition, item.FileName);
                }
                catch (FileNotFoundException)
                {

                }
                catch (ServiceException)
                {

                }
            }
        }

        public async static Task DownloadAll()
        {
            foreach (var item in await (await OneDriveStorage.GetDataAsync()).GetFilesAsync(50))
            {
                await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetDataAsync(),
                    await LocalStorage.GetDataFolderAsync(), item.Name);
                FileList.DBInsertTrace("Data", item.Name);
                FileList.DBInsertList("Data", item.Name);
            }
            foreach (var item in await (await OneDriveStorage.GetNoteAsync()).GetFilesAsync(50))
            {
                await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetNoteAsync(),
                    await LocalStorage.GetNoteFolderAsync(), item.Name);
                FileList.DBInsertTrace("Note", item.Name);
                FileList.DBInsertList("Note", item.Name);
            }
        }

    }
}
