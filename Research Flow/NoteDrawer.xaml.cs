using LogicService.Application;
using LogicService.Data;
using LogicService.Drawing;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NoteDrawer : Page
    {
        public NoteDrawer()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            InitializePanel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                if (e.Parameter.GetType().Equals(typeof(StorageFile)))
                {
                    StorageFile file = e.Parameter as StorageFile;
                    if (!notefilename.Text.Equals(file.DisplayName))
                    {
                        try
                        {
                            ImportFromInk(file);
                            notefilename.Text = file.DisplayName;
                        }
                        catch (Exception ex)
                        {
                            ApplicationMessage.SendMessage(new ShortMessage { Title = "NoteException", Content = ex.Message, Time = DateTimeOffset.Now },
                                ApplicationMessage.MessageType.InApp);
                        }
                    }
                }
            }

            InitializeNote();
        }

        private void InitializePanel()
        {
            colorlist.ItemsSource = ColorCollection.PaneColors;
            penlist.ItemsSource = new ObservableCollection<PenCollection>()
            {
                new PenCollection { Pen = new UsualPen(), Name = "Usual Pen", Image = "ms-appx:///Images/UsualPen.jpg" },
                new PenCollection { Pen = new MarkerPen(), Name = "Marker Pen", Image = "ms-appx:///Images/MarkerPen.jpg" },
                new PenCollection { Pen = new CalligraphyPen(), Name = "Calligraphy Pen", Image = "ms-appx:///Images/CalligraphyPen.jpg" },
                new PenCollection { Pen = new PencilBrush(), Name = "Pencil Brush", Image = "ms-appx:///Images/PencilBrush.jpg" },
                new PenCollection { Pen = new InkBrush(), Name = "Ink Brush", Image = "ms-appx:///Images/InkBrush.jpg" }
            };
            inkToolbar.Loaded += InkToolbar_Loaded;
        }

        private async void InitializeNote()
        {
            namelist.Clear();
            var filelist = await LocalStorage.GetNoteFolderAsync().GetFilesAsync();
            foreach (var file in filelist)
            {
                namelist.Add(file.DisplayName.Replace(".rfn", ""));
            }
            notelist.ItemsSource = namelist;
        }

        private void Flyout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);

        #region Ink Operation

        private Stack<InkStroke> UndoStrokes = new Stack<InkStroke>();

        private void UndoInk(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<InkStroke> strokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            if (strokes.Count > 0)
            {
                strokes[strokes.Count - 1].Selected = true;
                UndoStrokes.Push(strokes[strokes.Count - 1]); // 入栈
                inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            }
        }

        private void RedoInk(object sender, RoutedEventArgs e)
        {
            if (UndoStrokes.Count > 0)
            {
                var stroke = UndoStrokes.Pop();

                // This will blow up sky high:
                // InkCanvas.InkPresenter.StrokeContainer.AddStroke(stroke);

                var strokeBuilder = new InkStrokeBuilder();
                strokeBuilder.SetDefaultDrawingAttributes(stroke.DrawingAttributes);
                System.Numerics.Matrix3x2 matr = stroke.PointTransform;
                IReadOnlyList<InkPoint> inkPoints = stroke.GetInkPoints();
                InkStroke stk = strokeBuilder.CreateStrokeFromInkPoints(inkPoints, matr);
                inkCanvas.InkPresenter.StrokeContainer.AddStroke(stk);
            }
        }

        private async void ImportFromInk(StorageFile file)
        {
            try
            {
                var stream = await file.OpenReadAsync();
                await inkCanvas.InkPresenter.StrokeContainer.LoadAsync(stream);
            }
            catch (Exception ex)
            {
                ApplicationMessage.SendMessage(new ShortMessage { Title = "NoteException", Content = ex.Message, Time = DateTimeOffset.Now },
                    ApplicationMessage.MessageType.InApp);
            }
        }

        private async void ExportAsInk(StorageFile file)
        {
            try
            {
                IRandomAccessStream stream = new InMemoryRandomAccessStream();
                await inkCanvas.InkPresenter.StrokeContainer.SaveAsync(stream);
                CachedFileManager.DeferUpdates(file);
                var bt = await ConvertStreamtoByte(stream);
                await FileIO.WriteBytesAsync(file, bt);
                await CachedFileManager.CompleteUpdatesAsync(file);
            }
            catch (Exception ex)
            {
                ApplicationMessage.SendMessage(new ShortMessage { Title = "NoteException", Content = ex.Message, Time = DateTimeOffset.Now },
                    ApplicationMessage.MessageType.InApp);
            }
        }

        private async Task<byte[]> ConvertStreamtoByte(IRandomAccessStream fileStream)
        {
            var reader = new DataReader(fileStream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)fileStream.Size);
            byte[] pixels = new byte[fileStream.Size];
            reader.ReadBytes(pixels);
            return pixels;
        }

        #endregion

        #region Panel Setting

        private void InkToolbar_Loaded(object sender, RoutedEventArgs e)
        {
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;

            inkToolbar.ActiveTool = inkToolbar.GetToolButton(InkToolbarTool.BallpointPen);
            customPen.CustomPen = new UsualPen();
            customPen.Palette = ColorCollection.ToolColors;
        }

        private void ColorList_ItemClick(object sender, ItemClickEventArgs e)
            => paneColor.Fill = (e.ClickedItem as ColorCollection).ColorBrush;

        private void PenList_ItemClick(object sender, ItemClickEventArgs e)
        {
            customPen.CustomPen = (e.ClickedItem as PenCollection).Pen;
            // to activate the CustomPen
            customPen.SelectedStrokeWidth = 1;
            customPen.SelectedStrokeWidth = 2;
        }

        private void ChooseInputDevice(object sender, RoutedEventArgs e)
        {
            var button = sender as AppBarButton;
            if (button.Tag.Equals("pen"))
            {
                this.inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse
                | Windows.UI.Core.CoreInputDeviceTypes.Touch;
                button.Tag = "touch";
                button.Icon = new SymbolIcon((Symbol)0xED5F);
            }
            else
            {
                this.inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Pen;
                button.Tag = "pen";
                button.Icon = new SymbolIcon((Symbol)0xEDC6);
            }
        }

        #endregion

        #region File Operation (out App)

        private async void Import_Note(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".rfn");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                ImportFromInk(file);
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
                ExportAsInk(file);
            }
        }

        private void Share_Note(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += InkTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private async void InkTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequestDeferral deferral = args.Request.GetDeferral();

            DataRequest request = args.Request;
            request.Data.Properties.Title = "Research Flow Note";
            request.Data.Properties.Description = "Share your research note";

            StorageFile file = await LocalStorage.GetTemporaryFolder().CreateFileAsync("Note-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".rfn");

            if (file != null)
            {
                ExportAsInk(file);
            }

            var storage = new List<IStorageItem>();
            storage.Add(file);
            request.Data.SetStorageItems(storage);

            deferral.Complete();
        }

        #endregion

        #region File Management (in App)

        private ObservableCollection<string> namelist = new ObservableCollection<string>();

        private void Open_Document(object sender, RoutedEventArgs e)
            => notepanel.IsOpen = true;

        private async void Notelist_ItemClick(object sender, ItemClickEventArgs e)
        {
            var name = e.ClickedItem as string;
            var fileitem = await LocalStorage.GetNoteFolderAsync().GetFileAsync(name + ".rfn");
            ImportFromInk(fileitem);
            notefilename.Text = name;
        }

        private async void notefilename_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string notename; // without extention
            if (notefilename.Text.Equals(""))
            {
                notename = "Note-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }
            else
            {
                notename = notefilename.Text.Replace(".rfn", ""); // in case of *.rfn.rfn
            }
            StorageFile file = await LocalStorage.GetNoteFolderAsync().CreateFileAsync(notename + ".rfn", CreationCollisionOption.OpenIfExists);
            ExportAsInk(file);
            // record
            // note and paper shall be recorded dependently from general write, and how to deal with subfolder?
            FileList.DBInsertList(LocalStorage.GetNoteFolderAsync().Name, notename + ".rfn");
            FileList.DBInsertTrace(LocalStorage.GetNoteFolderAsync().Name, notename + ".rfn");

            ApplicationMessage.SendMessage(new ShortMessage { Title = "Note", Content = notename + " is saved", Time = DateTime.Now },
                ApplicationMessage.MessageType.Banner);
            foreach (string item in namelist)
            {
                if (item.Equals(notename))
                    return;
            }
            namelist.Add(notename);
        }

        private async void Delete_Note(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Delete application data?";
            dialog.PrimaryButtonText = "Yeah";
            dialog.CloseButtonText = "Forget it";
            dialog.DefaultButton = ContentDialogButton.Primary;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var name = notelist.SelectedItem as string;
                LocalStorage.GeneralDeleteAsync(LocalStorage.GetNoteFolderAsync(), name + ".rfn");
                namelist.Remove(name);
                notefilename.Text = "";
            }
        }

        #endregion

    }

    class PenCollection
    {
        public string Name { get; set; }

        public string Image { get; set; }

        public InkToolbarCustomPen Pen { get; set; }
    }
}
