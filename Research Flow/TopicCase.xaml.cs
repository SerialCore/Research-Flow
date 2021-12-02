using LogicService.Application;
using LogicService.Data;
using LogicService.Helper;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TopicCase : Page
    {
        public TopicCase()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UpdateTopic();
            UpdateTag();
        }

        private async void UpdateTopic()
        {
            try
            {
                topics = await LocalStorage.ReadJsonAsync<ObservableCollection<Topic>>(LocalStorage.GetLocalCacheFolder(), "topic.list");
            }
            catch
            {
                topics = new ObservableCollection<Topic>()
                {
                    new Topic(){ ID = HashEncode.MakeMD5(DateTimeOffset.Now.ToString()), Title = "@Search#glueball#", 
                        Completeness = 33, Deadline = DateTimeOffset.Now, RemindTime = TimeSpan.FromDays(1)}
                };
                LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "topic.list", topics);
            }
            finally
            {
                topiclist.ItemsSource = topics;
            }
        }

        private async void UpdateTag()
        {
            try
            {
                tags = await LocalStorage.ReadJsonAsync<HashSet<string>>(LocalStorage.GetLocalCacheFolder(), "tag.list");
            }
            catch
            {
                tags = new HashSet<string>() // these are system tags
                {
                    "@Search", "glueball"
                };
                LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "tag.list", tags);
            }
            finally
            {
                LoadTagView();
            }
        }

        #region Topic Managment

        private ObservableCollection<Topic> topics;

        private Topic currentTopic = null;

        private void AddTopicSetting(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ClearTopicSetting();
            topicSetting.IsOpen = true;
        }

        private void SubmitTopic(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(topicTitle.Text))
            {
                Topic topic = new Topic();
                topic.Title = topicTitle.Text;
                topic.Completeness = completeness.Value;
                if (deadLine.Date != null)
                    topic.Deadline = deadLine.Date.Value;
                if (remindTime.Time != null)
                    topic.RemindTime = remindTime.Time;

                if (currentTopic != null) // modify
                {
                    int index = topics.IndexOf(currentTopic);
                    topic.ID = topics[index].ID;
                    topics.RemoveAt(index);
                }
                else // add new
                {
                    topic.ID = HashEncode.MakeMD5(DateTimeOffset.Now.ToString());
                }
                topics.Insert(0, topic);

                LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "topic.list", topics);
                Topic.SaveTag(topicTitle.Text);

                // register a task
                SubmitTopictoTask(topic);
            }
        }

        private async void SubmitTopictoTask(Topic topic)
        {
            // TimeSpan.Zero equals 12:00 AM
            if (topic.Deadline == DateTimeOffset.MinValue && topic.RemindTime == TimeSpan.Zero) // an idea
            {
                ;
            }
            else if (topic.Deadline == DateTimeOffset.MinValue && topic.RemindTime != TimeSpan.Zero) // an alarm
            {
                await ApplicationNotification.CancelAlarmToast(topic.ID);
                DateTimeOffset dateTime = new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month, DateTimeOffset.Now.Day,
                    topic.RemindTime.Hours, topic.RemindTime.Minutes, topic.RemindTime.Seconds, DateTimeOffset.Now.Offset);
                if (DateTimeOffset.Now > dateTime)
                    await ApplicationNotification.ScheduleRepeatAlarmToast(topic.ID, "Research Topic", topic.Title, dateTime.AddDays(1), TimeSpan.FromDays(1), 40);
                else
                    await ApplicationNotification.ScheduleRepeatAlarmToast(topic.ID, "Research Topic", topic.Title, dateTime, TimeSpan.FromDays(1), 40);
            }
            else if (topic.RemindTime == TimeSpan.Zero) // a deadline
            {
                await ApplicationNotification.CancelAlarmToast(topic.ID);
                // make a setting about default time
                DateTimeOffset dateTime = new DateTimeOffset(topic.Deadline.Year, topic.Deadline.Month, topic.Deadline.Day,
                    0, 0, 0, DateTimeOffset.Now.Offset);
                ApplicationNotification.ScheduleAlarmToast(topic.ID, "Research Topic", topic.Title, dateTime);
            }
            else // a deadline with alarm
            {
                try
                {
                    await ApplicationNotification.CancelAlarmToast(topic.ID);
                    DateTimeOffset dateTime = new DateTimeOffset(topic.Deadline.Year, topic.Deadline.Month, topic.Deadline.Day,
                        topic.RemindTime.Hours, topic.RemindTime.Minutes, topic.RemindTime.Seconds, DateTimeOffset.Now.Offset);
                    ApplicationNotification.ScheduleAlarmToast(topic.ID, "Research Topic", topic.Title, dateTime);
                }
                catch (ArgumentException)
                {
                    ApplicationMessage.SendMessage(new MessageEventArgs { Title = "TopicWarning", Content = "Research Flow does not offer Time-Machine", Type = MessageType.InApp, Time = DateTimeOffset.Now });
                }
            }
        }

        private async void DeleteTopic(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Delete application data?";
            dialog.PrimaryButtonText = "Yeah";
            dialog.CloseButtonText = "Forget it";
            dialog.DefaultButton = ContentDialogButton.Primary;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                string topicID = currentTopic.ID;
                topics.Remove(currentTopic);
                LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "topic.list", topics);
                ClearTopicSetting();

                // cancel notification
                await ApplicationNotification.CancelAlarmToast(topicID);
            }
        }

        private void CancelTopic(object sender, RoutedEventArgs e) => ClearTopicSetting();

        private void ClearTopicSetting()
        {
            currentTopic = null;
            topicTitle.Text = "";
            completeness.Value = 0;
            deadLine.Date = null;
            remindTime.Time = TimeSpan.Zero;

            topicDelete.Visibility = Visibility.Collapsed;
            topicSetting.IsOpen = false;
        }

        private void Topiclist_ItemClick(object sender, ItemClickEventArgs e)
        {
            ClearTopicSetting();
            topicDelete.Visibility = Visibility.Visible;
            topicSetting.IsOpen = true;

            currentTopic = e.ClickedItem as Topic;
            topicTitle.Text = currentTopic.Title;
            completeness.Value = currentTopic.Completeness;
            if (currentTopic.Deadline != DateTimeOffset.MinValue)
                deadLine.Date = currentTopic.Deadline;
            if (currentTopic.RemindTime != TimeSpan.Zero)
                remindTime.Time = currentTopic.RemindTime;
        }

        private void TopicList_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "topic.list", topics);
        }

        #endregion

        #region Tag

        private HashSet<string> tags;

        private void LoadTagView()
        {
            Func<string, string> AlphaKey = (tag) =>
            {
                char head = tag[0];
                if (head >= '0' && head <= '9')
                    return "#";
                else if (head >= 'A' && head <= 'Z' || head >= 'a' && head <= 'z')
                    return head.ToString().ToUpper();
                else
                    return "@";
            };

            var groups = from t in tags
                         orderby t
                         group t by AlphaKey(t);

            CollectionViewSource collectionVS = new CollectionViewSource();
            collectionVS.IsSourceGrouped = true;
            collectionVS.Source = groups;
            taglist.ItemsSource = collectionVS.View;
            tagKlist.ItemsSource = collectionVS.View.CollectionGroups;
        }

        private void AddTagManually(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(tagEmbed.Text))
            {
                tags.UnionWith(Topic.TagPick(tagEmbed.Text));
                LoadTagView();
                LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "tag.list", tags);
            }
        }

        private void Taglist_ItemClick(object sender, ItemClickEventArgs e)
        {
            string tag = e.ClickedItem as string;
        }

        #endregion

    }
}
