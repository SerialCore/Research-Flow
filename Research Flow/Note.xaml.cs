using LogicService.Application;
using LogicService.Storage;
using Microsoft.Graphics.Canvas;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Note : Page
    {
        public Note()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            StorageFile file = e.Parameter as StorageFile;
            if (file != null)
            {
                try
                {
                    canvas.ImportFromJson(await FileIO.ReadTextAsync(file));
                }
                catch (Exception ex)
                {
                    InAppNotification.Show(ex.Message);
                }
            }
            else
            {
                LoadDefaultNote(null, null);
            }

            InitializeNote();
        }

        /// <summary>
        /// Add file, rename file or delete file, then re-Initialize
        /// </summary>
        private async void InitializeNote()
        {
            var filelist = await (await LocalStorage.GetNoteAsync()).GetFilesAsync(CommonFileQuery.OrderByName);
            var namelist = new List<NoteItem>();
            foreach(var file in filelist)
            {
                // name or displayname contains extention
                namelist.Add(new NoteItem { NoteName = file.Name});
            }
            notelist.ItemsSource = namelist;
        }

        #region File Operation

        private async void LoadDefaultNote(object sender, RoutedEventArgs e)
        {
            var defaultnote = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Resources/DefaultNote.txt"));
            canvas.ImportFromJson(await FileIO.ReadTextAsync(defaultnote));
            notefilename.Text = "";
        }

        private async void Import_Note(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".rfn");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                try
                {
                    canvas.ImportFromJson(await FileIO.ReadTextAsync(file));
                }
                catch (Exception ex)
                {
                    InAppNotification.Show(ex.Message);
                }
            }
        }

        private async void Export_Image(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                SuggestedFileName = "Note-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png",
            };
            picker.FileTypeChoices.Add("Note", new string[] { ".png" });
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await canvas.SaveBitmapAsync(stream, BitmapFileFormat.Png);
                }
            }
        }

        private async void Export_Note(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                SuggestedFileName = "Note-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".rfn",
            };
            picker.FileTypeChoices.Add("Research Flow Note", new string[] { ".rfn" });
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                await FileIO.WriteTextAsync(file, canvas.ExportAsJson());
            }
        }

        private void Share_Image(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += ImageTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void Share_Note(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += JsonTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private async void ImageTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequestDeferral deferral = args.Request.GetDeferral();

            DataRequest request = args.Request;
            request.Data.Properties.Title = "Research Flow Note";
            request.Data.Properties.Description = "Share your research note";

            StorageFile file = await LocalStorage.GetTemporaryFolder().CreateFileAsync("Note-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png");

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await canvas.SaveBitmapAsync(stream, BitmapFileFormat.Png);
            }

            RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromFile(file);
            request.Data.Properties.Thumbnail = imageStreamRef;
            request.Data.SetBitmap(imageStreamRef);

            deferral.Complete();
        }

        private async void JsonTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequestDeferral deferral = args.Request.GetDeferral();

            DataRequest request = args.Request;
            request.Data.Properties.Title = "Research Flow Note";
            request.Data.Properties.Description = "Share your research note";

            StorageFile file = await LocalStorage.GetTemporaryFolder().CreateFileAsync("Note-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".rfn");

            if (file != null)
            {
                await FileIO.WriteTextAsync(file, canvas.ExportAsJson());
            }

            var storage = new List<IStorageItem>();
            storage.Add(file);
            request.Data.SetStorageItems(storage);

            deferral.Complete();
        }

        #endregion

        #region File Management

        private void Open_Document(object sender, RoutedEventArgs e)
            => notepanel.IsPaneOpen = !notepanel.IsPaneOpen;

        private void Flyout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        private async void Notelist_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as NoteItem;
            var fileitem = await (await LocalStorage.GetNoteAsync()).GetFileAsync(item.NoteName);
            canvas.ImportFromJson(await FileIO.ReadTextAsync(fileitem));
            notefilename.Text = item.NoteName;
        }

        private async void Save_Note(object sender, RoutedEventArgs e)
        {
            if (notefilename.Text.Equals(""))
                LocalStorage.GeneralWrite(await LocalStorage.GetNoteAsync(),
                    "Note-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".rfn",
                    canvas.ExportAsJson());
            else
                LocalStorage.GeneralWrite(await LocalStorage.GetNoteAsync(),
                    notefilename.Text,
                    canvas.ExportAsJson());

            ApplicationMessage.SendMessage("Note saved", 3);
            await Task.Delay(1000);
            InitializeNote();
        }

        private async void Delete_Note(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("You are about to delete application data, please tell me that is not true.", "Operation confirming");
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
            var item = notelist.SelectedItem as NoteItem;
            LocalStorage.GeneralDelete(await LocalStorage.GetNoteAsync(), item.NoteName);
            await Task.Delay(1000);
            InitializeNote();
            notefilename.Text = "";
        }

        private void CancelInvokedHandler(IUICommand command) { }

        #endregion

    }

    public class NoteItem
    {
        public string NoteName { get; set; }

    }
}
