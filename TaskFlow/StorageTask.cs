﻿using System;
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
            


            deferral.Complete();
        }

        private void UptoOneDrive()
        {

        }

        private void DowntoLcal()
        {

        }

    }
}
