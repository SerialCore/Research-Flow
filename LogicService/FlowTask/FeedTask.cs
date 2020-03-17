using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogicService.FlowTask
{
    public class FeedTask
    {

        public void Run()
        {
            // thread security
            List<FeedSource> FeedSources = Task.Run(async () =>
            {
                return await LocalStorage.ReadJsonAsync<List<FeedSource>>(await LocalStorage.GetDataFolderAsync(), "rss.list");
            }).Result;

            foreach (FeedSource source in FeedSources)
            {
                RssService.GetRssItems(
                    source.Uri,
                    (items) =>
                    {
                        List<Feed> feeds = items as List<Feed>;
                        Feed.DBInsert(feeds);
                        source.LastUpdateTime = DateTime.Now;
                        Task.Run(async () =>
                        {
                            LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "rss.list", FeedSources);
                        });

                        Task.Run(() =>
                        {
                            LocalStorage.GeneralLogAsync<Feed>("FeedTask.log", "feed updated-" + source.Name);
                        });
                    },
                    (exception) =>
                    {
                        Task.Run(() =>
                        {
                            LocalStorage.GeneralLogAsync<Feed>("FeedTask.log", exception + "-" + source.Name);
                        });
                    }, null);
            }
        }

    }
}
