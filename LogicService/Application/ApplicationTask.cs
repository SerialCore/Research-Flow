using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace LogicService.Application
{
    public class ApplicationTask
    {

        public static async Task<BackgroundTaskRegistration> RegisterLearnTask(Type taskEntryPoint,
            IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            return await RegisterBackgroundTask(taskEntryPoint, "LearnTask", trigger, condition);
        }

        public static async Task<BackgroundTaskRegistration> RegisterSearchTask(Type taskEntryPoint,
            IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            return await RegisterBackgroundTask(taskEntryPoint, "SearchTask", trigger, condition);
        }

        public static async Task<BackgroundTaskRegistration> RegisterStorageTask(Type taskEntryPoint,
            IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            return await RegisterBackgroundTask(taskEntryPoint, "StorageTask", trigger, condition);
        }

        private static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(Type taskEntryPoint,
            string taskName, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.Unspecified || status == BackgroundAccessStatus.DeniedByUser
                || status == BackgroundAccessStatus.DeniedByUser)
            {
                return null;
            }

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == taskName)
                {
                    cur.Value.Unregister(true);
                }
            }

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
