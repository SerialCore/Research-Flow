using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;

namespace LogicService.FlowTask
{
    public class FeedTask : ForegroundTask
    {

        public event EventHandler<TaskCompletedEventArgs> TaskCompleted;

        public override async void Run()
        {
            isrunning = true;
            TaskCompletedEventArgs args = new TaskCompletedEventArgs();

            List<FeedSource> FeedSources;
            try
            {
                FeedSources = await LocalStorage.ReadJsonAsync<List<FeedSource>>(LocalStorage.GetLocalCacheFolder(), "rss.list");
            }
            catch
            {
                FeedSources = new List<FeedSource>();
            }

            foreach (FeedSource source in FeedSources)
            {
                RssService.BeginGetFeed(
                    source.Uri, (items) =>
                    {
                        List<Feed> feeds = items as List<Feed>;
                        Feed.DBInsert(feeds);
                        source.LastUpdateTime = DateTime.Now;
                        LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "rss.list", FeedSources);

                        args.Log += "feed updated-" + source.Name + "\r\n";
                        LocalStorage.GeneralLogAsync<Feed>("FeedTask.log", "feed updated-" + source.Name);
                        isrunning = false;
                    },
                    (exception) =>
                    {
                        args.Log += exception + "-" + source.Name + "\r\n";
                        LocalStorage.GeneralLogAsync<Feed>("FeedTask.log", exception + "-" + source.Name);
                        isrunning = false;
                    }, null);
            }

            //TaskCompleted(typeof(FeedTask), args);
        }

    }
}
