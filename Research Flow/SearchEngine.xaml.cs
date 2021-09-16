using LogicService.Application;
using LogicService.Data;
using LogicService.Security;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchEngine : Page
    {
        public SearchEngine()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            InitializeSearch();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                if (e.Parameter.GetType().Equals(typeof(string)))
                {
                    string link = e.Parameter as string;
                    if (webView.Source == null)
                    {
                        webView.Source = new Uri(link);
                        FirstCrawl(link);
                    }
                    else
                    {
                        if (!webView.Source.ToString().Equals(link)) // to solve the BUG: always re-pass parameter when GoBack
                        {
                            webView.Source = new Uri(link);
                            FirstCrawl(link);
                        }
                    }
                }
            }

            InitializeFavorite();
        }

        private async void InitializeSearch()
        {
            try
            {
                SearchSources = await LocalStorage.ReadJsonAsync<Dictionary<string, string>>(
                    await LocalStorage.GetDataFolderAsync(), "search.list");
            }
            catch
            {
                // for new user, remember to load default feed from file, not the follows
                SearchSources = new Dictionary<string, string>()
                {
                    { "ACS", "https://pubs.acs.org/action/doSearch?AllField=QUERY" },
                    { "arXiv All", "https://arxiv.org/search/?query=QUERY&searchtype=all" },
                    { "Bing Academic", "https://cn.bing.com/academic/search?q=QUEST&FORM=HDRSC4" },
                };
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "search.list", SearchSources);
            }
            finally
            {
                searchlist.ItemsSource = SearchSources.Keys;
                searchlist.SelectedIndex = 0;
                source_list.ItemsSource = SearchSources;
                linkFilter1.ItemsSource = Crawlable.LinkType.Keys;
                linkFilter1.Text = "Text: NotEmpty";
            }
        }

        private async void InitializeFavorite()
        {
            try
            {
                favorites = await LocalStorage.ReadJsonAsync<ObservableCollection<Crawlable>>(
                    await LocalStorage.GetDataFolderAsync(), "favorite.list");
            }
            catch
            {
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "favorite.list", favorites);
            }
            finally
            {
                favoritelist.ItemsSource = favorites;
            }
        }

        #region Search Engine

        private Dictionary<string, string> SearchSources;

        private void Search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string link = SearchSources.GetValueOrDefault(searchlist.SelectedItem as string).Replace("QUEST", queryQuest.Text);
            webView.Source = new Uri(link);
            FirstCrawl(link);
        }

        private void Open_SearchList(object sender, RoutedEventArgs e)
            => searchPane.IsPaneOpen = !searchPane.IsPaneOpen;

        private void Add_Source(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => source_panel.Visibility = Visibility.Visible;

        private void Source_list_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (KeyValuePair<string, string>)e.ClickedItem;
            source_panel.Visibility = Visibility.Visible;

            searchName.Text = item.Key;
            searchUrl.Text = item.Value;

            searchUrl.IsReadOnly = true;
            searchDelete.Visibility = Visibility;
            source_panel.Visibility = Visibility.Visible;
        }

        private async void Confirm_SearchSetting(object sender, RoutedEventArgs e)
        {
            // add: change name and url
            // modify: just change name
            string exist = null;
            foreach (string temp in SearchSources.Keys)
            {
                if (SearchSources[temp].Equals(searchUrl.Text))
                    exist = temp;
            }
            if (SearchSources.ContainsValue(searchUrl.Text)) // modify name
            {
                // check name
                if (!SearchSources.ContainsKey(searchName.Text))
                {
                    SearchSources.Remove(exist);
                    SearchSources.Add(searchName.Text, searchUrl.Text);
                }
            }
            else if (!SearchSources.ContainsKey(searchName.Text)) // add new
            { 
                // url will be checked
                SearchSources.Add(searchName.Text, searchUrl.Text);
            }

            searchlist.ItemsSource = null;
            searchlist.ItemsSource = SearchSources.Keys;
            searchlist.SelectedIndex = 0;
            source_list.ItemsSource = null;
            source_list.ItemsSource = SearchSources;
            LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "search.list", SearchSources);

            ClearSettings();
        }

        private async void Delete_SearchSetting(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("You are about to delete application data, please tell me that is not true.");
            messageDialog.Commands.Add(new UICommand("True", new UICommandInvokedHandler(this.DeleteInvokedHandler)));
            messageDialog.Commands.Add(new UICommand("Joke", new UICommandInvokedHandler(this.CancelInvokedHandler)));

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync();
        }

        private async void DeleteInvokedHandler(IUICommand command)
        {
            SearchSources.Remove(searchName.Text);

            searchlist.ItemsSource = null;
            searchlist.ItemsSource = SearchSources.Keys;
            searchlist.SelectedIndex = 0;
            source_list.ItemsSource = null;
            source_list.ItemsSource = SearchSources;
            LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "search.list", SearchSources);

            ClearSettings();
        }

        private void CancelInvokedHandler(IUICommand command) => ClearSettings();

        private void Leave_SearchSetting(object sender, RoutedEventArgs e) => ClearSettings();

        private void ClearSettings()
        {
            searchName.Text = "";
            searchUrl.Text = "";

            searchUrl.IsReadOnly = false;
            searchDelete.Visibility = Visibility.Collapsed;
            source_panel.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Browser

        private ObservableCollection<Crawlable> favorites = new ObservableCollection<Crawlable>();

        private void Flyout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            webWaiting.Visibility = Visibility.Visible;
            webWaiting.ShowPaused = false;
        }

        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            webWaiting.ShowPaused = true;
            webWaiting.Visibility = Visibility.Collapsed;
            siteUrl.Text = webView.Source.AbsoluteUri;
        }

        private void WebView_FrameNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            webWaiting.ShowPaused = true;
            webWaiting.Visibility = Visibility.Collapsed;
            siteUrl.Text = webView.Source.AbsoluteUri;
        }

        private void WebView_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            webWaiting.ShowPaused = true;
            webWaiting.Visibility = Visibility.Collapsed;
            siteUrl.Text = webView.Source.AbsoluteUri;
        }

        private void Browse_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            try
            {
                string url = siteUrl.Text;
                url = url.StartsWith("http") ? url : "http://" + url;
                webView.Source = new Uri(url);
                FirstCrawl(url);
            }
            catch { }
        }

        private void FavoriteList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Crawlable fav = e.ClickedItem as Crawlable;
            webView.Source = new Uri(fav.Url);
            FirstCrawl(fav.Url);
        }

        private void PageBack(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoBack) webView.GoBack();
        }

        private void PageForward(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoForward) webView.GoForward();
        }

        private void PageStop(object sender, RoutedEventArgs e)
        {
            webView.Stop();
            webWaiting.ShowPaused = true;
            webWaiting.Visibility = Visibility.Collapsed;
        }

        private void PageRefresh(object sender, RoutedEventArgs e) => webView.Refresh();

        private void Favorite_Click(object sender, RoutedEventArgs e)
        {
            if (webView.Source != null)
            {
                Crawlable newcrawl = new Crawlable()
                {
                    ID = HashEncode.MakeMD5(webView.Source.ToString()),
                    ParentID = "",
                    Text = webView.DocumentTitle,
                    Url = webView.Source.ToString(),
                    Content = "",
                    Tags = "",
                    Filters = ""
                };
                Crawlable.AddtoFavorite(newcrawl);
                favorites.Add(newcrawl);
            }
        }

        private async void AddToTagTopic(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(queryQuest.Text))
            {
                var tags = await LocalStorage.ReadJsonAsync<HashSet<string>>(
                    await LocalStorage.GetDataFolderAsync(), "tag.list");
                tags.UnionWith(new List<string> { queryQuest.Text , "@Search"});
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "tag.list", tags);
                var topics = await LocalStorage.ReadJsonAsync<List<Topic>>(
                    await LocalStorage.GetDataFolderAsync(), "topic.list");
                topics.Add(new Topic { ID = HashEncode.MakeMD5(DateTimeOffset.Now.ToString()), 
                    Title = "#QSearch##" + queryQuest.Text + '#' + SearchSources.GetValueOrDefault(searchlist.SelectedItem as string),
                    Color = "#FFFFF7D1"});
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "topic.list", topics);
            }
        }

        private void ShareLink(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if (webView.Source != null)
            {
                // deferral code for catching data
                DataRequestDeferral deferral = args.Request.GetDeferral();

                // info of share request
                DataRequest request = args.Request;
                request.Data.Properties.Title = "WebSite";
                request.Data.Properties.Description = "Share the website you found";

                request.Data.SetWebLink(webView.Source);

                // end if deferral
                deferral.Complete();
            }
        }

        private async void Link_list_ItemClick(object sender, ItemClickEventArgs e)
        {
            string link = (e.ClickedItem as Crawlable).Url;

            // check if a downloadable link
            if (Regex.IsMatch(link, Crawlable.LinkType["Url: HasPDF"]))
            {
                string name = "";
                // choose url name or page title to be file name, by user setting
                string[] split = link.Split('/');
                // or use regex ".*/(.*?)$"
                if (string.IsNullOrEmpty(split[split.Length - 1]))
                    name = DateTime.Now.ToString("yyyyMMddhhmm") + ".pdf";
                else
                    name = split[split.Length - 1].Replace(".pdf", "") + ".pdf"; // in case of *.pdf.pdf

                downloadClose.IsEnabled = false;
                downloadStatus.Text = "Processing";
                downloadPanel.Visibility = Visibility.Visible;
                WebClientService webClient = new WebClientService();
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                webClient.DownloadFile(link, (await LocalStorage.GetPaperFolderAsync()).Path, name,
                    async () =>
                    {
                        webClient.DownloadProgressChanged -= WebClient_DownloadProgressChanged;
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            downloadClose.IsEnabled = true;
                            // TODO: auto open
                        });
                    },
                    async (exception) =>
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            downloadClose.IsEnabled = true;
                            ApplicationMessage.SendMessage(new ShortMessage { Title = "DownloadException", Content = exception, Time = DateTimeOffset.Now }, 
                                ApplicationMessage.MessageType.InApp);
                        });
                    });
            }
            else
                try
                {
                    webView.Source = new Uri(link);
                }
                catch { }
        }

        private async void WebClient_DownloadProgressChanged(object sender, DownloadEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                int received = e.BytesReceived / 1024;
                int total = e.TotalBytes / 1024;
                downloadBar.Maximum = total;
                downloadBar.Value = received;
                downloadStatus.Text = received + " / " + total + " KB";
            });
        }

        private void CloseDownloadPanel(object sender, RoutedEventArgs e)
        {
            downloadBar.Value = 0;
            downloadPanel.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Fisrt Crawl

        private CrawlerService currentCrawled = null;

        private void FirstCrawl_Click(object sender, RoutedEventArgs e)
        {
            if (crawlPane.IsPaneOpen)
            {
                crawlPane.IsPaneOpen = false;
                return;
            }
            if (webView.Source != null)
                FirstCrawl(webView.Source.ToString());
        }

        private void FirstCrawl(string urlstring)
        {
            crawlPane.IsPaneOpen = true;
            if (currentCrawled != null)
                if (currentCrawled.Url.Equals(urlstring)) // in case of gb2312
                    return;
            craWaiting.IsActive = true;

            currentCrawled = new CrawlerService(urlstring);
            currentCrawled.BeginGetResponse(
                async (result) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        LinkFilter_QuerySubmitted(null, null);
                        craWaiting.IsActive = false;

                        try
                        {
                            pagetitle.Text = result.Title;
                            pagehost.Text = result.Host;
                            pagesize.Text = result.PageSize.ToString();
                            pagecontent.Text = result.Content;
                            string htmlname = HashEncode.MakeMD5(result.Url) + ".txt";
                            LocalStorage.GeneralWriteAsync(await LocalStorage.GetWebTempAsync(), htmlname, result.Html, false);
                        }
                        catch { }
                    });
                },
                async (exception) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        currentCrawled = null;
                        craWaiting.IsActive = false;
                        ApplicationMessage.SendMessage(new ShortMessage { Title = "SearchException", Content = exception , Time = DateTimeOffset.Now }, 
                            ApplicationMessage.MessageType.InApp);
                    });
                });
        }

        private async void HtmlFile_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            StorageFile file = await (await LocalStorage.GetWebTempAsync()).GetFileAsync(HashEncode.MakeMD5(currentCrawled.Url) + ".txt");
            await Launcher.LaunchFileAsync(file);
        }

        private void LinkFilter_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
            => link_list.ItemsSource = Crawlable.LinkFilter(currentCrawled, linkFilter1.Text);

        #endregion

    }
}
