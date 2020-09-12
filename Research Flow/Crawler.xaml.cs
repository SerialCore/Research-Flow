﻿using LogicService.Application;
using LogicService.Data;
using LogicService.Security;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            InitializeCrawler();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                if (e.Parameter.GetType().Equals(typeof(string)))
                {
                    string search = e.Parameter as string;
                    if (!crawlsearch.Text.Equals(search))
                    {
                        crawlsearch.Text = search;
                        CrawlSearch_QuerySubmitted(null, null);
                    }
                }
                else if (e.Parameter.GetType().Equals(typeof(CrawlerService)))
                {
                    CleanCrawlPanel();
                    CrawlerService service = e.Parameter as CrawlerService;
                    if (!crawltext.Text.Equals(service.Title))
                    {
                        crawltext.Text = service.Title;
                        crawlurl.Text = service.Url;
                        crawlcontent.Text = service.Content;
                        crawltags.Text = "";
                        crawlfilters.Text = service.LinkFilters;

                        // filter this
                        fromservice = service.Links;
                        servicelist.ItemsSource = fromservice;
                    }
                }
            }

            InitializeFavorite();
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

        private void InitializeCrawler()
        {
            linkFilter.ItemsSource = Crawlable.LinkType.Keys;
            linkFilter.Text = "Text: NotEmpty";
            searchtype.SelectedIndex = 1;
        }

        #region Favorite

        private Crawlable currentFavor = null;

        private ObservableCollection<Crawlable> favorites = new ObservableCollection<Crawlable>();

        private void OpenFavorites_Click(object sender, RoutedEventArgs e) => favoritepanel.IsPaneOpen = true;

        private void FavoriteList_ItemClick(object sender, ItemClickEventArgs e)
        {
            CleanCrawlPanel();

            Crawlable crawlable = e.ClickedItem as Crawlable;
            currentCrawlable = crawlable;
            crawltext.Text = crawlable.Text;
            crawlurl.Text = crawlable.Url;
            crawlcontent.Text = crawlable.Content;
            crawltags.Text = crawlable.Tags;
            crawlfilters.Text = crawlable.Filters;
            currentFavor = crawlable;
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
            crawlurl.Text = "";
            crawlcontent.Text = "";
            crawltags.Text = "";
            crawlfilters.Text = "";
        }

        private void CrawlUrl_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(crawlurl.Text))
            {
                this.Frame.Navigate(typeof(SearchEngine), crawlurl.Text);
            }
        }

        private void CrawlSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (searchtype.SelectedIndex == 0) // search by Content
            {
                fromdatabase = Crawlable.DBSelectByContent(crawlsearch.Text);
                databaselist.ItemsSource = fromdatabase;
            }
            else if (searchtype.SelectedIndex == 1) // search by text | Content
            {
                fromdatabase = Crawlable.DBSelectByTextContent(crawlsearch.Text);
                databaselist.ItemsSource = fromdatabase;
            }
        }

        private void CrawlCrawlable(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(crawlurl.Text))
                    return;
            craWaiting.IsActive = true;

            currentService = new CrawlerService(crawlurl.Text);
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
                        ApplicationMessage.SendMessage(new ShortMessage { Title = "CrawlerException", Content = exception, Time = DateTimeOffset.Now },
                            ApplicationMessage.MessageType.InApp);
                    });
                });
        }

        private void SaveCrawlable(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(crawltext.Text) || string.IsNullOrEmpty(crawlurl.Text))
            {
                ApplicationMessage.SendMessage(new ShortMessage { Title = "CrawlerWarning", Content = "There must be Text and Url", Time = DateTimeOffset.Now }, 
                    ApplicationMessage.MessageType.InApp);
                return;
            }

            string id = HashEncode.MakeMD5(crawlurl.Text);
            if (Crawlable.DBSelectByID(id).Count != 0)
                Crawlable.DBDeleteByID(id); // modify or delete?
            Crawlable.DBInsert(new List<Crawlable>()
                {
                    new Crawlable
                    {
                        ID = id,
                        ParentID = "",
                        Text = crawltext.Text,
                        Url = crawlurl.Text,
                        Content = crawlcontent.Text,
                        Tags = crawltags.Text,
                        Filters = crawlfilters.Text,
                    }
                });
            if (!string.IsNullOrEmpty(crawltags.Text))
                Topic.SaveTag(crawltags.Text);
        }

        private async void SaveFavorite(object sender, RoutedEventArgs e)
        {
            if (currentFavor != null) // favorite is selected
            {
                if (!currentFavor.Equals(crawltext.Text)) // rename the favorite
                    favorites.Remove(currentFavor);
                currentFavor = new Crawlable
                {
                    ID = HashEncode.MakeMD5(crawlurl.Text),
                    ParentID = "",
                    Text = crawltext.Text,
                    Url = crawlurl.Text,
                    Content = crawlcontent.Text,
                    Tags = crawltags.Text,
                    Filters = crawlfilters.Text,
                };
                favorites.Add(currentFavor);
                favoritepanel.IsPaneOpen = true;
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "favorite.list", favorites);
            }
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
            crawlurl.Text = currentCrawlable.Url;
            crawlcontent.Text = currentCrawlable.Content;
            crawltags.Text = currentCrawlable.Tags;
            crawlfilters.Text = currentCrawlable.Filters;
        }

        private void ServiceList_ItemClick(object sender, ItemClickEventArgs e)
        {
            CleanCrawlPanel();

            var item = e.ClickedItem as Crawlable;
            crawltext.Text = item.Text;
            crawlurl.Text = item.Url;
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
