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

                Topic topic = GetRandomTopic(topics);
                if (topic != null)
                {
                    ApplicationNotification.CancelLiveTile();
                    ApplicationNotification.ShowTextTile("Topic", topic.Title);
                }
            }

            deferral.Complete();
        }

        private Topic GetRandomTopic(List<Topic> topics)
        {
            if (topics != null && topics.Count != 0)
            {
                int index = (int)Math.Round((decimal)new Random().Next(0, 100) * (topics.Count - 1) / 100);
                return topics[index];
            }
            else
            {
                return null;
            }
        }

    }
}
