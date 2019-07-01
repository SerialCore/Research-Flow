using LogicService.Application;
using LogicService.Helper;
using LogicService.Objects;
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
            // the files in cloud may be deleted by user, not by app, so double check all the mirrors in try
            // roaming info lists files that SHOULD exist in cloud and all the clients
            // the files in local may be deleted by user, if someone did this, it's not our fault

            // load file trace
            HashSet<FileList> traceAll;
            StorageFile tracefile = await (await LocalStorage.GetLogAsync()).CreateFileAsync("filetrace",
                CreationCollisionOption.OpenIfExists);
            traceAll = SerializeHelper.DeserializeJsonToObject<HashSet<FileList>>(await FileIO.ReadTextAsync(tracefile));
            if (traceAll == null)
                traceAll = new HashSet<FileList>();

            // load file list
            HashSet<FileList> listAll;
            StorageFile listfile = await LocalStorage.GetRoamingFolder().CreateFileAsync(ApplicationSetting.AccountName + ".filelist",
                CreationCollisionOption.OpenIfExists);
            listAll = SerializeHelper.DeserializeJsonToObject<HashSet<FileList>>(await FileIO.ReadTextAsync(listfile));
            if (listAll == null)
                listAll = new HashSet<FileList>();

            // generate temp
            List<FileList> deleteTrace = new List<FileList>();
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
                                var local = await (await LocalStorage.GetFolderAsync(item.FilePosition)).GetFileAsync(item.FileName);
                                var mirrorfolder = await OneDriveStorage.GetFolderAsync(item.FilePosition);
                                await OneDriveStorage.CreateFileAsync(mirrorfolder, local);
                                //
                                item.DateModified = DateTime.Now;
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
                                        await LocalStorage.GetFolderAsync(item.FilePosition), item.FileName);
                                    //
                                    twin.DateModified = DateTime.Now;
                                }
                            }
                            catch (ServiceException)
                            {

                            }
                        }
                    }
                }
            }
            foreach (var item in list) // down way & download file
            {
                try
                {
                    var mirrorfolder = await OneDriveStorage.GetFolderAsync(item.FilePosition);
                    await OneDriveStorage.DownloadFileAsync(mirrorfolder,
                        await LocalStorage.GetFolderAsync(item.FilePosition), item.FileName);
                    // 
                    item.DateModified = DateTime.Now;
                    traceAll.Add(item);
                }
                catch (ServiceException)
                {

                }
            }
            foreach (var item in trace) // delete file
            {
                try
                {
                    var localfolder = await LocalStorage.GetFolderAsync(item.FilePosition);
                    var local = await localfolder.GetFileAsync(item.FileName);
                    await local.DeleteAsync();
                    //
                    var mirrorfolder = await OneDriveStorage.GetFolderAsync(item.FilePosition);
                    var mirror = await mirrorfolder.GetFileAsync(item.FileName);
                    await mirror.DeleteAsync();
                    // 
                    deleteTrace.Add(item);
                }
                catch (FileNotFoundException)
                {

                }
                catch (ServiceException)
                {

                }
            }

            foreach (var item in deleteTrace)
            {
                traceAll.Remove(item);
            }

            // save all
            await FileIO.WriteTextAsync(tracefile, SerializeHelper.SerializeToJson(traceAll));
            await FileIO.WriteTextAsync(listfile, SerializeHelper.SerializeToJson(listAll));
        }

        public async static Task DownloadAll()
        {
            // create a new file, or replace the older trace by full-tagged one
            List<FileList> trace = new List<FileList>();
            List<FileList> list = new List<FileList>();

            foreach (var item in await (await OneDriveStorage.GetFeedAsync()).GetFilesAsync(50))
            {
                await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetFeedAsync(),
                    await LocalStorage.GetFeedAsync(), item.Name);
                trace.Add(new FileList
                {
                    FileName = item.Name,
                    FilePosition = "Feed",
                    DateModified = DateTime.Now,
                });
                list.Add(new FileList
                {
                    FileName = item.Name,
                    FilePosition = "Feed",
                    DateModified = DateTime.Now,
                });
            }
            foreach (var item in await (await OneDriveStorage.GetNoteAsync()).GetFilesAsync(50))
            {
                await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetNoteAsync(),
                    await LocalStorage.GetNoteAsync(), item.Name);
                trace.Add(new FileList
                {
                    FileName = item.Name,
                    FilePosition = "Note",
                    DateModified = DateTime.Now,
                });
                list.Add(new FileList
                {
                    FileName = item.Name,
                    FilePosition = "Feed",
                    DateModified = DateTime.Now,
                });
            }

            StorageFile tracefile = await (await LocalStorage.GetLogAsync()).CreateFileAsync("filetrace",
                CreationCollisionOption.OpenIfExists);
            StorageFile listfile = await LocalStorage.GetRoamingFolder().CreateFileAsync(ApplicationSetting.AccountName + ".filelist",
                CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(tracefile, SerializeHelper.SerializeToJson(trace));
            await FileIO.WriteTextAsync(listfile, SerializeHelper.SerializeToJson(list));
        }

    }
}
