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

        public static async Task<bool> FileTracer()
        {
            // some rules:
            // updated files will all be tagged synced
            // local deleting in sync process will not be recorded, but checked

            // load file trace
            List<FileTrace> trace;
            StorageFile traceFile = await (await LocalStorage.GetLogAsync()).CreateFileAsync("filetrace",
                CreationCollisionOption.OpenIfExists);
            trace = SerializeHelper.DeserializeJsonToObject<List<FileTrace>>(await FileIO.ReadTextAsync(traceFile));
            if (trace == null)
                trace = new List<FileTrace>();

            // load remove list
            List<RemoveList> remove;
            StorageFile removeFile = await LocalStorage.GetRoamingFolder().CreateFileAsync(ApplicationSetting.AccountName + ".removelist",
                CreationCollisionOption.OpenIfExists);
            remove = SerializeHelper.DeserializeJsonToObject<List<RemoveList>>(await FileIO.ReadTextAsync(removeFile));
            if (remove == null)
                remove = new List<RemoveList>();

            List<int> deleteIntrace = new List<int>();

            // check process
            foreach (FileTrace traceItem in trace)
            {
                if (traceItem.IsSynced)
                {
                    try
                    {
                        var mirrorFolder = await OneDriveStorage.GetFolderAsync(traceItem.FilePosition); // for saving network resource
                        var mirror = await OneDriveStorage.RetrieveFileAsync(mirrorFolder, traceItem.FileName);
                        if (traceItem.DateModified < mirror.DateModified) // check date for saving network resource
                        {
                            await OneDriveStorage.DownloadFileAsync(mirrorFolder,
                                await LocalStorage.GetFolderAsync(traceItem.FilePosition), traceItem.FileName);
                        }
                    }
                    catch (ServiceException) // exist only in local, then delete // but network issue will cause the same exception
                    {
                        var temp = await (await LocalStorage.GetFolderAsync(traceItem.FilePosition)).GetFileAsync(traceItem.FileName);
                        await temp.DeleteAsync();
                        // check remove list, not add
                        foreach (RemoveList removeItem in remove)
                        {
                            if (removeItem.FileName == traceItem.FileName && removeItem.FilePosition == traceItem.FilePosition)
                            {
                                removeItem.Checked++; break;
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        var local = await (await LocalStorage.GetFolderAsync(traceItem.FilePosition)).GetFileAsync(traceItem.FileName);
                        try
                        {
                            var mirrorFolder = await OneDriveStorage.GetFolderAsync(traceItem.FilePosition); // for saving network resource
                            var mirror = await OneDriveStorage.RetrieveFileAsync(mirrorFolder, traceItem.FileName);
                            if (traceItem.DateModified < mirror.DateModified)
                            {
                                await OneDriveStorage.DownloadFileAsync(mirrorFolder,
                                    await LocalStorage.GetFolderAsync(traceItem.FilePosition), traceItem.FileName);
                                traceItem.IsSynced = true;
                            }
                            else
                            {
                                await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetFolderAsync(traceItem.FilePosition), local);
                                traceItem.IsSynced = true;
                            }
                        }
                        catch (ServiceException) // only exist in local
                        {
                            int removeIndex = -1;
                            foreach (RemoveList removeItem in remove)
                            {
                                if (removeItem.FileName == traceItem.FileName && removeItem.FilePosition == traceItem.FilePosition)
                                {
                                    removeIndex = remove.IndexOf(removeItem);
                                    break;
                                }
                            }
                            if (removeIndex >= 0) // another client had deleted this file from server
                            {
                                await local.DeleteAsync();
                                // there will never be such file, so check ths list, and remove trace
                                remove[removeIndex].Checked++;
                                deleteIntrace.Add(trace.IndexOf(traceItem));
                            }
                            else // the server has never had this file
                            {
                                await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetFolderAsync(traceItem.FilePosition), local);
                                traceItem.IsSynced = true; // tagged true
                            }
                        }
                    }
                    catch (FileNotFoundException) // not exist in local, logically that means existing in cloud
                    {
                        // there might be no network issue, since sync process should be under the available connection
                        var mirror = await OneDriveStorage.RetrieveFileAsync(await OneDriveStorage.GetFolderAsync(traceItem.FilePosition), traceItem.FileName);
                        await mirror.DeleteAsync();
                        // once deleted in cloud, remove the trace and leave the removelist recorded (untouched)
                        deleteIntrace.Add(trace.IndexOf(traceItem));
                    }
                }
            }

            // remove some trace
            foreach (int index in deleteIntrace)
            {
                trace.RemoveAt(index);
            }

            // save all
            await FileIO.WriteTextAsync(traceFile, SerializeHelper.SerializeToJson(trace));
            await FileIO.WriteTextAsync(removeFile, SerializeHelper.SerializeToJson(remove));

            return true;
        }

        public async static Task DownloadAll()
        {
            // create a new file, or replace the older trace by full-tagged one
            List<FileTrace> trace = new List<FileTrace>();

            foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetFeedAsync()))
            {
                await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetFeedAsync(),
                    await LocalStorage.GetFeedAsync(), item.Name);
                trace.Add(new FileTrace
                {
                    FileName = item.Name,
                    FilePosition = "Feed",
                    DateModified = DateTime.Now,
                    IsSynced = true
                });
            }
            foreach (var item in await OneDriveStorage.RetrieveFilesAsync(await OneDriveStorage.GetNoteAsync()))
            {
                await OneDriveStorage.DownloadFileAsync(await OneDriveStorage.GetNoteAsync(),
                    await LocalStorage.GetNoteAsync(), item.Name);
                trace.Add(new FileTrace
                {
                    FileName = item.Name,
                    FilePosition = "Note",
                    DateModified = DateTime.Now,
                    IsSynced = true
                });
            }

            StorageFile file = await (await LocalStorage.GetLogAsync()).CreateFileAsync("filetrace",
                CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, SerializeHelper.SerializeToJson(trace));
        }

    }
}
