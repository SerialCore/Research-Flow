using LogicService.Application;
using LogicService.Helper;
using LogicService.Services;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Research_Flow
{
    public delegate void MessageHandle(object sender);

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            
            ConfigureTask();
            ApplicationMessage.MessageReached += AppMessage_MessageReached;
        }

        private async void AppMessage_MessageReached(string message, ApplicationMessage.MessageType type)
        {
            switch (type)
            {
                case ApplicationMessage.MessageType.TopBanner:
                    appMessage.Text = message;
                    await Task.Delay(3000);
                    appMessage.Text = "";
                    break;
                case ApplicationMessage.MessageType.InAppNotification:
                    InAppNotification.Show(message);
                    break;
            }
        }

        private async void ConfigureTask()
        {
            await ApplicationTask.RegisterSearchTask();
            await ApplicationTask.RegisterStorageTask();
            await ApplicationTask.RegisterTopicTask();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            FileParameter = e.Parameter as StorageFile;

            if (GraphService.IsConnected)
            {
                try
                {
                    string name = await GraphService.GetDisplayName();
                    string email = await GraphService.GetPrincipalName();
                    BitmapImage image = new BitmapImage();
                    image.UriSource = new Uri("ms-appx:///Images/ResearchFlow_logo.jpg");
                    accountName.Text = name;
                    accountEmail.Text = email;
                    accountPhoto.ProfilePicture = image;
                }
                catch (Exception ex)
                {
                    InAppNotification.Show(ex.Source + ": " + ex.Message);
                }
            }
            else
            {
                accountName.Text = "Offline";
                accountEmail.Text = ApplicationSetting.AccountName;
            }
        }

        #region NavView

        private StorageFile FileParameter;

        // use of anonymous class
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("Overview", typeof(Overview)),
            ("TagTopic", typeof(TagTopic)),
            ("Paper", typeof(Paper)),
            ("RSS", typeof(RSS)),
            ("Search", typeof(Search)),
            ("Crawler", typeof(Crawler)),
            ("Note", typeof(Note)),
        };

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // you can also add items in code behind
            //NavView.MenuItems.Add(new NavigationViewItemSeparator());
            if (FileParameter != null)
            {
                if (FileParameter.FileType.Equals(".rfn"))
                {
                    ContentFrame.Navigate(typeof(Note), FileParameter);
                }
            }
            else
            {
                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals("Overview"));
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
                NavView_Navigate(args.SelectedItem as NavigationViewItem);
            }
        }

        private void NavView_Navigate(NavigationViewItem pageItem)
        {
            var item = _pages.FirstOrDefault(p => p.Tag.Equals(pageItem.Tag));
            
            ContentFrame.Navigate(item.Page);
        }

        private void NavView_Navigated(object sender, NavigationEventArgs e)
        {
            if (ContentFrame.SourcePageType == typeof(Settings))
            {
                NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
            }
            else if (ContentFrame.SourcePageType != null)
            {
                var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals(item.Tag));
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
            => ContentFrame.GoBack();

        #endregion

        #region Account

        private void Logout()
        {
            GraphService.OneDriveLogout();
            
            ContentFrame.IsEnabled = false;
            accountLogout.Content = "restart this app";
            accountName.Text = "";
            accountEmail.Text = "";
        }

        private async void AccountSync_Click(object sender, RoutedEventArgs e)
        {
            if (GraphService.IsConnected && GraphService.IsNetworkAvailable)
            {
                ApplicationMessage.SendMessage("Synchronizing", ApplicationMessage.MessageType.TopBanner);
                await Synchronization.ScanFiles();
                ApplicationMessage.SendMessage("Synchronized successfully", ApplicationMessage.MessageType.TopBanner);
            }
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
            if (ApplicationSetting.ContainKey("AccountName"))
                Logout();
            else
                await CoreApplication.RequestRestartAsync(string.Empty);
        }

        private void CancelInvokedHandler(IUICommand command) { }

        #endregion

        #region Content

        private void Flyout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        private async void ScreenShot_Export(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                SuggestedFileName = "ScreenShot-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png",
            };
            picker.FileTypeChoices.Add("ScreenShot", new string[] { ".png" });
            StorageFile file = await picker.PickSaveFileAsync();
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
            StorageFile file = await LocalStorage.GetTemporaryFolder().CreateFileAsync("ScreenShot-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png");
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
            StorageFile file = await LocalStorage.GetTemporaryFolder().CreateFileAsync("ScreenShot-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png");
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
                    await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetPictureAsync(), file);
                    ToastGenerator.ShowTextToast("OneDrive", "Screen Shot Saved");
                }
                catch
                {
                    await file.CopyAsync(KnownFolders.PicturesLibrary, file.Name);
                    ToastGenerator.ShowTextToast("Pictures Library", "Screen Shot Saved");
                }
            }
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
            }
            else
            {
                view.TryEnterFullScreenMode();
            }
        }

        #endregion

    }

    public class ApplicationTask
    {

        /*
         * UserPresent and UserAway are both most frequent
         */

        public static async Task<BackgroundTaskRegistration> RegisterTopicTask()
        {
            return await RegisterBackgroundTask(typeof(CoreFlow.TopicTask),
                "LearnTask", new SystemTrigger(SystemTriggerType.UserPresent, false),
                new SystemCondition(SystemConditionType.InternetAvailable));
        }

        public static async Task<BackgroundTaskRegistration> RegisterSearchTask()
        {
            return await RegisterBackgroundTask(typeof(CoreFlow.SearchTask),
                "SearchTask", new SystemTrigger(SystemTriggerType.UserAway, false),
                new SystemCondition(SystemConditionType.InternetAvailable));
        }

        public static async Task<BackgroundTaskRegistration> RegisterStorageTask()
        {
            return await RegisterBackgroundTask(typeof(CoreFlow.StorageTask),
                "StorageTask", new SystemTrigger(SystemTriggerType.InternetAvailable, false),
                new SystemCondition(SystemConditionType.InternetAvailable));
        }

        private static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(Type taskEntryPoint,
            string taskName, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.Unspecified || status == BackgroundAccessStatus.DeniedByUser
                || status == BackgroundAccessStatus.DeniedByUser)
            {
                return null;
            }

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == taskName)
                {
                    cur.Value.Unregister(true);
                }
            }

            var builder = new BackgroundTaskBuilder
            {
                Name = taskName,
                TaskEntryPoint = taskEntryPoint.FullName
            };

            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);
            }

            BackgroundTaskRegistration task = builder.Register();
            return task;
        }

    }

}
