using LogicService.Helper;
using LogicService.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace LogicService.Storage
{
    public class Synchronization
    {

        public static async Task FileTracer()
        {
            // Delete Treatment: scanning
            // If a file was not synced but only existed on cloud, that means it should be deleted from cloud.
            // If files are deleted from cloud by app or user, another app should directly remove this locally.
            // Add Treatment: recording
            // A file was wrote and tagged unsynced, check if there is a same one on cloud.
            // If true, compare modified date, else upload directly.

            List<FileTrace> trace;
            StorageFile file = await (await LocalStorage.GetLogAsync()).CreateFileAsync("filetrace",
                CreationCollisionOption.OpenIfExists);
            trace = SerializeHelper.DeserializeJsonToObject<List<FileTrace>>(await FileIO.ReadTextAsync(file));
            if (trace == null)
                trace = new List<FileTrace>();

            foreach (FileTrace item in trace)
            {
                if(!item.IsSynced)
                {
                    try
                    {
                        var source = await (await LocalStorage.GetFolderAsync(item.FilePosition)).GetFileAsync(item.FileName);
                        try
                        {
                            var mirror = await OneDriveStorage.RetrieveFileAsync(await OneDriveStorage.GetFolderAsync(item.FilePosition), item.FileName);

                        }
                        catch
                        {
                            // if net issue?
                            await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetFolderAsync(item.FilePosition), source);
                            item.IsSynced = true;
                        }
                    }
                    catch(FileNotFoundException) // set for failure of finding source
                    {
                        try
                        {
                            var mirror = await OneDriveStorage.RetrieveFileAsync(await OneDriveStorage.GetFolderAsync(item.FilePosition), item.FileName);
                            await mirror.DeleteAsync();
                            item.IsSynced = true;
                        }
                        catch
                        {
                            await LocalStorage.GeneralLogAsync<Synchronization>("FileManagement.log", 
                                "Fail to fetch cloud file (to be deleted)-" + item.FilePosition + ": " + item.FileName);
                        }
                    }
                }
            }

            //await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetFeedAsync(), );

            await FileIO.WriteTextAsync(file, SerializeHelper.SerializeToJson(trace));
        }

        public static async void AddTrace(string position, string name)
        {
            List<FileTrace> trace;
            StorageFile file = await (await LocalStorage.GetLogAsync()).CreateFileAsync("filetrace",
                CreationCollisionOption.OpenIfExists);
            trace = SerializeHelper.DeserializeJsonToObject<List<FileTrace>>(await FileIO.ReadTextAsync(file));
            if (trace == null)
                trace = new List<FileTrace>();

            // check the existing item
            int traceIndex = -1;
            FileTrace add = new FileTrace
            {
                FileName = name,
                FilePosition = position,
                DateModified = DateTime.Now,
                IsSynced = false
            };
            foreach (FileTrace item in trace)
            {
                if (item.FileName == name && item.FilePosition == position)
                    traceIndex = trace.IndexOf(item);
            }
            if (traceIndex >= 0)
                trace[traceIndex] = add;
            else
                trace.Add(add);

            await FileIO.WriteTextAsync(file, SerializeHelper.SerializeToJson(trace));
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
