using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;

namespace LogicService.FlowTask
{
    public class FeedTask : ForegroundTask
    {
        public delegate void TaskHandle(TaskCompletedEventArgs args);

        public event TaskHandle TaskCompleted;

        public override async void Run()
        {
            IsRunning = true;
            TaskCompletedEventArgs args = new TaskCompletedEventArgs();
            args.Task = typeof(FeedTask);

            List<FeedSource> sources = null;
            try
            {
                sources = await LocalStorage.ReadJsonAsync<List<FeedSource>>(LocalStorage.GetLocalCacheFolder(), "rss.list");
            }
            catch { }

            if (sources != null)
            {
                foreach (FeedSource source in sources)
                {
                    try
                    {
                        List<Feed> feeds = FeedService.TryGetFeed(source.Uri);
                        Feed.DBInsert(feeds);
                        args.Log += $"{source.Name} updated\t\n";
                    }
                    catch (Exception exception)
                    {
                        args.Log += $"{source.Name} {exception}\t\n";
                    }
                }
            }

            IsRunning = false;
            if (TaskCompleted != null)
            {
                TaskCompleted(args);
            }
        }
    }
}
