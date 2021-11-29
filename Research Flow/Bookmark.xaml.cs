using LogicService.Application;
using LogicService.Data;
using LogicService.Storage;
using System;
using System.Collections.Generic;
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

        #region List Operation

        private List<Feed> bookmarks = new List<Feed>();

        private void BookmarkList_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        #endregion

    }
}
