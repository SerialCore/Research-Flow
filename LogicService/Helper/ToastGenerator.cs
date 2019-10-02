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
                Scenario = ToastScenario.Reminder,

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
                        },

                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = "ms-appx:///LargeTile.scale-100.png",
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        },

                        Attribution = new ToastGenericAttributionText()
                        {
                            Text = "Via Research Flow"
                        }
                    }
                }
            };

            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toast.GetXml()));
        }

    }
}
