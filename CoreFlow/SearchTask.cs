using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicService.Helper;
using Windows.ApplicationModel.Background;

namespace CoreFlow
{
    public sealed class SearchTask : IBackgroundTask
    {

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            ToastGenerator.ShowTextToast("SearchTask", "Triggered");

            deferral.Complete();
        }

    }
}
