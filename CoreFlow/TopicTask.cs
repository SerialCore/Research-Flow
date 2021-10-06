using LogicService.Application;
using LogicService.Data;
using Windows.ApplicationModel.Background;

namespace CoreFlow
{
    public sealed class TopicTask : IBackgroundTask
    {

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            if (ApplicationSetting.EqualKey("LiveTile", "topic"))
            {
                Topic topic = await Topic.GetRandomTopic();
                if (topic != null)
                    ApplicationNotification.ShowTextTile("Topic", topic.Title);
            }

            deferral.Complete();
        }

    }
}
