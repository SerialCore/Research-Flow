using LogicService.Encapsulates;
using LogicService.Services;
using LogicService.Storage;
using Microsoft.Toolkit.Services.Bing;
using Research_Flow.Pages.SubPages;
using System;
using System.Collections.Generic;
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

        private async void InitializeData()
        {
            // feed source
            FeedSources = await LocalStorage.ReadObjectAsync<ObservableCollection<FeedSource>>(
                await LocalStorage.GetFeedsAsync(), "RSS") as ObservableCollection<FeedSource>;
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
                if (item.ID == (int)(button.Tag))
                {
                    source_panel.Visibility = Visibility.Visible;
                    currentFeed = item;
                    feedName.Text = item.Name;
                    feedUrl.Text = item.Uri;
                    feedStar.Value = item.Star;
                    isJournal.IsChecked = item.IsJournal;

                    feedUrl.IsReadOnly = true;
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
                    ID = feedUrl.Text.GetHashCode(),
                    Name = feedName.Text,
                    Uri = feedUrl.Text,
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

        private void Delete_FeedSetting(object sender, RoutedEventArgs e)
        {
            FeedSources.Remove(currentFeed);
            ClearSettings();
        }

        private void Leave_FeedSetting(object sender, RoutedEventArgs e) => ClearSettings();

        private void ClearSettings()
        {
            currentFeed = null;
            feedName.Text = "";
            feedUrl.Text = "";
            feedStar.Value = -1;
            isJournal.IsChecked = false;

            feedUrl.IsReadOnly = false;
            source_panel.Visibility = Visibility.Collapsed;
        }

        private void RSS_SourceClick(object sender, ItemClickEventArgs e)
            => SearchRss((e.ClickedItem as FeedSource).Uri);

        private void RSS_ItemClick(object sender, ItemClickEventArgs e)
            => this.Frame.Navigate(typeof(WebPage), (e.ClickedItem as FeedItem).Link);

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
