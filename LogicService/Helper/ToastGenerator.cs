using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace LogicService.Helper
{
    public class ToastGenerator
    {

        public static void ShowTextToast(string title,string content)
        {
            var toast = new ToastContent()
            {
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
