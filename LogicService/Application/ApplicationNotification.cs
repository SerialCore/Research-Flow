using Microsoft.Toolkit.Uwp.Notifications;
using System;
using Windows.UI.Notifications;

namespace LogicService.Application
{
    public class ApplicationNotification
    {
        public static void ShowTextToast(string title, string content)
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
                            Source = "ms-appx:///Assets/LargeTile.scale-100.png",
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

        public static void ScheduleAlarmToast(string id, string title, string content, DateTimeOffset dateTime)
        {
            var toast = new ToastContent()
            {
                Scenario = ToastScenario.Alarm,

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
                            Source = "ms-appx:///Assets/LargeTile.scale-100.png",
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        },

                        Attribution = new ToastGenericAttributionText()
                        {
                            Text = "Via Research Flow"
                        }
                    }
                },

                Actions = new ToastActionsCustom()
                {
                    Inputs =
                    {
                        new ToastSelectionBox("snoozeTime")
                        {
                            DefaultSelectionBoxItemId = "15",
                            Items =
                            {
                                new ToastSelectionBoxItem("5", "5 minutes"),
                                new ToastSelectionBoxItem("15", "15 minutes"),
                                new ToastSelectionBoxItem("60", "1 hour"),
                                new ToastSelectionBoxItem("240", "4 hours"),
                                new ToastSelectionBoxItem("1440", "1 day")
                            }
                        }
                    },

                    Buttons =
                    {
                        new ToastButtonSnooze()
                        {
                            SelectionBoxId = "snoozeTime"
                        },

                        new ToastButtonDismiss()
                    }
                }

                //Audio = new ToastAudio()
                //{
                //    Src = new Uri("ms-appx:///Assets/NewMessage.mp3")
                //}
            };

            var alarm = new ScheduledToastNotification(toast.GetXml(), dateTime);
            alarm.Tag = id;
            ToastNotificationManager.CreateToastNotifier().AddToSchedule(alarm);
        }

        public static void CancelAlarmToast(string id)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            foreach (var toast in notifier.GetScheduledToastNotifications())
            {
                if (toast.Tag.Equals(id))
                    notifier.RemoveFromSchedule(toast);
            }
        }

    }
}
