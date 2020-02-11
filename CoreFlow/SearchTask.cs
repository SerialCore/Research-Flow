using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;

namespace CoreFlow
{
    public sealed class SearchTask : IBackgroundTask
    {

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            SearchRSS();

            deferral.Complete();
        }

        private async void SearchRSS()
        {
            try
            {
                List<RSSSource> FeedSources = await LocalStorage.ReadJsonAsync<List<RSSSource>>(
                        await LocalStorage.GetDataFolderAsync(), "rsslist");

                foreach (RSSSource source in FeedSources)
                {
                    RssService.GetRssItems(
                        source.Uri,
                        async (items) =>
                        {
                            List<FeedItem> feeds = items as List<FeedItem>;
                            FeedItem.DBInsert(feeds);
                            source.LastUpdateTime = DateTime.Now;
                            LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "rsslist", FeedSources);

                            // inform user
                            LocalStorage.GeneralLogAsync<RssService>("SearchTask.log",
                                    "just updated your rss feed-" + source.Name);
                        },
                        (exception) =>
                        {
                            // save to log
                            LocalStorage.GeneralLogAsync<RssService>("SearchTask.log",
                                    exception + "-" + source.Name);
                        }, null);
                }
            }
            catch (Exception exception)
            {
                LocalStorage.GeneralLogAsync<RssService>("SearchTask.log", exception.Message);
            }
        }

        /// <summary>
        /// Search Engine
        /// </summary>
        private void SearchTags()
        {

        }

        /// <summary>
        /// Crawler
        /// </summary>
        private void SearchUrls()
        {

        }

    }
}
