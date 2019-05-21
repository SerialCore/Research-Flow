using System;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WebPage : Page
    {
        public WebPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            webView.Source = new Uri("https://blamder.github.io/Research-Flow/");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string link = e.Parameter as string;
            if (!string.IsNullOrEmpty(link))
                webView.Navigate(new Uri(link));
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            try
            {
                string uri = siteUrl.Text;
                uri = uri.StartsWith("http") ? uri : "http://" + uri;
                webView.Navigate(new Uri(uri));
            }
            catch
            {

            }
        }

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

        private void Back(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoBack) webView.GoBack();
        }

        private void Forward(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoForward) webView.GoForward();
        }

        private void Refresh(object sender, RoutedEventArgs e)
            => webView.Refresh();

        private async void OpenBrowser(object sender, RoutedEventArgs e)
            => await Launcher.LaunchUriAsync(webView.Source);

        private void SavetoLearn(object sender, RoutedEventArgs e)
        {

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
                request.Data.Properties.Title = "Search Result";
                request.Data.Properties.Description = "Share your search result";

                request.Data.SetWebLink(webView.Source);

                // end if deferral
                deferral.Complete();
            }
        }

    }
}
