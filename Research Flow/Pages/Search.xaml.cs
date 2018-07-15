using LogicService.Encapsulates;
using LogicService.Services;
using Microsoft.Toolkit.Services.Bing;
using Research_Flow.Pages.SubPages;
using System;
using System.Collections.ObjectModel;
using Windows.System;
using Windows.UI.Core;
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

        private void InitializeData()
        {
            // feed source
            FeedSources = new ObservableCollection<FeedSource>()
            {
                new FeedSource{SourceName="ACS",SourceUri="https://pubs.acs.org/action/showFeed?ui=0&mi=4ta59b4&type=search&feed=rss&query=%2526AllField%253Dhydrogen%252Bbond%2526publication%253D40025988%2526sortBy%253DEarliest%2526target%253Ddefault%2526targetTab%253Dstd",Star=5,IsJournal=true},
                new FeedSource{SourceName="科学网",SourceUri="http://www.sciencenet.cn/xml/paper.aspx?di=7",Star=4,IsJournal=false},
                new FeedSource{SourceName="PRA",SourceUri="http://feeds.aps.org/rss/recent/pra.xml",Star=5,IsJournal=true},
                new FeedSource{SourceName="PRB",SourceUri="http://feeds.aps.org/rss/recent/prb.xml",Star=5,IsJournal=true},
                new FeedSource{SourceName="PRC",SourceUri="http://feeds.aps.org/rss/recent/prc.xml",Star=5,IsJournal=true},
                new FeedSource{SourceName="PRD",SourceUri="http://feeds.aps.org/rss/recent/prd.xml",Star=5,IsJournal=true}
            };
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

        private void Open_feedsource(object sender, RoutedEventArgs e)
        {
            feedsource_view.IsPaneOpen = true;
        }

        private void Source_modify(object sender, RoutedEventArgs e)
        {
            source_panel.Visibility = Visibility.Visible;
        }

        private void RSS_SourceClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FeedSource;
            SearchRss(item.SourceUri);
        }

        private void RSS_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FeedItem;
            this.Frame.Navigate(typeof(WebPage), item.Link);
        }

        private void SearchRss(string feedlink)
        {
            RssService.GetRssItems(
                feedlink,
                async (items) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        feeditem_list.ItemsSource = items;
                    });
                },
                async (exception) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        InAppNotification.Show("RssException: " + exception);
                    });
                }, null);
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
