using Windows.ApplicationModel.Background;

namespace CoreFlow
{
    public sealed class TaskQuery : IBackgroundTask
    {

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            // process tasks in task query
            // delete file, search paper

            deferral.Complete();
        }

    }
}
