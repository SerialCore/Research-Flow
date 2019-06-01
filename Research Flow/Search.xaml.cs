using LogicService.Objects;
using LogicService.Security;
using LogicService.Services;
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
                    { "Bing", "https://www.bing.com/search?q=QUEST" },
                };
                LocalStorage.WriteJson(await LocalStorage.GetLinkAsync(), "searchlist", SearchSources);
            }
            finally
            {
                searchlist.ItemsSource = SearchSources.Keys;
                searchlist.SelectedIndex = 0;
            }
        }

        public Dictionary<string, string> SearchSources { get; set; }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var urlstring = SearchSources.GetValueOrDefault(searchlist.SelectedItem as string).Replace("QUEST", queryQuest.Text);
            webview.Navigate(new Uri(urlstring));
        }
    }
}
