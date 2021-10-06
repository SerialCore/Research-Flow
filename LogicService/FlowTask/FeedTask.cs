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

        // TODO: refer the method in Feed.class
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
                    RssService.BeginGetFeed(
                        source.Uri, (items) =>
                        {
                            List<Feed> feeds = items as List<Feed>;
                            Feed.DBInsert(feeds);
                            source.LastUpdateTime = DateTime.Now;
                            LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "rss.list", sources);
                            LocalStorage.GeneralLogAsync<Feed>("FeedTask.log", "feed updated-" + source.Name);

                            IsRunning = false;
                            args.Log += "feed updated-" + source.Name + "\r\n";
                            if (TaskCompleted != null)
                            {
                                TaskCompleted(args);
                            }
                        },
                        (exception) =>
                        {
                            LocalStorage.GeneralLogAsync<Feed>("FeedTask.log", exception + "-" + source.Name);

                            IsRunning = false;
                            args.Log += exception + "-" + source.Name + "\r\n";
                            if (TaskCompleted != null)
                            {
                                TaskCompleted(args);
                            }
                        }, null);
                }
            }
        }
    }
}
