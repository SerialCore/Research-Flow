using LogicService.Application;
using LogicService.Data;
using LogicService.Security;
using LogicService.Storage;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
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
            InitializeTopic();
        }

        private async void InitializeTopic()
        {
            try
            {
                topics = await LocalStorage.ReadJsonAsync<ObservableCollection<Topic>>(
                    await LocalStorage.GetDataFolderAsync(), "topic.list");
            }
            catch
            {
                // for new user, remember to load default feed from file, not the follows
                topics = new ObservableCollection<Topic>();
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "topic.list", topics);
            }
            finally
            {
                topiclist.ItemsSource = topics;
            }
        }

        private ObservableCollection<Topic> topics;

        private Topic currentTopic = null;

        private void AddTopicSetting(object sender, RoutedEventArgs e) => topicSetting.IsPaneOpen = true;

        private void ColorSpot1(object sender, RoutedEventArgs e) => topicTitle.Background = (sender as AppBarButton).Background;

        private void ColorSpot2(object sender, RoutedEventArgs e) => topicTitle.Background = (sender as AppBarButton).Background;

        private void ColorSpot3(object sender, RoutedEventArgs e) => topicTitle.Background = (sender as AppBarButton).Background;

        private void ColorSpot4(object sender, RoutedEventArgs e) => topicTitle.Background = (sender as AppBarButton).Background;

        private void ColorSpot5(object sender, RoutedEventArgs e) => topicTitle.Background = (sender as AppBarButton).Background;

        private void ColorSpot6(object sender, RoutedEventArgs e) => topicTitle.Background = (sender as AppBarButton).Background;

        private void ColorSpot7(object sender, RoutedEventArgs e) => topicTitle.Background = (sender as AppBarButton).Background;

        private async void SubmitTopic(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(topicTitle.Text))
            {
                Topic topic = new Topic();
                topic.Title = topicTitle.Text;
                topic.Color = (topicTitle.Background as SolidColorBrush).Color.ToString();
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

                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "topic.list", topics);
                Topic.SaveTag(topicTitle.Text);
                
                // register a task
                SubmitTopictoTask(topic);
            }
            ClearTopicSetting();
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
                    ApplicationMessage.SendMessage("TopicWarning: Research Flow does not offer Time-Machine", ApplicationMessage.MessageType.InApp);
                }
            }
        }

        private async void DeleteTopic(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("You are about to delete application data, please tell me that is not true.", "Operation confirming");
            messageDialog.Commands.Add(new UICommand("True", new UICommandInvokedHandler(this.DeleteInvokedHandler)));
            messageDialog.Commands.Add(new UICommand("Joke", new UICommandInvokedHandler(this.CancelInvokedHandler)));

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync();
        }

        private async void DeleteInvokedHandler(IUICommand command)
        {
            string topicID = currentTopic.ID;
            topics.Remove(currentTopic);
            LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "topic.list", topics);
            ClearTopicSetting();

            // cancel notification
            await ApplicationNotification.CancelAlarmToast(topicID);
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
            topicSetting.IsPaneOpen = false;
        }

        private void Topiclist_ItemClick(object sender, ItemClickEventArgs e)
        {
            ClearTopicSetting();
            topicDelete.Visibility = Visibility.Visible;
            topicSetting.IsPaneOpen = true;

            currentTopic = e.ClickedItem as Topic;
            topicTitle.Text = currentTopic.Title;
            string hex = currentTopic.Color.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            topicTitle.Background = new SolidColorBrush(Color.FromArgb(a, r, g, b));
            if (currentTopic.Deadline != DateTimeOffset.MinValue)
                deadLine.Date = currentTopic.Deadline;
            if (currentTopic.RemindTime != TimeSpan.Zero)
                remindTime.Time = currentTopic.RemindTime;
        }

    }
}
