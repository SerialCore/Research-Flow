using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Helper
{
    public class ToastGenerator
    {

        /// <summary>
        /// Save Screen shot
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static ToastContent TextToast(string title,string content)
        {
            return new ToastContent()
            {
                Launch = "action=viewEvent&eventId=1983",
                Scenario = ToastScenario.Default,

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title
                            },

                            new AdaptiveText()
                            {
                                Text = content
                            }
                        }
                    }
                }
            };
        }

    }
}
