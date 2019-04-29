using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace LogicService.Helper
{
    public class ToastGenerator
    {

        public static void ShowTextToast(string title,string content)
        {
            var toast = new ToastContent()
            {
                Launch = "action=viewEvent&eventId=1983", // how to launch target app
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

            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toast.GetXml()));
        }

    }
}
