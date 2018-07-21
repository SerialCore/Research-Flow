using LogicService.Helper;
using LogicService.Services;
using LogicService.Storage;
using Research_Flow.Pages;
using Research_Flow.Pages.SubPages;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(ApplicationData.Current.LocalSettings.Values.ContainsKey("AccountName"))
                Login();
            else
            {
                account_panel.Visibility = Visibility.Visible;
                account_panel_background.Visibility = Visibility.Visible;
                NavView.IsEnabled = false;
                accountToggle.IsOn = false;
                account_exit.IsEnabled = false;
            }

        }

        #region NavView

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // you can also add items in code behind
            //NavView.MenuItems.Add(new NavigationViewItemSeparator());

            // set the initial SelectedItem
            foreach (NavigationViewItemBase item in NavView.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == "Overview")
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(Settings));
            }
            else
            {
                // find NavigationViewItem with Content that equals InvokedItem
                var item = sender.MenuItems.OfType<NavigationViewItem>().First(x => (string)x.Content == (string)args.InvokedItem);
                NavView_Navigate(item as NavigationViewItem);

            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(Settings));
            }
            else
            {
                NavigationViewItem item = args.SelectedItem as NavigationViewItem;
                NavView_Navigate(item);
            }
        }

        private void NavView_Navigate(NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "Overview":
                    ContentFrame.Navigate(typeof(Overview));
                    break;

                case "Contact":
                    ContentFrame.Navigate(typeof(Contact));
                    break;

                case "Topic":
                    ContentFrame.Navigate(typeof(Topic));
                    break;

                case "Search":
                    ContentFrame.Navigate(typeof(Search));
                    break;

                case "Learn":
                    ContentFrame.Navigate(typeof(Learn));
                    break;

                case "Compose":
                    ContentFrame.Navigate(typeof(Compose));
                    break;

                case "Tool":
                    ContentFrame.Navigate(typeof(Tool));
                    break;

            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (NavView.SelectedItem == NavView.SettingsItem)
            {
                ContentFrame.Navigate(typeof(Settings));
            }
            else
            {
                NavView_Navigate(NavView.SelectedItem as NavigationViewItem);
            }
        }

        private void WebPage_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(WebPage));
        }

        #endregion

        #region Account

        private async void Login()
        {
            if (await GraphService.ServiceLogin())
            {
                accountStatu.Text = await GraphService.GetDisplayName();
                ApplicationData.Current.LocalSettings.Values["AccountName"] = await GraphService.GetPrincipalName();
                accountToggle.OnContent = await GraphService.GetPrincipalName();
                accountToggle.IsOn = true;
                account_exit.IsEnabled = true;
            }
            else
            {
                accountStatu.Text = "Serivce Offline";
            }

            // show user info after handling
            signWait.ShowPaused = true;
            signWait.Visibility = Visibility.Collapsed;
            signPane.Visibility = Visibility.Visible;
        }

        private void Logout()
        {
            GraphService.ServiceLogout();

            ApplicationData.Current.LocalSettings.Values.Remove("AccountName");
            accountToggle.OnContent = "On";
            accountToggle.IsOn = false;
            account_exit.IsEnabled = false;
            accountStatu.Text = "Serivce Offline";
        }

        private void Service_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            account_panel.Visibility = Visibility.Visible;
            account_panel_background.Visibility = Visibility.Visible;
            NavView.IsEnabled = false;

            accountToggle.OnContent = ApplicationData.Current.LocalSettings.Values["AccountName"] as string;
            accountToggle.IsOn = true;
        }

        private void AccountToggle_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch.IsOn == true)
                Login();
            else
                Logout();
        }

        private void Account_exit_Click(object sender, RoutedEventArgs e)
        {
            account_panel.Visibility = Visibility.Collapsed;
            account_panel_background.Visibility = Visibility.Collapsed;
            NavView.IsEnabled = true;
        }

        #endregion

        #region Content

        private void Flyout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void ScreenShot_Share(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequestDeferral deferral = args.Request.GetDeferral();

            DataRequest request = args.Request;
            request.Data.Properties.Title = "ScreenShot";
            request.Data.Properties.Description = "Share your current idea";

            var bitmap = new RenderTargetBitmap();
            StorageFile file = await (await LocalStorage.GetPhotosAsync()).CreateFileAsync("ResearchFlow-ScreenShot.png", CreationCollisionOption.ReplaceExisting);
            await bitmap.RenderAsync(FullPage);
            var buffer = await bitmap.GetPixelsAsync();
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encode = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encode.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)bitmap.PixelWidth,
                    (uint)bitmap.PixelHeight,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    buffer.ToArray()
                   );
                await encode.FlushAsync();
            }

            RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromFile(file);
            request.Data.Properties.Thumbnail = imageStreamRef;
            request.Data.SetBitmap(imageStreamRef);

            deferral.Complete();
        }

        private async void ScreenShot_Save(object sender, RoutedEventArgs e)
        {
            var bitmap = new RenderTargetBitmap();
            StorageFile file = await (await LocalStorage.GetPhotosAsync()).CreateFileAsync("ResearchFlow-ScreenShot.png", CreationCollisionOption.ReplaceExisting);
            await bitmap.RenderAsync(FullPage);
            var buffer = await bitmap.GetPixelsAsync();
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encode = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encode.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)bitmap.PixelWidth,
                    (uint)bitmap.PixelHeight,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    buffer.ToArray()
                   );
                await encode.FlushAsync();
            }

            if (file != null)
            {
                // confirm the app was associated with Microsoft account
                try
                {
                    await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetPhotosAsync(), file);
                    ToastNotificationManager.CreateToastNotifier().Show(
                        new ToastNotification(ToastGenerator.TextToast("OneDrive", "Screen Shot Saved").GetXml()));
                }
                catch
                {
                    await file.CopyAsync(KnownFolders.PicturesLibrary, "ResearchFlow-ScreenShot.png", NameCollisionOption.GenerateUniqueName);
                    ToastNotificationManager.CreateToastNotifier().Show(
                        new ToastNotification(ToastGenerator.TextToast("Pictures Library", "Screen Shot Saved").GetXml()));
                }
            }
        }

        #endregion

    }

}
