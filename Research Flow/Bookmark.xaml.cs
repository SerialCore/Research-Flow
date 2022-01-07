using LogicService.Application;
using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Bookmark : Page
    {
        public Bookmark()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UpdatePaper();
        }

        private void UpdatePaper()
        {
            bookmarks = Feed.DBSelectBookmark();
            bookmarklist.ItemsSource = bookmarks;
        }

        #region Feed Operation

        private List<Feed> bookmarks = new List<Feed>();

        private Feed selectedFeed;

        private void BookmarkList_ItemClick(object sender, ItemClickEventArgs e)
        {
            selectedFeed = e.ClickedItem as Feed;

            feedTitle.Text = selectedFeed.Title;
            feedAuthor.Text = selectedFeed.Authors;
            feedPublished.Text = selectedFeed.Published;
            articleID.Text = selectedFeed.ArticleID;
            feedLink.Text = selectedFeed.Link;
            feedSummary.Text = selectedFeed.Summary;
        }

        private void FeedLink_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (selectedFeed != null)
                this.Frame.Navigate(typeof(SearchEngine), selectedFeed.Link);
        }

        private async void OpenPdf_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFeed == null)
                return;

            StorageFile pdf = null;
            string[] articleid = selectedFeed.ArticleID.Split('/');
            string filename = articleid[articleid.Length - 1] + ".pdf";
            try
            {
                pdf = await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(filename);
            }
            catch { }

            if (pdf != null)
            {
                await Launcher.LaunchFileAsync(pdf);
            }
            else
            {
                downloadpanel.IsOpen = true;
                downloadstate.IsIndeterminate = true;
                downloadstate.ShowPaused = false;
                downloadlist.ItemsSource = null;
                CrawlerService crawler = new CrawlerService(selectedFeed.Link);
                crawler.BeginGetResponse(
                    async (items) =>
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            downloadstate.IsIndeterminate = false;
                            downloadlist.ItemsSource = crawler.GetSpecialLinksByUrl(@"(.pdf\z)|(/pdf/)");
                        });
                    },
                    async (exception) =>
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            ApplicationMessage.SendMessage(new MessageEventArgs { Title = "CrawlerException", Content = exception, Type = MessageType.InApp, Time = DateTimeOffset.Now });
                            downloadstate.IsIndeterminate = false;
                        });
                    });
            }
        }

        private async void DownloadList_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                await (await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(selectedFeed.ID + ".pdf")).DeleteAsync();
            }
            catch { }

            string url = (e.ClickedItem as Crawlable).Url;
            string[] articleid = selectedFeed.ArticleID.Split('/');
            string filename = articleid[articleid.Length - 1] + ".pdf";
            WebClientService webClient = new WebClientService();
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            webClient.DownloadFile(url, (await LocalStorage.GetPaperFolderAsync()).Path, filename, async () =>
                {
                    try
                    {
                        StorageFile pdf = await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(filename);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            await Launcher.LaunchFileAsync(pdf);
                        });
                    }
                    catch { }
                    webClient.DownloadProgressChanged -= WebClient_DownloadProgressChanged;
                });
        }

        private async void WebClient_DownloadProgressChanged(object sender, DownloadEventArgs e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                downloadstate.Value = (e.BytesReceived / e.TotalBytes) * 100;
            });
        }

        private async void DeleteFeed_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Delete application data?";
            dialog.PrimaryButtonText = "Yeah";
            dialog.CloseButtonText = "Forget it";
            dialog.DefaultButton = ContentDialogButton.Primary;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Feed.DBDeleteBookmarkByID(selectedFeed.ID);
                UpdatePaper();

                try
                {
                    await (await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(selectedFeed.ID + ".pdf")).DeleteAsync();
                }
                catch { }
            }
        }

        #endregion

    }
}
