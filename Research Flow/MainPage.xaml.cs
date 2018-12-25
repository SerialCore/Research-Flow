﻿using LogicService.Helper;
using LogicService.Services;
using LogicService.Storage;
using Research_Flow.Pages;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Login();
            await Task.Run(() =>
            {
                Synchronization.ScanChanges();
            });
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

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            On_BackRequested();
        }

        private bool On_BackRequested()
        {
            if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed.
            if (NavView.IsPaneOpen &&
                (NavView.DisplayMode == NavigationViewDisplayMode.Compact ||
                 NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
                return false;

            ContentFrame.GoBack();
            return true;
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

            if (ContentFrame.SourcePageType == typeof(Settings))
            {
                //SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
                NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
                NavView.Header = "Settings";
            }
            else if (ContentFrame.SourcePageType != null)
            {
                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals("Overview"));

                NavView.Header =
                    ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
            }
        }

        private void WebPage_Click(object sender, RoutedEventArgs e)
            => ContentFrame.Navigate(typeof(WebPage));

        private void Open_Tutorials(object sender, RoutedEventArgs e)
            => ContentFrame.Navigate(typeof(Tutorials));

        #endregion

        #region Account

        private async void Login()
        {
            if (await GraphService.ServiceLogin())
            {
                string name = await GraphService.GetDisplayName();
                string email = await GraphService.GetPrincipalName();
                BitmapImage image = new BitmapImage();
                image.UriSource = new Uri("ms-appx:///Assets/Logos/ResearchFlow_logo.jpg");
                accountName.Text = name;
                accountEmail.Text = email;
                accountPhoto.ProfilePicture = image;
            }
            else
            {
                accountName.Text = "Offline";
            }
        }

        private void Logout()
        {
            GraphService.ServiceLogout();

            ApplicationService.RemoveKey("AccountName");
            ContentFrame.IsEnabled = false;
            ContentHeader.IsEnabled = false;
            accountLogout.Content = "restart this app";
            accountName.Text = "";
            accountEmail.Text = "";
        }

        private async void AccountSync_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                Synchronization.ScanChanges();
            });
        }

        private async void AccountLogout_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("Are you sure to log out?", "Operation confirming");
            messageDialog.Commands.Add(new UICommand(
                "True",
                new UICommandInvokedHandler(this.DeleteInvokedHandler)));
            messageDialog.Commands.Add(new UICommand(
                "Joke",
                new UICommandInvokedHandler(this.CancelInvokedHandler)));

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync();
        }

        private async void DeleteInvokedHandler(IUICommand command)
        {
            if (ApplicationService.ContainsKey("AccountName"))
                Logout();
            else
                await CoreApplication.RequestRestartAsync(string.Empty);
        }

        private void CancelInvokedHandler(IUICommand command) { }

        #endregion

        #region Content

        private void Flyout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        private async void ScreenShot_Save(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("ScreenShot", new string[] { ".png" });
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.SuggestedFileName = "ScreenShot-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
            StorageFile file=await picker.PickSaveFileAsync();
            if (file != null)
            {
                var bitmap = new RenderTargetBitmap();
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
            }
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
            StorageFile file = await (await LocalStorage.GetPhotoAsync()).CreateFileAsync("ScreenShot-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png", 
                CreationCollisionOption.ReplaceExisting);
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

        private async void ScreenShot_Upload(object sender, RoutedEventArgs e)
        {
            var bitmap = new RenderTargetBitmap();
            StorageFile file = await (await LocalStorage.GetPhotoAsync()).CreateFileAsync("ScreenShot-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png", 
                CreationCollisionOption.ReplaceExisting);
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
                    await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetPhotoAsync(), file);
                    ToastNotificationManager.CreateToastNotifier().Show(
                        new ToastNotification(ToastGenerator.TextToast("OneDrive", "Screen Shot Saved").GetXml()));
                }
                catch
                {
                    await file.CopyAsync(KnownFolders.PicturesLibrary, file.Name, NameCollisionOption.ReplaceExisting);
                    ToastNotificationManager.CreateToastNotifier().Show(
                        new ToastNotification(ToastGenerator.TextToast("Pictures Library", "Screen Shot Saved").GetXml()));
                }
            }
        }

        private async void Give_Rate(object sender, RoutedEventArgs e)
            => await ApplicationService.ShowRatingReviewDialog();

        private async void Give_FeedBack(object sender, RoutedEventArgs e)
            => await Launcher.LaunchUriAsync(new Uri("mailto://zwx.atomx@outlook.com"));

        #endregion

    }

}
