using LogicService.Storage;
using Microsoft.Graphics.Canvas;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
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
            this.NavigationCacheMode = NavigationCacheMode.Required;
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
                var defaultnote = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Resources/DefaultNote.note"));
                canvas.ImportFromJson(await FileIO.ReadTextAsync(defaultnote));
            }
        }

        #region File Operation

        private async void Save_Note(object sender, RoutedEventArgs e)
        {
            await LocalStorage.WriteText(await LocalStorage.GetNoteAsync(),
                "Note-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".note",
                canvas.ExportAsJson());
        }

        private async void Export_Note(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                SuggestedFileName = "Note-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".note",
            };
            picker.FileTypeChoices.Add("Note", new string[] { ".note" });
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                await FileIO.WriteTextAsync(file, canvas.ExportAsJson());
            }
        }

        private async void Import_Note(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".note");
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

        private void Share_Note(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequestDeferral deferral = args.Request.GetDeferral();

            DataRequest request = args.Request;
            request.Data.Properties.Title = "Research Note";
            request.Data.Properties.Description = "Share your research note";

            StorageFile file = await LocalStorage.WriteText(await LocalStorage.GetNoteAsync(),
                "Note-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".note",
                canvas.ExportAsJson());

            RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromFile(file);
            request.Data.Properties.Thumbnail = imageStreamRef;
            request.Data.SetBitmap(imageStreamRef);

            deferral.Complete();
        }

        #endregion

        #region File Management

        private void Open_Document(object sender, RoutedEventArgs e)
            => notepanel.IsPaneOpen = !notepanel.IsPaneOpen;

        #endregion

    }
}
