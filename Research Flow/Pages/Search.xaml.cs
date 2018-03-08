using System;
using System.Collections.Generic;
using LogicService.Encapsulates;
using LogicService.Services;
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

            feedsource_list.ItemsSource = new List<FeedSource>()
            {
                new FeedSource{SourceName="ACS",SourceUri="https://pubs.acs.org/action/showFeed?ui=0&mi=4ta59b4&type=search&feed=rss&query=%2526AllField%253Dhydrogen%252Bbond%2526publication%253D40025988%2526sortBy%253DEarliest%2526target%253Ddefault%2526targetTab%253Dstd"},
                new FeedSource{SourceName="科学网",SourceUri="http://www.sciencenet.cn/xml/paper.aspx?di=7"}
            };
            this.DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

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

        private void RSS_SourceClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FeedSource;
            SearchRss(item.SourceUri);
        }

        private async void RSS_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FeedItem;
            await Launcher.LaunchUriAsync(new Uri(item.AbsoluteUri));
        }

    }
}
