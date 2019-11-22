using LogicService.Application;
using LogicService.Helper;
using LogicService.Objects;
using LogicService.Security;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TagTopic : Page
    {
        public TagTopic()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            InitializeTag();
            InitializeTopic();
        }

        private async void InitializeTag()
        {
            try
            {
                tags = await LocalStorage.ReadJsonAsync<HashSet<string>>(
                    await LocalStorage.GetDataFolderAsync(), "taglist");
            }
            catch
            {
                // for new user, remember to load default feed from file, not the follows
                tags = new HashSet<string>()
                {
                    "AnOS", "QCD", "QED", "Pedal Motion", "DNA", "AI", "Bond", "Computer", "Hydrogen", "Halogen", "OS"
                };
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "taglist", tags);
            }
            finally
            {
                LoadTagView();
            }
        }

        private async void InitializeTopic()
        {
            try
            {
                topics = await LocalStorage.ReadJsonAsync<ObservableCollection<Topic>>(
                    await LocalStorage.GetDataFolderAsync(), "topiclist");
            }
            catch
            {
                // for new user, remember to load default feed from file, not the follows
                topics = new ObservableCollection<Topic>();
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "topiclist", topics);
            }
            finally
            {
                topiclist.ItemsSource = topics;
            }
        }

        #region Tag Management

        private HashSet<string> tags;

        private void Flyout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        /// <summary>
        /// shall be removed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Flyout_Opened(object sender, object e)
            => tagEmbed.Focus(FocusState.Programmatic);

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
                    return "Other";
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

        private async void AddTagManually(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tagEmbed.Text))
            {
                tags.UnionWith(Topic.TagPicker(tagEmbed.Text));
                LoadTagView();
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "taglist", tags);
            }
        }

        #endregion

        #region Task Management

        private ObservableCollection<Topic> topics;

        private Topic currentTopic = null;

        private void LoadTaskView()
        {
            
        }

        private void AddTopicSetting(object sender, TappedRoutedEventArgs e) => topicSetting.Visibility = Visibility.Visible;

        private async void SubmitTopic(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(topicTitle.Text))
            {
                Topic topic = new Topic();
                //
                topic.ID = HashEncode.MakeMD5(DateTimeOffset.Now.ToString());
                topic.Title = topicTitle.Text;
                if (deadLine.Date != null)
                    topic.Deadline = deadLine.Date.Value;
                if (remindTime.Time != null)
                    topic.RemindTime = remindTime.Time;

                if (currentTopic != null) // modify
                {
                    int index = topics.IndexOf(currentTopic);
                    topics[index].Title = topic.Title;
                    topics[index].Deadline = topic.Deadline;
                    topics[index].RemindTime = topic.RemindTime;
                }
                else // add new
                {
                    topics.Add(topic);
                }

                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "topiclist", topics);

                // double check tags then add them
                int count = tags.Count;
                tags.UnionWith(Topic.TagPicker(topicTitle.Text));
                if (count != tags.Count)
                    LoadTagView();
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "taglist", tags);

                // register a task
                SubmitTopictoTask(topic);
            }
            ClearTopicSetting();
        }

        private void SubmitTopictoTask(Topic topic)
        {
            // TimeSpan.Zero equals 12:00 AM
            if (topic.Deadline == DateTimeOffset.MinValue && topic.RemindTime == TimeSpan.Zero) // an idea
            {
                ;
            }
            else if (topic.Deadline == DateTimeOffset.MinValue && topic.RemindTime != TimeSpan.Zero) // an alarm
            {
                ;
            }
            else if (topic.RemindTime == TimeSpan.Zero) // a deadline
            {
                ApplicationNotification.CancelAlarmToast(topic.ID);
                // make a setting about default time
                DateTimeOffset date = new DateTimeOffset(topic.Deadline.Year, topic.Deadline.Month, topic.Deadline.Day,
                    0, 0, 0, DateTimeOffset.Now.Offset);
                ApplicationNotification.ScheduleAlarmToast(topic.ID, "Research Topic", topic.Title, date);
            }
            else // a deadline with alarm
            {
                ApplicationNotification.CancelAlarmToast(topic.ID);
                DateTimeOffset date = new DateTimeOffset(topic.Deadline.Year, topic.Deadline.Month, topic.Deadline.Day,
                    topic.RemindTime.Hours, topic.RemindTime.Minutes, topic.RemindTime.Seconds, DateTimeOffset.Now.Offset);
                ApplicationNotification.ScheduleAlarmToast(topic.ID, "Research Topic", topic.Title, date);
            }
        }

        private async void DeleteTopic(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("You are about to delete application data, please tell me that is not true.", "Operation confirming");
            messageDialog.Commands.Add(new UICommand(
                "True",
                new UICommandInvokedHandler(this.DeleteInvokedHandler)));
            messageDialog.Commands.Add(new UICommand(
                "Joke",
                new UICommandInvokedHandler(this.CancelInvokedHandler)));

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync();
        }

        private async void DeleteInvokedHandler(IUICommand command)
        {
            // cancel notification
            ApplicationNotification.CancelAlarmToast(currentTopic.ID);

            topics.Remove(currentTopic);
            LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "topiclist", topics);
            ClearTopicSetting();
        }

        private void CancelInvokedHandler(IUICommand command) => ClearTopicSetting();

        private void CancelTopic(object sender, RoutedEventArgs e) => ClearTopicSetting();

        private void ClearTopicSetting()
        {
            currentTopic = null;
            topicTitle.Text = "";
            deadLine.Date = null;
            remindTime.Time = TimeSpan.Zero;

            topicDelete.Visibility = Visibility.Collapsed;
            topicSetting.Visibility = Visibility.Collapsed;
        }

        private void Topiclist_ItemClick(object sender, ItemClickEventArgs e)
        {
            topicDelete.Visibility = Visibility.Visible;
            topicSetting.Visibility = Visibility.Visible;

            currentTopic = e.ClickedItem as Topic;
            topicTitle.Text = currentTopic.Title;
            if (currentTopic.Deadline != DateTimeOffset.MinValue)
                deadLine.Date = currentTopic.Deadline;
            if (currentTopic.RemindTime != TimeSpan.Zero)
                remindTime.Time = currentTopic.RemindTime;
        }

        #endregion

        private void TextBlock_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            
        }
    }

}
