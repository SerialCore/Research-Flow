using LogicService.Application;
using LogicService.Service;
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

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            StorageSync();

            deferral.Complete();
        }

        private async void StorageSync()
        {
            if (await GraphService.OneDriveLogin())
            {
                await Synchronization.ScanFiles();
                LocalStorage.GeneralLogAsync<Synchronization>("StorageTask.log",
                    "Synchronized");
            }
        }

        private void StorageClean()
        {
            // clean database
        }

    }
}
