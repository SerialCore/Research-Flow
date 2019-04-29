using LogicService.Helper;
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

            if(await Synchronization.ScanChanges())
            {
                ToastGenerator.ShowTextToast("StorageTask", "Triggered");
            }
            else
            {
                ToastGenerator.ShowTextToast("StorageTask", "Fail to sync");
            }

            deferral.Complete();
        }

    }
}
