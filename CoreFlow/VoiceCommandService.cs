using Windows.ApplicationModel.Background;

namespace CoreFlow
{
    public sealed class VoiceCommandService:IBackgroundTask
    {

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            deferral.Complete();
        }

    }
}
