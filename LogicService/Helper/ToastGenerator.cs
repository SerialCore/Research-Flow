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

        public static ToastContent ScreenShotSaved(string folder)
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
                                Text = "Sceen Shot Saved"
                            },

                            new AdaptiveText()
                            {
                                Text = "Please check out " + folder
                            }
                        }
                    }
                }
            };
        }

    }
}
