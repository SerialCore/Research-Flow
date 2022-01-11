using LogicService.Application;
using LogicService.Data;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;

namespace CoreFlow
{
    public sealed class TileTask : IBackgroundTask
    {

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            if (ApplicationSetting.EqualKey("LiveTile", "topic"))
            {
                List<Topic> topics = null;
                try
                {
                    topics = await LocalStorage.ReadJsonAsync<List<Topic>>(LocalStorage.GetLocalCacheFolder(), "topic.list");
                }
                catch { }

                Topic topic = GetRandomItem(topics);
                if (topic != null)
                {
                    ApplicationNotification.CancelLiveTile();
                    ApplicationNotification.ShowTextTile(topic.Title, (topic.Completeness / 100).ToString("P0"));
                }
            }
            else if (ApplicationSetting.EqualKey("LiveTile", "bookmark"))
            {
                List<Feed> feeds = null;
                try
                {
                    feeds = Feed.DBSelectBookmark();
                }
                catch { }

                Feed feed = GetRandomItem(feeds);
                if (feed != null)
                {
                    ApplicationNotification.CancelLiveTile();
                    ApplicationNotification.ShowTextTile(feed.Title, feed.Authors);
                }
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
