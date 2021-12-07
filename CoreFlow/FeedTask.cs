using LogicService.Application;
using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;

namespace CoreFlow
{
    public sealed class FeedTask : IBackgroundTask
    {

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            List<FeedSource> sources = null;
            try
            {
                sources = await LocalStorage.ReadJsonAsync<List<FeedSource>>(LocalStorage.GetLocalCacheFolder(), "rss.list");
            }
            catch { }

            FeedSource source = GetRandomFeedSource(sources);
            if (source != null)
            {
                Random random = new Random();
                while (source == null || random.Next(1) > (source.Star - 1) / 5)
                {
                    source = GetRandomFeedSource(sources);
                }

                FeedService.GetFeed(
                    source.Uri,
                    (items) =>
                    {
                        List<Feed> feeds = items as List<Feed>;
                        Feed.DBInsert(feeds);
                        LocalStorage.GeneralLogAsync<FeedTask>("feed.log", source.Name + " updated");
                        ApplicationNotification.ShowTextToast("Feed Task", source.Name + " updated");
                    },
                    (exception) =>
                    {
                        LocalStorage.GeneralLogAsync<FeedTask>("feed.log", source.Name + " unchanged: " + exception);
                    }, null);
            }

            deferral.Complete();
        }

        private FeedSource GetRandomFeedSource(List<FeedSource> sources)
        {
            if (sources != null && sources.Count != 0)
            {
                int index = (int)Math.Round((decimal)new Random().Next(0, 100) * (sources.Count - 1) / 100);
                return sources[index];
            }
            else
            {
                return null;
            }
        }

    }
}
