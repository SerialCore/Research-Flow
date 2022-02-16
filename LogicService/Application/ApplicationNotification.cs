using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace LogicService.Application
{
    public class ApplicationNotification
    {

        #region Toast 

        public static void ShowTextToast(string title, string content, string comment)
        {
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(GetTextToast(title, content, comment).GetXml()));
        }

        public static void ScheduleAlarmToast(string id, string title, string content, DateTimeOffset dateTime)
        {
            var alarm = new ScheduledToastNotification(GetAlarmToast(title, content).GetXml(), dateTime);
            alarm.Tag = id;
            alarm.NotificationMirroring = NotificationMirroring.Allowed;
            ToastNotificationManager.CreateToastNotifier().AddToSchedule(alarm);
        }

        public static async Task ScheduleRepeatAlarmToast(string id, string title, string content, DateTimeOffset dateTime, TimeSpan period, uint repeat)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < repeat; i++)
                {
                    var alarm = new ScheduledToastNotification(GetAlarmToast(title, content).GetXml(), dateTime + i * period);
                    alarm.Tag = id;
                    alarm.NotificationMirroring = NotificationMirroring.Allowed;
                    ToastNotificationManager.CreateToastNotifier().AddToSchedule(alarm);
                }
            });
        }

        public static IReadOnlyList<ScheduledToastNotification> ListAlarmToast()
        {
            return ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
        }

        public async static Task CancelAlarmToast(string id)
        {
            await Task.Run(() =>
            {
                var notifier = ToastNotificationManager.CreateToastNotifier();
                foreach (var toast in notifier.GetScheduledToastNotifications())
                {
                    if (toast.Tag.Equals(id))
                        notifier.RemoveFromSchedule(toast);
                }
            });
        }

        public async static Task CancelAllToast()
        {
            await Task.Run(() =>
            {
                var notifier = ToastNotificationManager.CreateToastNotifier();
                foreach (var toast in notifier.GetScheduledToastNotifications())
                {
                    notifier.RemoveFromSchedule(toast);
                }
            });
        }

        private static ToastContent GetTextToast(string title, string content, string comment)
        {
            return new ToastContent()
            {
                Scenario = ToastScenario.Reminder,

                //Launch = "",

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
                            },
                        },

                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = "ms-appx:///Assets/LargeTile.scale-100.png",
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        },

                        Attribution = new ToastGenericAttributionText()
                        {
                            Text = comment
                        }
                    }
                }
            };
        }

        private static ToastContent GetAlarmToast(string title, string content)
        {
            return new ToastContent()
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
                            Text = "Research Flow"
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
        }

        #endregion

        #region Tile

        public static void ShowTextTile(string title, string body)
        {
            var notification = new TileNotification(GetTileContent(title, body).GetXml());
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        }

        public static void CancelLiveTile()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
        }

        private static TileContent GetTileContent(string title, string body)
        {
            return new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = new TileBinding()
                    {
                        Branding = TileBranding.Name,
                        DisplayName = "Research Flow",
                        Content = new TileBindingContentAdaptive()
                        {
                            PeekImage = new TilePeekImage()
                            {
                                Source = "Assets/Square150x150Logo.scale-200.png"
                            },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintWrap = true
                                },

                                new AdaptiveText()
                                {
                                    Text = body,
                                    HintWrap = true,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    },

                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            PeekImage = new TilePeekImage()
                            {
                                Source = "Assets/Wide310x150Logo.scale-200.png"
                            },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintWrap = true
                                },

                                new AdaptiveText()
                                {
                                    Text = body,
                                    HintWrap = true,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    },

                    TileLarge = new TileBinding()
                    {
                        Branding = TileBranding.NameAndLogo,
                        DisplayName = "Research Flow",
                        Content = new TileBindingContentAdaptive()
                        {
                            PeekImage = new TilePeekImage()
                            {
                                Source = "Assets/LargeTile.scale-200.png"
                            },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintWrap = true,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },

                                new AdaptiveText()
                                {
                                    Text = body,
                                    HintWrap = true,
                                    HintStyle = AdaptiveTextStyle.SubtitleSubtle
                                }
                            }
                        }
                    }
                }
            };
        }

        #endregion

    }
}
