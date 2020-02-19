using LogicService.Application;
using LogicService.Data;
using LogicService.Security;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                Crawlable crawlable = e.Parameter as Crawlable;
                if (crawlable != null)
                {

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
            
        }

        #region Favorite

        private List<Crawlable> favorites = new List<Crawlable>();

        private void OpenFavorites_Click(object sender, RoutedEventArgs e) => crawpanel.IsPaneOpen = true;

        private void FavoriteList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Crawlable crawlable = e.ClickedItem as Crawlable;
            currentCrawlable = crawlable;
            crawltext.Text = crawlable.Text;
            crawlurl.Content = crawlable.Url;
            crawlurl.NavigateUri = new Uri(crawlable.Url);
            crawlcontent.Text = crawlable.Content;
            crawltags.Text = crawlable.Tags;
            crawlfilters.Text = crawlable.Filters;

            crawpanel.IsPaneOpen = true;
        }

        #endregion

        #region Crawler

        private Crawlable currentCrawlable = null;

        private ObservableCollection<Crawlable> crawlables = new ObservableCollection<Crawlable>();

        private void SaveCrawlable(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(crawltext.Text) || string.IsNullOrEmpty(crawlurl.Content as string))
            {
                ApplicationMessage.SendMessage("CrawlerWarning: There must be Text and Url", ApplicationMessage.MessageType.InApp);
                return;
            }

            string id = HashEncode.MakeMD5(crawlurl.Content as string);
            if (Crawlable.DBSelectByID(id).Count == 0)
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
            if (string.IsNullOrEmpty(crawlurl.Content as string))
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

            crawltext.Text = "";
            crawlurl.Content = "Link";
            crawlurl.NavigateUri = null;
            crawlcontent.Text = "";
            crawltags.Text = "";
            crawlfilters.Text = "";
        }

        private void CancelInvokedHandler(IUICommand command) { }

        #endregion

    }
}
