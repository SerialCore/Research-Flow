using LogicService.Data;
using LogicService.Security;
using LogicService.Service;
using LogicService.Storage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                CrawlerService first = e.Parameter as CrawlerService;
                if (first != null)
                {
                    Crawlable crawlable = new Crawlable()
                    {
                        ID = HashEncode.MakeMD5(first.Url),
                        ParentID = "Null",
                        Text = first.Title,
                        Url = first.Url,
                        Content = first.Content,
                        Tags = "Null"
                    };
                }
            }

            InitializeFavorite();
        }

        private async void InitializeFavorite()
        {
            try
            {
                favorites = await LocalStorage.ReadJsonAsync<List<Crawlable>>(
                    await LocalStorage.GetDataFolderAsync(), "favoritelist");
            }
            catch
            {
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "favoritelist", favorites);
            }
            finally
            {
                favoritelist.ItemsSource = favorites;
            }
        }

        private void InitializeCrawl()
        {
            
        }

        #region Favorite

        private List<Crawlable> favorites = new List<Crawlable>();

        #endregion

        #region Crawler

        private ObservableCollection<Crawlable> crawlables = new ObservableCollection<Crawlable>();

        private void OpenCrawPanel_Click(object sender, RoutedEventArgs e) => crawpanel.IsPaneOpen = true;

        #endregion

    }
}
