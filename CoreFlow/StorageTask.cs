using LogicService.Services;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace CoreFlow
{
    public sealed class StorageTask : IBackgroundTask
    {

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            await GraphService.OneDriveLogin();
            if (GraphService.IsSignedIn && GraphService.IsNetworkAvailable)
            {
                if (await Synchronization.FileTracer())
                {
                    LocalStorage.GeneralLog<Synchronization>("StorageTask.log",
                        "Synchronized successfully");
                }
            }

            deferral.Complete();
        }

    }
}
