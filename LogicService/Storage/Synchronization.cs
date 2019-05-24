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
            // some rules: associated with main syncing algorithm as a supplement
            // updated files will all be tagged synced
            // local deleting in sync process will not be recorded, but checked
            // remove file, and remove local trace
            // download file, and update datetime
            // the files in cloud may be deleted by user, not by app, so double check all the mirrors in try
            // the files in local may be deleted by user, if someone did this, it's not our fault

            // bug in removelist
            // if a file was deleted by user (recorded), but added again sometime, 
            // then this file will be deleted locally
            // so, when to clean removelist
            // furthermore, when to clean addlist

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

            List<FileTrace> deleteTrace = new List<FileTrace>();

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
                            traceItem.DateModified = DateTime.Now;
                        }
                    }
                    catch (ServiceException) // exist only in local, then delete
                    {
                        try
                        {
                            var temp = await (await LocalStorage.GetFolderAsync(traceItem.FilePosition)).GetFileAsync(traceItem.FileName);
                            await temp.DeleteAsync();
                            deleteTrace.Add(traceItem);
                            // check remove list, not add
                            foreach (RemoveList removeItem in remove)
                            {
                                if (removeItem.FileName == traceItem.FileName && removeItem.FilePosition == traceItem.FilePosition)
                                {
                                    removeItem.Checked++; break;
                                }
                            }
                        } catch (FileNotFoundException) { } // someone's mistake
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
                                traceItem.DateModified = DateTime.Now;
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
                                deleteTrace.Add(traceItem);
                            }
                            else // the server has never had this file
                            {
                                await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetFolderAsync(traceItem.FilePosition), local);
                                traceItem.IsSynced = true; // tagged true
                            }
                        }
                    }
                    catch (FileNotFoundException) // not exist in local, may exist in cloud
                    {
                        try
                        {
                            // there might be no network issue, since sync process should be under the available connection
                            var mirror = await OneDriveStorage.RetrieveFileAsync(await OneDriveStorage.GetFolderAsync(traceItem.FilePosition), traceItem.FileName);
                            await mirror.DeleteAsync();
                        }
                        catch (ServiceException) { } // not exist in cloud
                        finally
                        {
                            // once deleted in cloud, by app or user (externally)
                            // or not uploaded after deleted
                            // remove the trace and leave the removelist recorded (untouched)
                            deleteTrace.Add(traceItem);
                        }
                    }
                }
            }

            // remove some trace
            foreach (FileTrace index in deleteTrace)
            {
                trace.Remove(index);
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
                CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(file, SerializeHelper.SerializeToJson(trace));
        }

    }
}
