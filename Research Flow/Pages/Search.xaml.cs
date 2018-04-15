using System;
using System.Collections.Generic;
using LogicService.Encapsulates;
using LogicService.Services;
using Microsoft.Toolkit.Services.Bing;
using Research_Flow.Pages.SubPages;
using Windows.System;
using Windows.UI.Core;
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
            feedsource_list.ItemsSource = new List<FeedSource>()
            {
                new FeedSource{SourceName="ACS",SourceUri="https://pubs.acs.org/action/showFeed?ui=0&mi=4ta59b4&type=search&feed=rss&query=%2526AllField%253Dhydrogen%252Bbond%2526publication%253D40025988%2526sortBy%253DEarliest%2526target%253Ddefault%2526targetTab%253Dstd"},
                new FeedSource{SourceName="科学网",SourceUri="http://www.sciencenet.cn/xml/paper.aspx?di=7"}
            };

            // Bing configure
            country.ItemsSource = Enum.GetValues(typeof(BingCountry));
            language.ItemsSource = Enum.GetValues(typeof(BingLanguage));
            queryType.ItemsSource = Enum.GetValues(typeof(BingQueryType));
            country.SelectedItem = BingCountry.None;
            language.SelectedItem = BingLanguage.None;
            queryType.SelectedItem = BingQueryType.Search;
        }

        #region RSS

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

        private void RSS_SourceClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FeedSource;
            SearchRss(item.SourceUri);
        }

        private void RSS_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FeedItem;
            this.Frame.Navigate(typeof(WebPage), new string[] { item.Title, item.Link });
        }

        #endregion

        #region Bing

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
            this.Frame.Navigate(typeof(WebPage), new string[] { result.Title, result.Link });
        }

        #endregion

    }
}
