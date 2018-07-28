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
                    new FeedSource{ID="https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DHydrogen%252BBond%2526target%253Ddefault%2526targetTab%253Dstd".GetHashCode().ToString(),
                        Name ="Hydrogen Bond in ACS",Uri="https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DHydrogen%252BBond%2526target%253Ddefault%2526targetTab%253Dstd",MaxCount=50,Star=5,IsJournal=true},
                    new FeedSource{ID="http://feeds.aps.org/rss/recent/prl.xml".GetHashCode().ToString(),
                        Name ="Physical Review Letters",Uri="http://feeds.aps.org/rss/recent/prl.xml",MaxCount=50,Star=5,IsJournal=true},
                    new FeedSource{ID="http://www.sciencenet.cn/xml/paper.aspx?di=7".GetHashCode().ToString(),
                        Name ="科学网-数理科学",Uri="http://www.sciencenet.cn/xml/paper.aspx?di=7",MaxCount=50,Star=5,IsJournal=false}
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

        private FeedSource currentFeed = null;

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
                FeedSource newFeed = new FeedSource
                {
                    ID = feedUrl.Text.GetHashCode().ToString(),
                    Name = feedName.Text,
                    Uri = feedUrl.Text,
                    MaxCount = feedCount.Value,
                    Star = feedStar.Value,
                    IsJournal = (bool)(isJournal.IsChecked)
                };
                if (currentFeed != null)
                {
                    FeedSources[FeedSources.IndexOf(currentFeed)] = newFeed;
                }
                else
                {
                    FeedSources.Add(newFeed);
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
            LocalStorage.DeleteFile(await LocalStorage.GetFeedsAsync(), currentFeed.ID);
            ClearSettings();
        }

        private void CancelInvokedHandler(IUICommand command)
            => ClearSettings();

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
            => this.Frame.Navigate(typeof(WebPage), (e.ClickedItem as FeedItem));

        private async void SearchRss(FeedSource source)
        {
            try
            {
                List<FeedItem> feedItems = await LocalStorage.ReadObjectAsync<List<FeedItem>>(
                    await LocalStorage.GetFeedsAsync(), source.ID, "blamder" + source.ID) as List<FeedItem>;
                feeditem_list.ItemsSource = feedItems;
            }
            catch
            {
                waiting_feed.IsActive = true;
                RssService.GetRssItems(
                    source.Uri,
                    async (items) =>
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            feeditem_list.ItemsSource = items;
                            waiting_feed.IsActive = false;
                        });
                        await LocalStorage.WriteObjectAsync(await LocalStorage.GetFeedsAsync(), source.ID, items, "blamder" + source.ID);
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
