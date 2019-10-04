﻿using LogicService.Application;
using LogicService.Objects;
using LogicService.Services;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
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
    public sealed partial class Search : Page
    {
        public Search()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            InitializeSearch();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null) // to solve the BUG: always show old site when NavigatedTo
            {
                string link = e.Parameter as string;
                if (!string.IsNullOrEmpty(link))
                {
                    webView.Source = new Uri(link);
                }
            }
        }

        private async void InitializeSearch()
        {
            try
            {
                SearchSources = await LocalStorage.ReadJsonAsync<Dictionary<string, string>>(
                    await LocalStorage.GetDataAsync(), "searchlist");
            }
            catch
            {
                // for new user, remember to load default feed from file, not the follows
                SearchSources = new Dictionary<string, string>()
                {
                    { "ACS", "https://pubs.acs.org/action/doSearch?AllField=QUERY"},
                    { "arXiv All", "https://arxiv.org/search/?query=QUERY&searchtype=all"},
                };
                LocalStorage.WriteJson(await LocalStorage.GetDataAsync(), "searchlist", SearchSources);
            }
            finally
            {
                searchlist.ItemsSource = SearchSources.Keys;
                searchlist.SelectedIndex = 0;
                source_list.ItemsSource = SearchSources;
                linkFilter.ItemsSource = CrawlerService.LinkFilter.Keys;
                linkFilter.Text = "Text: NotEmpty";
            }
        }

        private void Flyout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        #region Search Engine

        public Dictionary<string, string> SearchSources { get; set; }

        private void Search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string link = SearchSources.GetValueOrDefault(searchlist.SelectedItem as string).Replace("QUEST", queryQuest.Text);
            webView.Source = new Uri(link);
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
            LocalStorage.WriteJson(await LocalStorage.GetDataAsync(), "searchlist", SearchSources);

            ClearSettings();
        }

        private async void Delete_SearchSetting(object sender, RoutedEventArgs e)
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
            SearchSources.Remove(searchName.Text);

            searchlist.ItemsSource = null;
            searchlist.ItemsSource = SearchSources.Keys;
            searchlist.SelectedIndex = 0;
            source_list.ItemsSource = null;
            source_list.ItemsSource = SearchSources;
            LocalStorage.WriteJson(await LocalStorage.GetDataAsync(), "searchlist", SearchSources);

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

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
            => webWaiting.IsActive = true;

        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            webWaiting.IsActive = false;
            siteUrl.Text = webView.Source.AbsoluteUri;
        }

        private void WebView_FrameNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            webWaiting.IsActive = false;
            siteUrl.Text = webView.Source.AbsoluteUri;
        }

        private void WebView_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            webWaiting.IsActive = false;
            siteUrl.Text = webView.Source.AbsoluteUri;
        }

        private void Browse_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            try
            {
                string uri = siteUrl.Text;
                uri = uri.StartsWith("http") ? uri : "http://" + uri;
                webView.Source = new Uri(uri);
            }
            catch { }
        }

        private void PageBack(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoBack) webView.GoBack();
        }

        private void PageForward(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoForward) webView.GoForward();
        }

        private void PageRefresh(object sender, RoutedEventArgs e)
            => webView.Refresh();

        private async void OpenBrowser(object sender, RoutedEventArgs e)
            => await Launcher.LaunchUriAsync(webView.Source);

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

        #endregion

        #region Fisrt Crawl

        private CrawlerService currentCrawled = null;

        private void Open_CrawlerPanel(object sender, RoutedEventArgs e) => crawlPane.IsPaneOpen = !crawlPane.IsPaneOpen;

        private void FirstCrawl_Click(object sender, RoutedEventArgs e)
        {
            if (webView.Source != null)
                FirstCrawl(webView.Source.ToString());
        }

        private void FirstCrawl(string urlstring)
        {
            crawlPane.IsPaneOpen = true;
            craWaiting.IsActive = true;

            currentCrawled = new CrawlerService(urlstring);
            currentCrawled.BeginGetResponse(
                async (result) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        LinkFilter_QuerySubmitted(null, null);
                        craWaiting.IsActive = false;
                    });
                },
                async (exception) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ApplicationMessage.SendMessage("SearchException: " + exception, ApplicationMessage.MessageType.InAppNotification);
                        craWaiting.IsActive = false;
                    });
                });
        }

        private void LinkFilter_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => LinkFilter_QuerySubmitted(null, null);

        private void LinkFilter_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (currentCrawled == null)
                return;

            Regex regex = new Regex(@"(?<header>^(Text|Url):\s(\w+$|\w+=))(?<param>\w*$)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Match match = regex.Match(linkFilter.Text);
            if (match.Success && currentCrawled != null)
            {
                string header = match.Groups["header"].Value;
                string param = match.Groups["param"].Value;
                if (header.StartsWith("Text"))
                {
                    if (header.Equals("Url: Insite"))
                        link_list.ItemsSource = currentCrawled.InsiteLinks;
                    else
                        link_list.ItemsSource = currentCrawled.GetSpecialLinksByText(CrawlerService.LinkFilter.GetValueOrDefault(header) + param);
                }
                if (header.StartsWith("Url"))
                {
                    link_list.ItemsSource = currentCrawled.GetSpecialLinksByUrl(CrawlerService.LinkFilter.GetValueOrDefault(header) + param);
                }
            }
            else if (string.IsNullOrEmpty(linkFilter.Text))
                link_list.ItemsSource = currentCrawled.Links;
        }

        private void Link_list_ItemClick(object sender, ItemClickEventArgs e)
        {
            string link = (e.ClickedItem as Crawlable).Url;
            // if not a downloadable link
            webView.Source = new Uri(link);
        }

        private void SubmitToCrawler(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Crawler), currentCrawled);
        }

        #endregion

    }
}
