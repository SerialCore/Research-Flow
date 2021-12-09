using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Overview : Page
    {
        public Overview()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private bool IsPageFirstLoaded = true;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (IsPageFirstLoaded)
            {
                InitBackgroundTask();
                IsPageFirstLoaded = false;
            }
        }

        private async void InitBackgroundTask()
        {
            await ApplicationTask.RegisterTileTask();
            await ApplicationTask.RegisterFeedTask();
        }

    }

    public class ApplicationTask
    {
        public static async Task<BackgroundTaskRegistration> RegisterTileTask()
        {
            return await RegisterBackgroundTask(typeof(CoreFlow.TileTask),
                "TileTask", new TimeTrigger(30, false));
        }

        public static async Task<BackgroundTaskRegistration> RegisterFeedTask()
        {
            return await RegisterBackgroundTask(typeof(CoreFlow.FeedTask),
                "FeedTask", new TimeTrigger(60, false), new SystemCondition(SystemConditionType.InternetAvailable));
        }

        public static IReadOnlyDictionary<Guid, IBackgroundTaskRegistration> ListBackgroundTask()
        {
            return BackgroundTaskRegistration.AllTasks;
        }

        public static bool ContainBackgroundTask(string taskName)
        {
            foreach (var cur in ListBackgroundTask())
            {
                if (cur.Value.Name == taskName)
                {
                    return true;
                }
            }
            return false;
        }

        public static void CancelBackgroundTask(string taskName)
        {
            foreach (var cur in ListBackgroundTask())
            {
                if (cur.Value.Name == taskName)
                {
                    cur.Value.Unregister(true);
                }
            }
        }

        private static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(Type taskEntryPoint,
            string taskName, IBackgroundTrigger trigger, IBackgroundCondition condition = null)
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.Unspecified || status == BackgroundAccessStatus.DeniedByUser
                || status == BackgroundAccessStatus.DeniedByUser)
            {
                return null;
            }

            CancelBackgroundTask(taskName); // always cancel?

            var builder = new BackgroundTaskBuilder
            {
                Name = taskName,
                TaskEntryPoint = taskEntryPoint.FullName
            };

            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);
            }

            BackgroundTaskRegistration task = builder.Register();
            return task;
        }

    }
}
