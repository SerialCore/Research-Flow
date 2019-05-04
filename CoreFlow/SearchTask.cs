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
                    await LocalStorage.GetFeedAsync(), "RSS") as List<RSSSource>;

            foreach (RSSSource source in FeedSources)
            {
                TimeSpan ts1 = new TimeSpan(DateTime.Now.Ticks);
                TimeSpan ts2 = new TimeSpan(source.LastUpdateTime.Ticks);
                TimeSpan ts = ts1.Subtract(ts2).Duration();
                if (ts.Days >= source.DaysforUpdate)
                {
                    RssService.GetRssItems(
                        source.Uri,
                        async (items) =>
                        {
                            List<FeedItem> feeds = items as List<FeedItem>;
                            if (feeds.Count > source.MaxCount)
                                feeds.RemoveRange(source.MaxCount, feeds.Count - source.MaxCount);
                            await LocalStorage.WriteJsonAsync(await LocalStorage.GetFeedAsync(), source.ID, items);
                            source.LastUpdateTime = DateTime.Now;
                            await LocalStorage.WriteJsonAsync(await LocalStorage.GetFeedAsync(), "RSS", FeedSources);

                            // inform user
                        },
                        (exception) =>
                        {
                            // save to log
                        }, null);
                }
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
