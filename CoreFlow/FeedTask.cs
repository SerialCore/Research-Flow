using LogicService.Application;
using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;

namespace CoreFlow
{
    public sealed class FeedTask : IBackgroundTask
    {

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            List<FeedSource> sources = await LocalStorage.ReadJsonAsync<List<FeedSource>>(LocalStorage.GetLocalCacheFolder(), "rss.list");

            foreach (FeedSource source in sources)
            {
                FeedService.GetFeed(
                    source.Uri,
                    (items) =>
                    {
                        List<Feed> feeds = items as List<Feed>;
                        Feed.DBInsert(feeds);
                        LocalStorage.GeneralLogAsync<FeedTask>("feed.log", "update " + source.Name);
                    },
                    (exception) =>
                    {
                        LocalStorage.GeneralLogAsync<FeedTask>("feed.log", "update " + source.Name);
                    }, null);
            }

            ApplicationNotification.ShowTextToast("Background Task", "Feeds have been updated");

            deferral.Complete();
        }

    }
}
