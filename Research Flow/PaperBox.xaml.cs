using LogicService.Application;
using LogicService.Data;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PaperBox : Page
    {
        public PaperBox()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InitializePaper();
        }

        private async void InitializePaper()
        {
            pdfs.Clear();
            var filelist = await (await LocalStorage.GetPaperFolderAsync()).GetFilesAsync();
            foreach (var file in filelist)
            {
                pdfs.Add(file.DisplayName.Replace(".pdf", ""));
            }
            pdftree.ItemsSource = pdfs;
        }

        #region File Management

        private ObservableCollection<string> pdfs = new ObservableCollection<string>();

        private void Pdf_List(object sender, RoutedEventArgs e) => pdfpanel.IsPaneOpen = !pdfpanel.IsPaneOpen;

        private void Pdftree_ItemClick(object sender, ItemClickEventArgs e)
        {
            var filename = e.ClickedItem as string + ".pdf";
            pdfname.Text = filename;

            // there is only one paper in list or nothing
            foreach (Paper paper in Paper.DBSelectByFile(filename))
            {
                paperid.Text = paper.ID;
                papertitle.Text = paper.Title;
                paperauthor.Text = paper.Authors;
                paperlink.Content = paper.Link;
                paperlink.NavigateUri = new Uri(paper.Link);
                papernote.Text = paper.Note;
                papertags.Text = paper.Tags;
            }
        }

        #endregion

        #region Pdf Operation

        private ObservableCollection<BitmapImage> pages = new ObservableCollection<BitmapImage>();

        PdfDocument pdfDocument = null;

        private async void Pdf_Preview(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(pdfname.Text))
                return;

            try
            {
                StorageFile file = await(await LocalStorage.GetPaperFolderAsync()).GetFileAsync(pdfname.Text);
                pdfDocument = await PdfDocument.LoadFromFileAsync(file);
            }
            catch (Exception exception)
            {
                ApplicationMessage.SendMessage("PdfException: " + exception.Message, ApplicationMessage.MessageType.InApp);
            }

            if (pdfDocument != null)
            {
                uint pageCount = pdfDocument.PageCount > 10 ? 10 : pdfDocument.PageCount;
                pagecount.Text = pageCount.ToString();
                pageindex.Text = "1";

                pages.Clear();
                for (uint p = 0; p < pageCount; p++)
                {
                    using (PdfPage page = pdfDocument.GetPage(p))
                    {
                        await page.PreparePageAsync();

                        InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
                        await page.RenderToStreamAsync(stream);

                        BitmapImage bitmap = new BitmapImage();
                        bitmap.DecodePixelWidth = 1360;
                        pages.Add(bitmap);
                        await bitmap.SetSourceAsync(stream);
                    }
                }
                pdfPages.ItemsSource = pages;
            }
        }

        private async void Pdf_Launch(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(pdfname.Text))
                return;

            try
            {
                StorageFile file = await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(pdfname.Text);
                await Launcher.LaunchFileAsync(file);
            }
            catch (Exception exception)
            {
                ApplicationMessage.SendMessage("PdfException: " + exception.Message, ApplicationMessage.MessageType.InApp);
            }
        }

        private void Pdf_Share(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += FileTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private async void FileTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequestDeferral deferral = args.Request.GetDeferral();

            DataRequest request = args.Request;
            request.Data.Properties.Title = "Pdf File";
            request.Data.Properties.Description = "Share the paper you found";

            StorageFile file = await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(pdfname.Text);

            var storage = new List<IStorageItem>();
            storage.Add(file);
            request.Data.SetStorageItems(storage);

            deferral.Complete();
        }

        private void PageIndex_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                pdfPages.SelectedIndex = Convert.ToInt32(pageindex.Text) - 1;
        }

        private void PdfPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pageindex.Text = (pdfPages.SelectedIndex + 1).ToString();
        }

        #endregion

        #region Paper Management

        /// <summary>
        /// will be recorded
        /// </summary>
        private void ArchiveThisPaper()
        {
            // if user want to sync this paper between devices, they will have the right to make the choice
            // choosing synchronization is equals to choosing registing on database
        }

        #endregion

    }
}
