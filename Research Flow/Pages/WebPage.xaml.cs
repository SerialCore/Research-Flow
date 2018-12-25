using LogicService.Objects;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WebPage : Page
    {
        public WebPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
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
                webView.Navigate(new Uri(siteUrl.Text));
            }
            catch
            {

            }
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
            => webWaiting.IsActive = true;

        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
            => webWaiting.IsActive = false;

        private void WebView_FrameNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
            => webWaiting.IsActive = false;

        private void Return_Navigate(object sender, RoutedEventArgs e)
            => this.Frame.GoBack();

        private void Back(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoBack)
                webView.GoBack();
        }

        private void Forward(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoForward)
                webView.GoForward();
        }

        private void Refresh(object sender, RoutedEventArgs e)
            => webView.Refresh();

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
