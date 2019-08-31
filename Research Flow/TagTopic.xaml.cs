using LogicService.Helper;
using LogicService.Objects;
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
                tags = await LocalStorage.ReadJsonAsync<List<string>>(
                    await LocalStorage.GetDataAsync(), "taglist");
            }
            catch
            {
                // for new user, remember to load default feed from file, not the follows
                tags = new List<string>()
                {
                    "AnOS", "QCD", "QED", "Pedal Motion", "DNA", "AI", "Bond", "Computer", "Hydrogen", "Halogen", "OS"
                };
                LocalStorage.WriteJson(await LocalStorage.GetDataAsync(), "taglist", tags);
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
                    await LocalStorage.GetDataAsync(), "topiclist");
            }
            catch
            {
                // for new user, remember to load default feed from file, not the follows
                topics = new ObservableCollection<Topic>();
                LocalStorage.WriteJson(await LocalStorage.GetDataAsync(), "topiclist", topics);
            }
            finally
            {
                topiclist.ItemsSource = topics;
            }
        }

        #region Tag Management

        public List<string> tags { get; set; }

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

        #endregion

        #region Task Management

        public ObservableCollection<Topic> topics { get; set; }

        private Topic currentTopic = null;

        private void AddTopicSetting(object sender, TappedRoutedEventArgs e) => topicSetting.Visibility = Visibility.Visible;

        private async void SubmitTopic(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(topicTitle.Text))
            {
                Topic topic = new Topic();
                topic.Title = topicTitle.Text;
                if (startDate.Date != null)
                    topic.StartDate = startDate.Date.Value.UtcDateTime;
                if (endDate.Date != null)
                    topic.EndDate = endDate.Date.Value.UtcDateTime;
                topics.Add(topic);
                LocalStorage.WriteJson(await LocalStorage.GetDataAsync(), "topiclist", topics);

                // add tags
            }
            ClearTopicSetting();
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
            topics.Remove(currentTopic);
            LocalStorage.WriteJson(await LocalStorage.GetDataAsync(), "topiclist", topics);
            ClearTopicSetting();
        }

        private void CancelInvokedHandler(IUICommand command) => ClearTopicSetting();

        private void CancelTopic(object sender, RoutedEventArgs e) => ClearTopicSetting();

        private void ClearTopicSetting()
        {
            topicTitle.Text = "";
            startDate.Date = null;
            endDate.Date = null;

            topicDelete.Visibility = Visibility.Collapsed;
            topicSetting.Visibility = Visibility.Collapsed;
        }

        #endregion

    }

}
