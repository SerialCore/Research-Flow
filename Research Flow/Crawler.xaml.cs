using LogicService.Application;
using LogicService.Data;
using LogicService.Security;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;
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
    public sealed partial class Crawler : Page
    {
        public Crawler()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            InitializeCrawl();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                if (e.Parameter.GetType().Equals(typeof(string)))
                {
                    string search = e.Parameter as string;
                    crawlsearch.Text = search;
                    CrawlSearch_QuerySubmitted(null, null);
                }
                else if (e.Parameter.GetType().Equals(typeof(CrawlerService)))
                {
                    CleanCrawlPanel();

                    CrawlerService service = e.Parameter as CrawlerService;
                    crawltext.Text = service.Title;
                    crawlurl.Content = service.Url;
                    crawlurl.NavigateUri = new Uri(service.Url);
                    crawlcontent.Text = service.Content;
                    crawltags.Text = "";
                    crawlfilters.Text = service.LinkFilters;

                    // filter this
                    fromservice = service.Links;
                    servicelist.ItemsSource = fromservice;
                }
            }

            InitializeFavorite();
        }

        private async void InitializeFavorite()
        {
            try
            {
                favorites = await LocalStorage.ReadJsonAsync<List<Crawlable>>(
                    await LocalStorage.GetDataFolderAsync(), "favorite.list");
            }
            catch
            {
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "favorite.list", favorites);
            }
            finally
            {
                favoritelist.ItemsSource = favorites;
                linkFilter.ItemsSource = Crawlable.LinkType.Keys;
                linkFilter.Text = "Text: NotEmpty";
            }
        }

        private void InitializeCrawl()
        {
            fromdatabase = Crawlable.DBSelectByLimit(100);
            databaselist.ItemsSource = fromdatabase;
            searchtype.SelectedIndex = 0;
        }

        #region Favorite

        private List<Crawlable> favorites = new List<Crawlable>();

        private void OpenFavorites_Click(object sender, RoutedEventArgs e) => favoritepanel.IsPaneOpen = true;

        private void FavoriteList_ItemClick(object sender, ItemClickEventArgs e)
        {
            CleanCrawlPanel();

            Crawlable crawlable = e.ClickedItem as Crawlable;
            currentCrawlable = crawlable;
            crawltext.Text = crawlable.Text;
            crawlurl.Content = crawlable.Url;
            crawlurl.NavigateUri = new Uri(crawlable.Url);
            crawlcontent.Text = crawlable.Content;
            crawltags.Text = crawlable.Tags;
            crawlfilters.Text = crawlable.Filters;
        }

        #endregion

        #region Crawler

        private Crawlable currentCrawlable = null; // selected from database or favorites

        private CrawlerService currentService = null; // real-time crawler service

        private List<Crawlable> fromdatabase = new List<Crawlable>();
        private List<Crawlable> fromservice = new List<Crawlable>();

        private void CleanCrawlPanel()
        {
            crawltext.Text = "";
            crawlurl.Content = "Link";
            crawlurl.NavigateUri = null;
            crawlcontent.Text = "";
            crawltags.Text = "";
            crawlfilters.Text = "";
        }

        private void CrawlSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (searchtype.SelectedIndex == 0) // search by text | Content
            {
                fromdatabase = Crawlable.DBSelectByTextContent(crawlsearch.Text);
                databaselist.ItemsSource = fromdatabase;
            }
        }

        private void CrawlCrawlable(object sender, RoutedEventArgs e)
        {
            if (crawlurl.NavigateUri == null)
                    return;
            craWaiting.IsActive = true;

            currentService = new CrawlerService(crawlurl.Content as string);
            currentService.BeginGetResponse(
                async (result) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        // filter this
                        fromservice = result.Links;
                        servicelist.ItemsSource = fromservice;
                        crawlcontent.Text = result.Content;
                        craWaiting.IsActive = false;
                    });
                },
                async (exception) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        currentService = null;
                        craWaiting.IsActive = false;
                        ApplicationMessage.SendMessage("CrawlerException: " + exception, ApplicationMessage.MessageType.InApp);
                    });
                });
        }

        private void SaveCrawlable(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(crawltext.Text) || crawlurl.NavigateUri == null)
            {
                ApplicationMessage.SendMessage("CrawlerWarning: There must be Text and Url", ApplicationMessage.MessageType.InApp);
                return;
            }

            string id = HashEncode.MakeMD5(crawlurl.Content as string);
            if (Crawlable.DBSelectByID(id).Count != 0)
                Crawlable.DBDeleteByID(id); // modify or delete?
            Crawlable.DBInsert(new List<Crawlable>()
                {
                    new Crawlable
                    {
                        ID = id,
                        ParentID = "Null",
                        Text = crawltext.Text,
                        Url = crawlurl.Content as string,
                        Content = string.IsNullOrEmpty(crawlcontent.Text)? "Null" : crawlcontent.Text,
                        Tags = string.IsNullOrEmpty(crawltags.Text)? "Null" : crawltags.Text,
                        Filters = string.IsNullOrEmpty(crawlfilters.Text)? "Null" : crawlfilters.Text,
                    }
                });
        }

        private async void DeleteCrawlable(object sender, RoutedEventArgs e)
        {
            if (currentCrawlable == null) // must exist before deleted
                return;

            var messageDialog = new MessageDialog("You are about to delete application data, please tell me that is not true.", "Operation confirming");
            messageDialog.Commands.Add(new UICommand("True", new UICommandInvokedHandler(this.DeleteInvokedHandler)));
            messageDialog.Commands.Add(new UICommand("Joke", new UICommandInvokedHandler(this.CancelInvokedHandler)));

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync();
        }

        private async void DeleteInvokedHandler(IUICommand command)
        {
            Crawlable.DBDeleteByID(currentCrawlable.ID);
            favorites.Remove(currentCrawlable);
            currentCrawlable = null;
            LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "favorite.list", favorites);

            CleanCrawlPanel();
        }

        private void CancelInvokedHandler(IUICommand command) { }

        private void DatabaseList_ItemClick(object sender, ItemClickEventArgs e)
        {
            CleanCrawlPanel();

            currentCrawlable = e.ClickedItem as Crawlable;
            crawltext.Text = currentCrawlable.Text;
            crawlurl.Content = currentCrawlable.Url;
            crawlurl.NavigateUri = new Uri(currentCrawlable.Url);
            crawlcontent.Text = currentCrawlable.Content;
            crawltags.Text = currentCrawlable.Tags;
            crawlfilters.Text = currentCrawlable.Filters;
        }

        private void ServiceList_ItemClick(object sender, ItemClickEventArgs e)
        {
            CleanCrawlPanel();

            var item = e.ClickedItem as Crawlable;
            crawltext.Text = item.Text;
            crawlurl.Content = item.Url;
            crawlurl.NavigateUri = new Uri(item.Url);
            crawlcontent.Text = item.Content;
            crawltags.Text = item.Tags;
            crawlfilters.Text = item.Filters;
        }

        private void ShowDatabase_Click(object sender, RoutedEventArgs e)
        {
            fromdatabase = Crawlable.DBSelectByLimit(100);
            databaselist.ItemsSource = fromdatabase;
        }

        private void SaveLinks_Click(object sender, RoutedEventArgs e)
        {
            Crawlable.DBInsert(fromservice);
        }

        #endregion

    }
}
