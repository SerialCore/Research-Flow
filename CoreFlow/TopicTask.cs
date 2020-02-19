using LogicService.Application;
using Windows.ApplicationModel.Background;

namespace CoreFlow
{
    public sealed class TopicTask : IBackgroundTask
    {

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            //List<RSSSource> FeedSources = Task.Run(async () =>
            //{
            //    return await LocalStorage.ReadJsonAsync<List<RSSSource>>(await LocalStorage.GetDataFolderAsync(), "rss.list");
            //}).Result;
            //ApplicationNotification.ShowTextToast("TopicTask", "");

            deferral.Complete();
        }

    }
}
