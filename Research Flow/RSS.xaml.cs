using LogicService.Application;
using LogicService.Data;
using LogicService.Security;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RSS : Page
    {
        public RSS()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            InitializeRSS();
        }

        private async void InitializeRSS()
        {
            try
            {
                FeedSources = await LocalStorage.ReadJsonAsync<ObservableCollection<RSSSource>>(
                    await LocalStorage.GetDataFolderAsync(), "rsslist");
            }
            catch
            {
                // for new user, remember to load default feed from file, not the follows
                FeedSources = new ObservableCollection<RSSSource>()
                {
                    new RSSSource{ ID = HashEncode.MakeMD5("https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DHydrogen%252BBond%2526target%253Ddefault%2526targetTab%253Dstd"),
                        Name = "Hydrogen Bond in ACS", Uri = "https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DHydrogen%252BBond%2526target%253Ddefault%2526targetTab%253Dstd", Star = 5, IsJournal = true },
                    new RSSSource{ ID = HashEncode.MakeMD5("https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DPedal%252BMotion%2526target%253Ddefault%2526targetTab%253Dstd"),
                        Name = "Pedal Motion in ACS", Uri = "https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DPedal%252BMotion%2526target%253Ddefault%2526targetTab%253Dstd", Star = 5, IsJournal = true },
                    new RSSSource{ ID = HashEncode.MakeMD5("http://feeds.aps.org/rss/recent/prl.xml"),
                        Name = "Physical Review Letters", Uri = "http://feeds.aps.org/rss/recent/prl.xml", Star = 5, IsJournal = true },
                    new RSSSource{ ID = HashEncode.MakeMD5("http://www.sciencenet.cn/xml/paper.aspx?di=7"),
                        Name = "科学网-数理科学", Uri = "http://www.sciencenet.cn/xml/paper.aspx?di=7", Star = 5, IsJournal = false}
                };
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "rsslist", FeedSources);
            }
            finally
            {
                feedSource_list.ItemsSource = FeedSources;
                feedSource_list.SelectedIndex = 0;
                shownRSS = feedSource_list.SelectedItem as RSSSource;
                LoadFeed(shownRSS);
            }
        }

        #region RSS Source

        public ObservableCollection<RSSSource> FeedSources { get; set; }

        private RSSSource modifiedRSS = null; // Used for item modification, not clicked item.
        private RSSSource shownRSS = null; // always un-null

        private void Add_Source(object sender, TappedRoutedEventArgs e) => source_panel.Visibility = Visibility.Visible;

        private void Modify_Source(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            foreach (var item in FeedSources)
            {
                if (item.ID.Equals((string)button.Tag))
                {
                    source_panel.Visibility = Visibility.Visible;
                    rssDelete.Visibility = Visibility;
                    modifiedRSS = item;

                    rssName.Text = item.Name;
                    rssUrl.Text = item.Uri;
                    rssStar.Value = item.Star;
                    isJournal.IsChecked = item.IsJournal;
                    rssUrl.IsReadOnly = true;
                }
            }
        }

        private async void Confirm_FeedSetting(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(rssUrl.Text))
            {
                RSSSource source = new RSSSource
                {
                    ID = HashEncode.MakeMD5(rssUrl.Text),
                    Name = rssName.Text,
                    Uri = rssUrl.Text,
                    Star = rssStar.Value,
                    IsJournal = (bool)(isJournal.IsChecked)
                };
                if (modifiedRSS != null)
                {
                    // create new and remain other data
                    source.LastUpdateTime = modifiedRSS.LastUpdateTime;
                    FeedSources[FeedSources.IndexOf(modifiedRSS)] = source;
                }
                else
                {
                    foreach (RSSSource item in FeedSources)
                    {
                        if (item == source) // require the Equals method
                        {
                            ApplicationMessage.SendMessage("RssException: There has been the same url.", ApplicationMessage.MessageType.InApp);
                            ClearSettings();
                            return;
                        }
                    }
                    FeedSources.Add(source);
                }
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "rsslist", FeedSources);
            }
            ClearSettings();
        }

        private async void Delete_RSSSetting(object sender, RoutedEventArgs e)
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
            FeedSources.Remove(modifiedRSS);
            LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "rsslist", FeedSources);
            // check
            FeedItem.DBDeleteByPID(modifiedRSS.ID);
            ClearSettings();
        }

        private void CancelInvokedHandler(IUICommand command) => ClearSettings();

        private void Leave_FeedSetting(object sender, RoutedEventArgs e) => ClearSettings();

        private void ClearSettings()
        {
            modifiedRSS = null;
            rssName.Text = "";
            rssUrl.Text = "";
            rssStar.Value = -1;
            isJournal.IsChecked = false;

            rssUrl.IsReadOnly = false;
            rssDelete.Visibility = Visibility.Collapsed;
            source_panel.Visibility = Visibility.Collapsed;
        }

        private void RSS_SourceClick(object sender, ItemClickEventArgs e)
        {
            shownRSS = e.ClickedItem as RSSSource;
            LoadFeed(shownRSS);
        }

        private void RSS_SourceRefresh(object sender, RoutedEventArgs e)
            => SearchFeed(shownRSS);

        #endregion

        #region RSS Feed

        private FeedItem selectedFeed = null;

        private void Feed_ItemClick(object sender, ItemClickEventArgs e)
        {
            var feed = e.ClickedItem as FeedItem;
            selectedFeed = feed;
            this.feedItem_detail.IsPaneOpen = true;

            feedTitle.Text = feed.Title + "\n";
            feedPublished.Text = feed.Published;
            feedSummary.Text = feed.Summary + "\n";
            if (!feed.Tags.Equals("Null"))
            {
                feedTags.Text = feed.Tags;
            }

            StringBuilder builder = new StringBuilder();
            foreach (XmlElement pair in FeedItem.GetNodes(feed.Nodes))
            {
                builder.AppendLine(pair.Name + " : " + pair.InnerText);
            }
            feedNodes.Text = builder.ToString();
        }

        private void Browse_Feed(object sender, RoutedEventArgs e)
            => this.Frame.Navigate(typeof(Search), selectedFeed.Link);

        private void Favorite_Feed(object sender, RoutedEventArgs e)
        {
            Crawlable.AddtoFavorite(new Crawlable()
            {
                ID = HashEncode.MakeMD5(selectedFeed.Link),
                ParentID = "Null",
                Text = selectedFeed.Title,
                Url = selectedFeed.Link,
                Content = selectedFeed.Summary,
                Tags = selectedFeed.Tags,
            });
        }

        private void Close_FeedDetail(object sender, RoutedEventArgs e)
            => feedItem_detail.IsPaneOpen = false;

        private void LoadFeed(RSSSource source)
        {
            try
            {
                feedItem_list.ItemsSource = FeedItem.DBSelectByPID(source.ID);
            }
            catch (Exception exception)
            {
                ApplicationMessage.SendMessage("RssException: " + exception, ApplicationMessage.MessageType.InApp);
            }
        }

        private void SearchFeed(RSSSource source)
        {
            int selectedFeedIndex = FeedSources.IndexOf(source); // now you can modify source while fetching feed

            waiting_feed.IsActive = true;
            RssService.GetRssItems(
                source.Uri,
                async (items) =>
                {
                    List<FeedItem> feeds = items as List<FeedItem>;
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        feedItem_list.ItemsSource = feeds;
                        waiting_feed.IsActive = false;
                    });
                    FeedItem.DBInsert(feeds);
                    FeedSources[selectedFeedIndex].LastUpdateTime = DateTime.Now;
                    LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "rsslist", FeedSources);
                },
                async (exception) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ApplicationMessage.SendMessage("RssException: " + exception, ApplicationMessage.MessageType.InApp);
                        waiting_feed.IsActive = false;
                    });
                }, null);
        }

        #endregion

    }
}
