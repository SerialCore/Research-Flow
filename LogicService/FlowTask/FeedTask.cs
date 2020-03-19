using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;

namespace LogicService.FlowTask
{
    public class FeedTask
    {

        public async void Run()
        {
            // thread security
            List<FeedSource> FeedSources = await LocalStorage.ReadJsonAsync<List<FeedSource>>(await LocalStorage.GetDataFolderAsync(), "rss.list");

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
                    },
                    (exception) =>
                    {
                        LocalStorage.GeneralLogAsync<Feed>("FeedTask.log", exception + "-" + source.Name);
                    }, null);
            }
        }

    }
}
