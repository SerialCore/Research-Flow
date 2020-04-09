using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;

namespace LogicService.FlowTask
{
    public class FeedTask : ForegroundTask
    {

        //public event EventHandler<TaskCompletedEventArgs> TaskCompleted;

        public FeedTask()
        {
            this.id = "";
            this.isrunning = false;
            this.isavailable = false;
        }

        public override async void Run()
        {
            isrunning = true;
            //TaskCompletedEventArgs args = new TaskCompletedEventArgs();

            List<FeedSource> FeedSources;
            try
            {
                FeedSources = await LocalStorage.ReadJsonAsync<List<FeedSource>>(await LocalStorage.GetDataFolderAsync(), "rss.list");
            }
            catch
            {
                FeedSources = new List<FeedSource>();
            }

            foreach (FeedSource source in FeedSources)
            {
                RssService.GetRssItems(
                    source.Uri,
                    async (items) =>
                    {
                        List<Feed> feeds = items as List<Feed>;
                        Feed.DBInsert(feeds);
                        source.LastUpdateTime = DateTime.Now;
                        LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "rss.list", FeedSources);

                        LocalStorage.GeneralLogAsync<Feed>("FeedTask.log", "feed updated-" + source.Name);
                        isrunning = false;
                    },
                    (exception) =>
                    {
                        LocalStorage.GeneralLogAsync<Feed>("FeedTask.log", exception + "-" + source.Name);
                        isrunning = false;
                    }, null);
            }
        }

    }
}
