using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using LogicService.Helper;
using LogicService.Storage;
using LogicService.Objects;

namespace CoreFlow
{
    public sealed class TopicTask : IBackgroundTask
    {

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            TopicNatification();

            deferral.Complete();
        }

        /*
         * Start Off, End Off, Remind Off       just a topic
         * Start Off, End Off, Remind On        an alarm
         * Start Off, End On, Remind Off        a deadline
         * Start Off, End On, Remind On         a deadline with alarm
         * Start On, End Off, Remind Off        None
         * Start On, End Off, Remind On         None
         * Start On, End On, Remind Off         a project
         * Start On, End On, Remind On          a proctect with alarm
         */

        private void TopicNatification()
        {
            //ToastGenerator.ShowTextToast("TopicTask", "TopicNatification");
        }

    }
}
