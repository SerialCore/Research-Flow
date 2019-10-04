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

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            StorageSync();

            deferral.Complete();
        }

        private async void StorageSync()
        {
            await GraphService.OneDriveLogin();
            if (GraphService.IsConnected && GraphService.IsNetworkAvailable)
            {
                await Synchronization.ScanFiles();
                LocalStorage.GeneralLogAsync<Synchronization>("StorageTask.log",
                    "Synchronized");
            }
        }

        /// <summary>
        /// Clean unnecessary data
        /// </summary>
        private void StorageClean()
        {
            // clean database

        }

    }
}
