using LogicService.Application;
using LogicService.Data;
using LogicService.Helper;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Core;
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
    public sealed partial class FeedCollector : Page
    {
        public FeedCollector()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            InitializeFeed();
        }

        private async void InitializeFeed()
        {
            try
            {
                FeedSources = await LocalStorage.ReadJsonAsync<ObservableCollection<FeedSource>>(LocalStorage.GetLocalCacheFolder(), "rss.list");
            }
            catch
            {
                FeedSources = new ObservableCollection<FeedSource>()
                {
                    new FeedSource{ ID = HashEncode.MakeMD5("http://feeds.aps.org/rss/recent/prl.xml"),
                        Name = "Physical Review Letters", Uri = "http://feeds.aps.org/rss/recent/prl.xml", Star = 5, IsRSS = true },
                    new FeedSource{ ID = HashEncode.MakeMD5("http://feeds.aps.org/rss/recent/prd.xml"),
                        Name = "Physical Review D", Uri = "http://feeds.aps.org/rss/recent/prd.xml", Star = 5, IsRSS = true },
                    new FeedSource{ ID = HashEncode.MakeMD5("https://www.science.org/rss/express.xml"),
                        Name = "Science Express", Uri = "https://www.science.org/rss/express.xml", Star = 3, IsRSS = true },
                    new FeedSource{ ID = HashEncode.MakeMD5("http://feeds.nature.com/nmeth/rss/current"),
                        Name = "Nature Method", Uri = "http://feeds.nature.com/nmeth/rss/current", Star = 3, IsRSS = true },
                };
                LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "rss.list", FeedSources);
            }
            finally
            {
                feedSource_list.ItemsSource = FeedSources;
                feedSource_list.SelectedIndex = 0;
                querytype.ItemsSource = new List<string> { "Title", "TitleSummary", "Authors", "Published", "Identifier" };
                querytype.SelectedIndex = 0;
                shownRSS = feedSource_list.SelectedItem as FeedSource;
                LoadFeed(shownRSS);
            }
        }

        #region Feed Source

        public ObservableCollection<FeedSource> FeedSources { get; set; }

        private FeedSource modifiedRSS = null; // Used for item modification, not clicked item.
        private FeedSource shownRSS = null; // always un-null

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
                    isRSS.IsChecked = item.IsRSS;
                    rssUrl.IsReadOnly = true;
                }
            }
        }

        private void Confirm_FeedSetting(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(rssUrl.Text))
            {
                FeedSource source = new FeedSource
                {
                    ID = HashEncode.MakeMD5(rssUrl.Text),
                    Name = rssName.Text,
                    Uri = rssUrl.Text,
                    Star = rssStar.Value,
                    IsRSS = (bool)(isRSS.IsChecked)
                };
                if (modifiedRSS != null)
                {
                    // create new and remain other data
                    FeedSources[FeedSources.IndexOf(modifiedRSS)] = source;
                }
                else
                {
                    foreach (FeedSource item in FeedSources)
                    {
                        if (item == source) // require the Equals method
                        {
                            ApplicationMessage.SendMessage(new MessageEventArgs { Title = "RssWarning", Content = "There has been the same url.", Type = MessageType.InApp, Time = DateTimeOffset.Now });
                            ClearSettings();
                            return;
                        }
                    }
                    FeedSources.Add(source);
                }
                LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "rss.list", FeedSources);
            }
            ClearSettings();
        }

        private async void Delete_RSSSetting(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Delete application data?";
            dialog.PrimaryButtonText = "Yeah";
            dialog.CloseButtonText = "Forget it";
            dialog.DefaultButton = ContentDialogButton.Primary;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                FeedSources.Remove(modifiedRSS);
                LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "rss.list", FeedSources);
                // check
                Feed.DBDeleteByPID(modifiedRSS.ID);
                ClearSettings();
            }
        }

        private void Leave_FeedSetting(object sender, RoutedEventArgs e) => ClearSettings();

        private void ClearSettings()
        {
            modifiedRSS = null;
            rssName.Text = "";
            rssUrl.Text = "";
            rssStar.Value = -1;
            isRSS.IsChecked = false;

            rssUrl.IsReadOnly = false;
            rssDelete.Visibility = Visibility.Collapsed;
            source_panel.Visibility = Visibility.Collapsed;
        }

        private void RSS_SourceClick(object sender, ItemClickEventArgs e)
        {
            shownRSS = e.ClickedItem as FeedSource;
            LoadFeed(shownRSS);
        }

        private void RSS_SourceRefresh(object sender, RoutedEventArgs e)
            => SearchFeed(shownRSS);

        #endregion

        #region Feed

        private Feed selectedFeed = null;

        private void SearchFeed_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            // { "Title", "TitleSummary", "Authors", "Published", "Identifier" }
            switch (querytype.SelectedIndex)
            {
                case 0:
                    feedItem_list.ItemsSource = Feed.DBSelectByTitle(feedQuery.Text);
                    break;
                case 1:
                    feedItem_list.ItemsSource = Feed.DBSelectByText(feedQuery.Text);
                    break;
                case 2:
                    feedItem_list.ItemsSource = Feed.DBSelectByAuthor(feedQuery.Text);
                    break;
                case 3:
                    feedItem_list.ItemsSource = Feed.DBSelectByPublished(feedQuery.Text);
                    break;
                case 4:
                    feedItem_list.ItemsSource = Feed.DBSelectByIdentifier(feedQuery.Text);
                    break;
            }
        }

        private void Feed_ItemClick(object sender, ItemClickEventArgs e)
        {
            var feed = e.ClickedItem as Feed;
            selectedFeed = feed;
            this.feedDetail.IsOpen = true;

            feedTitle.Text = feed.Title + "\n";
            feedLink.Content = feed.Link;
            feedLink.NavigateUri = new Uri(feed.Link);
            feedPublished.Text = feed.Published;
            feedSummary.Text = feed.Summary + "\n";
        }

        private void Browse_Feed(object sender, RoutedEventArgs e)
            => this.Frame.Navigate(typeof(SearchEngine), selectedFeed.Link);

        private void Favorite_Feed(object sender, RoutedEventArgs e)
            => Feed.DBInsertBookmark(selectedFeed);

        private void LoadFeed(FeedSource source)
        {
            try
            {
                feedItem_list.ItemsSource = Feed.DBSelectByPID(source.ID);
            }
            catch (Exception exception)
            {
                ApplicationMessage.SendMessage(new MessageEventArgs { Title = "RssException", Content = exception.Message, Type = MessageType.InApp, Time = DateTimeOffset.Now });
            }
        }

        private void SearchFeed(FeedSource source)
        {
            int selectedFeedIndex = FeedSources.IndexOf(source); // now you can modify source while fetching feed

            waiting_feed.IsActive = true;
            FeedService.BeginGetFeed(
                source.Uri,
                async (items) =>
                {
                    List<Feed> feeds = items as List<Feed>;
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        feedItem_list.ItemsSource = feeds;
                        waiting_feed.IsActive = false;
                    });
                    Feed.DBInsert(feeds);
                },
                async (exception) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ApplicationMessage.SendMessage(new MessageEventArgs { Title = "RssException", Content = exception, Type = MessageType.InApp, Time = DateTimeOffset.Now });
                        waiting_feed.IsActive = false;
                    });
                }, null);
        }

        #endregion

    }
}
