using LogicService.Objects;
using LogicService.Security;
using LogicService.Services;
using LogicService.Storage;
using Research_Flow.Pages;
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

namespace Research_Flow
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

        private async void InitializeData()
        {
            // there must be feed source file
            // if someone delete the file
            try
            {
                FeedSources = await LocalStorage.ReadObjectAsync<ObservableCollection<FeedSource>>(
                    await LocalStorage.GetFeedAsync(), "RSS") as ObservableCollection<FeedSource>;
            }
            catch { }
            feedSource_list.ItemsSource = FeedSources;
        }

        #region RSS

        public ObservableCollection<FeedSource> FeedSources { get; set; }

        // Used for item modification, not clicked item.
        private FeedSource currentFeed = null;

        private void Add_Source(object sender, RoutedEventArgs e) => source_panel.Visibility = Visibility.Visible;

        private void Modify_Source(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            foreach (var item in FeedSources)
            {
                if (item.ID.Equals((string)button.Tag))
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
                await LocalStorage.WriteObjectAsync(await LocalStorage.GetFeedAsync(), "RSS", FeedSources);
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
            await LocalStorage.WriteObjectAsync(await LocalStorage.GetFeedAsync(), "RSS", FeedSources);
            try
            {
                LocalStorage.DeleteFile(await LocalStorage.GetFeedAsync(), currentFeed.ID);
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
            => SearchRss(e.ClickedItem as FeedSource);

        private void RSS_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.feedItem_detail.IsPaneOpen = true;
        }

        private async void SearchRss(FeedSource source)
        {
            // now you can modify source while fetching feed
            int selectedFeedIndex = FeedSources.IndexOf(source);
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
                            feedItem_list.ItemsSource = feeds;
                            waiting_feed.IsActive = false;
                        });
                        if (feeds.Count > source.MaxCount)
                            feeds.RemoveRange(source.MaxCount, feeds.Count - source.MaxCount);
                        await LocalStorage.WriteObjectAsync(await LocalStorage.GetFeedAsync(), source.ID, items, "hashashin" + source.ID);
                        FeedSources[selectedFeedIndex].LastUpdateTime = DateTime.Now;
                        await LocalStorage.WriteObjectAsync(await LocalStorage.GetFeedAsync(), "RSS", FeedSources);
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
                // if someone delete the file, then try
                try
                {
                    List<FeedItem> feedItems = await LocalStorage.ReadObjectAsync<List<FeedItem>>(
                        await LocalStorage.GetFeedAsync(), source.ID, "hashashin" + source.ID) as List<FeedItem>;
                    feedItem_list.ItemsSource = feedItems;
                }
                catch { }
            }
        }

        #endregion

    }
}
