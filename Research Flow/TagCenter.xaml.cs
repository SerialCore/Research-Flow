using LogicService.Data;
using LogicService.Storage;
using System;
using System.Collections.Generic;
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
    public sealed partial class TagCenter : Page
    {
        public TagCenter()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InitializeTag();
        }

        private async void InitializeTag()
        {
            try
            {
                tags = await LocalStorage.ReadJsonAsync<HashSet<string>>(
                    await LocalStorage.GetDataFolderAsync(), "tag.list");
            }
            catch
            {
                tags = new HashSet<string>() // these are system tags
                {
                    "@Search"/*search words in engine*/, "@Remind"/*make user remind of content*/,
                    "@Concentrate",/*make user concentrate on content*/
                };
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "tag.list", tags);
            }
            finally
            {
                LoadTagView();
            }
        }

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

        private async void AddTagManually(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(tagEmbed.Text))
            {
                tags.UnionWith(Topic.TagPick(tagEmbed.Text));
                LoadTagView();
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "tag.list", tags);
            }
        }

        private async void DeleteTagManually(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tagPanelTitle.Text))
            {
                tags.Remove(tagPanelTitle.Text);
                LoadTagView();
                ClearTagPanel();
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "tag.list", tags);
            }
        }

        private async void Taglist_ItemClick(object sender, ItemClickEventArgs e)
        {
            string tag = e.ClickedItem as string;
            ClearTagPanel();
            tagPanelTitle.Text = tag;
            // topicTags should be handled immediately and there must exist tagPanelTitle.Text
            // Pivot_SelectionChanged is an async process thus tagPanelTitle.Text could be null
            if (topicTags.ItemsSource == null && !string.IsNullOrEmpty(tagPanelTitle.Text))
            {
                List<Topic> selectTopic = new List<Topic>();

                List<Topic> topics = new List<Topic>();
                try
                {
                    topics = await LocalStorage.ReadJsonAsync<List<Topic>>(
                        await LocalStorage.GetDataFolderAsync(), "topic.list");
                }
                catch { }
                foreach (Topic topic in topics)
                {
                    if (topic.Title.Contains('#' + tagPanelTitle.Text + '#'))
                        selectTopic.Add(topic);
                }
                topicTags.ItemsSource = selectTopic;
            }
        }

        private void ClearTagPanel()
        {
            tagPanelTitle.Text = "";
            topicTags.ItemsSource = null;
            feedTags.ItemsSource = null;
            crawlTags.ItemsSource = null;
            paperTags.ItemsSource = null;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as Pivot).SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    if (feedTags.ItemsSource == null && !string.IsNullOrEmpty(tagPanelTitle.Text))
                        feedTags.ItemsSource = Feed.DBSelectByTag(tagPanelTitle.Text);
                    break;
                case 2:
                    if (crawlTags.ItemsSource == null && !string.IsNullOrEmpty(tagPanelTitle.Text))
                        crawlTags.ItemsSource = Crawlable.DBSelectByTag(tagPanelTitle.Text);
                    break;
                case 3:
                    if (paperTags.ItemsSource == null && !string.IsNullOrEmpty(tagPanelTitle.Text))
                        paperTags.ItemsSource = Paper.DBSelectByTag(tagPanelTitle.Text);
                    break;
            }
        }

    }

}
