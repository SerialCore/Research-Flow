using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicService.Helper;
using Windows.ApplicationModel.Background;
using LogicService.Objects;
using LogicService.Storage;
using LogicService.Services;

namespace CoreFlow
{
    public sealed class SearchTask : IBackgroundTask
    {

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            SearchRss();

            deferral.Complete();
        }

        private async void SearchRss()
        {
            List<RSSSource> FeedSources = await LocalStorage.ReadJsonAsync<List<RSSSource>>(
                    await LocalStorage.GetDataAsync(), "rsslist");

            foreach (RSSSource source in FeedSources)
            {
                RssService.GetRssItems(
                    source.Uri,
                    async (items) =>
                    {
                        List<FeedItem> feeds = items as List<FeedItem>;

                        FeedItem.DBOpen();
                        FeedItem.DBInsert(feeds);
                        FeedItem.DBClose();

                        source.LastUpdateTime = DateTime.Now;
                        LocalStorage.WriteJson(await LocalStorage.GetDataAsync(), "rsslist", FeedSources);

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

        private void SearchKeys()
        {

        }

        private void SearchUrls()
        {

        }

    }
}
