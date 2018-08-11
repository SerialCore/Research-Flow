using LogicService.Encapsulates;
using LogicService.Security;
using LogicService.Services;
using LogicService.Storage;
using Microsoft.Toolkit.Services.Bing;
using Research_Flow.Pages.SubPages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Search : Page
    {
        public Search()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            InitializeData();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private async void InitializeData()
        {
            //there must be feed source file
            try
            {
                FeedSources = await LocalStorage.ReadObjectAsync<ObservableCollection<FeedSource>>(
                    await LocalStorage.GetFeedsAsync(), "RSS") as ObservableCollection<FeedSource>;
            }
            catch
            {
                FeedSources = new ObservableCollection<FeedSource>()
                {
                    new FeedSource{ID=TripleDES.MakeMD5("https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DHydrogen%252BBond%2526target%253Ddefault%2526targetTab%253Dstd"),
                        Name="Hydrogen Bond in ACS",Uri="https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DHydrogen%252BBond%2526target%253Ddefault%2526targetTab%253Dstd",MaxCount=50,DaysforUpdate=5,Star=5,IsJournal=true},
                    new FeedSource{ID=TripleDES.MakeMD5("https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DPedal%252BMotion%2526target%253Ddefault%2526targetTab%253Dstd"),
                        Name="Pedal Motion in ACS",Uri="https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DPedal%252BMotion%2526target%253Ddefault%2526targetTab%253Dstd",MaxCount=50,DaysforUpdate=5,Star=5,IsJournal=true},
                    new FeedSource{ID=TripleDES.MakeMD5("https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DPaul%252BPopelier%2526target%253Ddefault%2526targetTab%253Dstd"),
                        Name="Paul L. A. Popelier in ACS",Uri="https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DPaul%252BPopelier%2526target%253Ddefault%2526targetTab%253Dstd",MaxCount=50,DaysforUpdate=5,Star=5,IsJournal=true},
                    new FeedSource{ID=TripleDES.MakeMD5("https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DFuyang%252BLi%2526target%253Ddefault%2526targetTab%253Dstd"),
                        Name="Fuyang Li in ACS",Uri="https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DFuyang%252BLi%2526target%253Ddefault%2526targetTab%253Dstd",MaxCount=50,DaysforUpdate=5,Star=5,IsJournal=true},
                    new FeedSource{ID=TripleDES.MakeMD5("http://feeds.aps.org/rss/recent/prl.xml").GetHashCode().ToString(),
                        Name="Physical Review Letters",Uri="http://feeds.aps.org/rss/recent/prl.xml",MaxCount=50,DaysforUpdate=5,Star=5,IsJournal=true},
                    new FeedSource{ID=TripleDES.MakeMD5("http://www.sciencenet.cn/xml/paper.aspx?di=7"),
                        Name="科学网-数理科学",Uri="http://www.sciencenet.cn/xml/paper.aspx?di=7",MaxCount=50,DaysforUpdate=5,Star=5,IsJournal=false}
                };
                await LocalStorage.WriteObjectAsync(await LocalStorage.GetFeedsAsync(), "RSS", FeedSources);
            }
            feedsource_list.ItemsSource = FeedSources;

            // Bing configure
            country.ItemsSource = Enum.GetValues(typeof(BingCountry));
            language.ItemsSource = Enum.GetValues(typeof(BingLanguage));
            queryType.ItemsSource = Enum.GetValues(typeof(BingQueryType));
            country.SelectedItem = BingCountry.None;
            language.SelectedItem = BingLanguage.None;
            queryType.SelectedItem = BingQueryType.Search;
        }

        #region RSS

        public ObservableCollection<FeedSource> FeedSources { get; set; }

        // Used for item modification, not clicked item.
        private FeedSource currentFeed = null;
        // Used for item clicking, and conflict with modification
        private int selectedFeedIndex;

        private void Open_Source(object sender, RoutedEventArgs e) => feedsource_view.IsPaneOpen = !feedsource_view.IsPaneOpen;

        private void Add_Source(object sender, RoutedEventArgs e) => source_panel.Visibility = Visibility.Visible;

        private void Modify_Source(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            foreach (var item in FeedSources)
            {
                if (item.ID.Equals((string)(button.Tag)))
                {
                    source_panel.Visibility = Visibility.Visible;
                    currentFeed = item;
                    feedName.Text = item.Name;
                    feedUrl.Text = item.Uri;
                    feedCount.Value = item.MaxCount;
                    feedDays.Value = item.DaysforUpdate;
                    feedStar.Value = item.Star;
                    isJournal.IsChecked = item.IsJournal;
                    
                    feedUrl.IsReadOnly = true;
                    feedDelete.Visibility = Visibility;
                    source_panel.Visibility = Visibility.Visible;
                }
            }
        }

        private async void Confirm_FeedSetting(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(feedUrl.Text))
            {
                FeedSource source = new FeedSource
                {
                    ID = TripleDES.MakeMD5(feedUrl.Text),
                    Name = feedName.Text,
                    Uri = feedUrl.Text,
                    MaxCount = (int)(feedCount.Value),
                    DaysforUpdate = feedDays.Value,
                    Star = feedStar.Value,
                    IsJournal = (bool)(isJournal.IsChecked)
                };
                if (currentFeed != null)
                {
                    // create new and remain other data
                    source.LastUpdateTime = currentFeed.LastUpdateTime;
                    FeedSources[FeedSources.IndexOf(currentFeed)] = source;
                }
                else
                {
                    foreach (FeedSource item in FeedSources)
                    {
                        if (item.Uri.Equals(feedUrl.Text))
                        {
                            InAppNotification.Show("There has been the same url.");
                            ClearSettings();
                            return;
                        }
                    }
                    FeedSources.Add(source);
                }
                await LocalStorage.WriteObjectAsync(await LocalStorage.GetFeedsAsync(), "RSS", FeedSources);
            }
            ClearSettings();
        }

        private async void Delete_FeedSetting(object sender, RoutedEventArgs e)
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
            FeedSources.Remove(currentFeed);
            await LocalStorage.WriteObjectAsync(await LocalStorage.GetFeedsAsync(), "RSS", FeedSources);
            try
            {
                LocalStorage.DeleteFile(await LocalStorage.GetFeedsAsync(), currentFeed.ID);
            }
            catch { }
            ClearSettings();
        }

        private void CancelInvokedHandler(IUICommand command) => ClearSettings();

        private void Leave_FeedSetting(object sender, RoutedEventArgs e) => ClearSettings();

        private void ClearSettings()
        {
            currentFeed = null;
            feedName.Text = "";
            feedUrl.Text = "";
            feedCount.Value = 20;
            feedStar.Value = -1;
            isJournal.IsChecked = false;
            
            feedUrl.IsReadOnly = false;
            feedDelete.Visibility = Visibility.Collapsed;
            source_panel.Visibility = Visibility.Collapsed;
        }

        private void RSS_SourceClick(object sender, ItemClickEventArgs e)
            => SearchRss((e.ClickedItem as FeedSource));

        private void RSS_ItemClick(object sender, ItemClickEventArgs e)
            => this.Frame.Navigate(typeof(WebPage), (e.ClickedItem as FeedItem).Link);

        private async void SearchRss(FeedSource source)
        {
            // now you can modify source while fetching feed
            selectedFeedIndex = FeedSources.IndexOf(source);
            TimeSpan ts1 = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan ts2 = new TimeSpan(source.LastUpdateTime.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            if (ts.Days >= source.DaysforUpdate)
            {
                waiting_feed.IsActive = true;
                RssService.GetRssItems(
                    source.Uri,
                    async (items) =>
                    {
                        List<FeedItem> feeds = items as List<FeedItem>;
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            feeditem_list.ItemsSource = feeds;
                            waiting_feed.IsActive = false;
                        });
                        if (feeds.Count > source.MaxCount)
                            feeds.RemoveRange(source.MaxCount, feeds.Count - source.MaxCount);
                        await LocalStorage.WriteObjectAsync(await LocalStorage.GetFeedsAsync(), source.ID, items, "blamder" + source.ID);
                        FeedSources[selectedFeedIndex].LastUpdateTime = DateTime.Now;
                        await LocalStorage.WriteObjectAsync(await LocalStorage.GetFeedsAsync(), "RSS", FeedSources);
                    },
                    async (exception) =>
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            InAppNotification.Show("RssException: " + exception);
                            waiting_feed.IsActive = false;
                        });
                    }, null);
            }
            else
            {
                List<FeedItem> feedItems = await LocalStorage.ReadObjectAsync<List<FeedItem>>(
                    await LocalStorage.GetFeedsAsync(), source.ID, "blamder" + source.ID) as List<FeedItem>;
                feeditem_list.ItemsSource = feedItems;
            }
        }

        #endregion

        #region Bing

        private void Query_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                SearchBing(null, null);
        }

        private void SearchBing(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            int count = Convert.ToInt16(queryCount.Text);

            BingQuery.QueryAsync(query.Text, count,
                async (items) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        bingResult.ItemsSource = items;
                    });
                },
                async (exception) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        InAppNotification.Show(exception);
                    });
                }, 
                (BingCountry)(country.SelectedItem), (BingLanguage)(language.SelectedItem), (BingQueryType)(queryType.SelectedItem));

        }

        private void BingResult_ItemClick(object sender, ItemClickEventArgs e)
        {
            BingResult result = e.ClickedItem as BingResult;
            this.Frame.Navigate(typeof(WebPage), result.Link);
        }

        #endregion

    }
}
