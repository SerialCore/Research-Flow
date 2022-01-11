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

            FeedSource source = GetRandomItem(sources);
            if (source != null)
            {
                Random random = new Random();
                while (source == null || random.Next(1) > (source.Star - 1) / 5)
                {
                    source = GetRandomItem(sources);
                }

                FeedService.GetFeed(
                    source.Uri,
                    (items) =>
                    {
                        List<Feed> feeds = items as List<Feed>;
                        Feed.DBInsert(source.ID, feeds);
                        LocalStorage.GeneralLogAsync<FeedTask>("feed.log", source.Name + " updated");
                        if (source.Notify)
                        {
                            Feed feed =  GetRandomItem(feeds);
                            ApplicationNotification.ShowTextToast(feed.Title, feed.Authors, source.Name);
                        }
                    },
                    (exception) =>
                    {
                        LocalStorage.GeneralLogAsync<FeedTask>("feed.log", source.Name + " unchanged: " + exception);
                    }, null);
            }

            deferral.Complete();
        }

        private T GetRandomItem<T>(List<T> objs)
        {
            if (objs != null && objs.Count != 0)
            {
                int index = (int)Math.Round((decimal)new Random().Next(0, 100) * (objs.Count - 1) / 100);
                return (T)objs[index];
            }
            else
            {
                return default(T);
            }
        }

    }
}
