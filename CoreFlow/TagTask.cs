using LogicService.Application;
using Windows.ApplicationModel.Background;

namespace CoreFlow
{
    public sealed class TagTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            ApplicationNotification.ShowTextToast("TagTask", "");

            deferral.Complete();
        }

    }
}
