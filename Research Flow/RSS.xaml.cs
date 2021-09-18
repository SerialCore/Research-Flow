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
                FeedSources = await LocalStorage.ReadJsonAsync<ObservableCollection<FeedSource>>(LocalStorage.GetLocalCacheFolder(), "rss.list");
            }
            catch
            {
                FeedSources = new ObservableCollection<FeedSource>()
                {
                    new FeedSource{ ID = HashEncode.MakeMD5("http://feeds.aps.org/rss/recent/prl.xml"),
                        Name = "Physical Review Letters", Uri = "http://feeds.aps.org/rss/recent/prl.xml", Star = 5, IsJournal = true },
                    new FeedSource{ ID = HashEncode.MakeMD5("http://feeds.aps.org/rss/recent/prd.xml"),
                        Name = "Physical Review D", Uri = "http://feeds.aps.org/rss/recent/prd.xml", Star = 5, IsJournal = true },
                };
                LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "rss.list", FeedSources);
            }
            finally
            {
                feedSource_list.ItemsSource = FeedSources;
                feedSource_list.SelectedIndex = 0;
                shownRSS = feedSource_list.SelectedItem as FeedSource;
                LoadFeed(shownRSS);
            }
        }

        #region RSS Source

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
                    isJournal.IsChecked = item.IsJournal;
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
                    foreach (FeedSource item in FeedSources)
                    {
                        if (item == source) // require the Equals method
                        {
                            ApplicationMessage.SendMessage(new ShortMessage { Title = "RssWarning", Content = "There has been the same url.", Time = DateTimeOffset.Now },
                                ApplicationMessage.MessageType.InApp);
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
            var messageDialog = new MessageDialog("You are about to delete application data, please tell me that is not true.");
            messageDialog.Commands.Add(new UICommand("True", new UICommandInvokedHandler(this.DeleteInvokedHandler)));
            messageDialog.Commands.Add(new UICommand("Joke", new UICommandInvokedHandler(this.CancelInvokedHandler)));

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync();
        }

        private void DeleteInvokedHandler(IUICommand command)
        {
            FeedSources.Remove(modifiedRSS);
            LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "rss.list", FeedSources);
            // check
            Feed.DBDeleteByPID(modifiedRSS.ID);
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
            shownRSS = e.ClickedItem as FeedSource;
            LoadFeed(shownRSS);
        }

        private void RSS_SourceRefresh(object sender, RoutedEventArgs e)
            => SearchFeed(shownRSS);

        #endregion

        #region RSS Feed

        private Feed selectedFeed = null;

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

        private void Flow_Feed(object sender, RoutedEventArgs e)
            => this.Frame.Navigate(typeof(PaperBox), selectedFeed);

        private void Favorite_Feed(object sender, RoutedEventArgs e)
        {
            Crawlable.AddtoFavorite(new Crawlable()
            {
                ID = HashEncode.MakeMD5(selectedFeed.Link),
                ParentID = "",
                Text = selectedFeed.Title,
                Url = selectedFeed.Link,
                Content = selectedFeed.Summary,
            });
        }

        private void LoadFeed(FeedSource source)
        {
            try
            {
                feedItem_list.ItemsSource = Feed.DBSelectByPID(source.ID);
            }
            catch (Exception exception)
            {
                ApplicationMessage.SendMessage(new ShortMessage { Title = "RssException", Content = exception.Message, Time = DateTimeOffset.Now },
                    ApplicationMessage.MessageType.InApp);
            }
        }

        private void SearchFeed(FeedSource source)
        {
            int selectedFeedIndex = FeedSources.IndexOf(source); // now you can modify source while fetching feed

            waiting_feed.IsActive = true;
            RssService.BeginGetFeed(
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
                    FeedSources[selectedFeedIndex].LastUpdateTime = DateTime.Now;
                    LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "rss.list", FeedSources);
                },
                async (exception) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ApplicationMessage.SendMessage(new ShortMessage { Title = "RssException", Content = exception, Time = DateTimeOffset.Now }, 
                            ApplicationMessage.MessageType.InApp);
                        waiting_feed.IsActive = false;
                    });
                }, null);
        }

        #endregion

    }
}
