using LogicService.Objects;
using LogicService.Security;
using LogicService.Services;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

            InitializeEngine();
        }

        private async void InitializeEngine()
        {
            try
            {
                SearchSources = await LocalStorage.ReadJsonAsync<Dictionary<string, string>>(
                    await LocalStorage.GetLinkAsync(), "searchlist");
            }
            catch
            {
                // for new user, remember to load default feed from file, not the follows
                SearchSources = new Dictionary<string, string>()
                {
                    { "Bing", "https://www.bing.com/search?q=QUEST"},
                    { "ACS", "https://pubs.acs.org/action/doSearch?AllField=QUEST"},
                };
                LocalStorage.WriteJson(await LocalStorage.GetLinkAsync(), "searchlist", SearchSources);
            }
            finally
            {
                searchlist.ItemsSource = SearchSources.Keys;
                searchlist.SelectedIndex = 0;
                source_list.ItemsSource = SearchSources;
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string urlstring = SearchSources.GetValueOrDefault(searchlist.SelectedItem as string).Replace("QUEST", queryQuest.Text);
            if (viewMode.IsOn) // App View
            {
                craWaiting.IsActive = true;
                var crawled = new CrawlerService(urlstring);
                Task.Run(() =>
                {
                    crawled.BeginGetResponse(
                        async (result) =>
                        {
                            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                link_list.ItemsSource = result.Links;
                                craWaiting.IsActive = false;
                            });
                        },
                        async (exception) =>
                        {
                            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                InAppNotification.Show(exception);
                                craWaiting.IsActive = false;
                            });
                        });
                });

                webview.Navigate(new Uri(urlstring));
            }
            else // User View
            {
                webview.Navigate(new Uri(urlstring));
            }
        }

        private void ViewMode_Toggled(object sender, RoutedEventArgs e)
        {
            if (viewMode.IsOn) // App View
            {
                crawlPane.IsPaneOpen = true;
                if (webview.Source != null)
                {
                    craWaiting.IsActive = true;
                    var crawled = new CrawlerService(webview.Source.ToString());
                    Task.Run(()=>
                    {
                        crawled.BeginGetResponse(
                            async (result) =>
                            {
                                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    link_list.ItemsSource = result.Links;
                                    craWaiting.IsActive = false;
                                });
                            },
                            async (exception) =>
                            {
                                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    InAppNotification.Show(exception);
                                    craWaiting.IsActive = false;
                                });
                            });
                    });
                }
            }
            else // User View
            {
                crawlPane.IsPaneOpen = false;
                link_list.ItemsSource = null;
            }
        }

        #region Search Engine

        public Dictionary<string, string> SearchSources { get; set; }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
            => webWaiting.IsActive = true;

        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
            => webWaiting.IsActive = false;

        private void WebView_FrameNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
            => webWaiting.IsActive = false;

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
            LocalStorage.WriteJson(await LocalStorage.GetLinkAsync(), "searchlist", SearchSources);

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
            LocalStorage.WriteJson(await LocalStorage.GetLinkAsync(), "searchlist", SearchSources);

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

    }
}
