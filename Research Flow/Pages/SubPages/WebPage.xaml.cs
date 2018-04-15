using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow.Pages.SubPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WebPage : Page
    {
        public WebPage()
        {
            this.InitializeComponent();
            webView.FrameNavigationCompleted += WebView_FrameNavigationCompleted;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            webWaiting.IsActive = true;
            string[] site = e.Parameter as string[];
            webTitle.Text = site[0];
            webView.Navigate(new Uri(site[1]));
        }

        private void WebView_FrameNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            webWaiting.IsActive = false;
        }

        private void Return_Navigate(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void SavetoLearn(object sender, RoutedEventArgs e)
        {

        }

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
        {
            webWaiting.IsActive = true;
            webView.Refresh();
        }
    }
}
