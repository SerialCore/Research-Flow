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
using Windows.UI.Popups;
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
            if (e.Parameter != null)
            {
                if (e.Parameter.GetType().Equals(typeof(Feed)))
                {
                    Feed feed = e.Parameter as Feed;
                    paperid.Text = Feed.GetDoi(feed.Nodes);
                    papertitle.Text = feed.Title;
                    paperauthor.Text = Feed.GetAuthor(feed.Nodes);
                    paperlink.Content = feed.Link;
                    paperlink.NavigateUri = new Uri(feed.Link);
                    papertags.Text = feed.Tags;
                }
            }

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

        private Paper currentpaper = null;

        private void SavePaper(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(paperid.Text) || string.IsNullOrEmpty(papertitle.Text))
            {
                ApplicationMessage.SendMessage("PaperWarning: There must be Title and ID", ApplicationMessage.MessageType.InApp);
                return;
            }

            if (Paper.DBSelectByID(paperid.Text).Count == 0)
                Paper.DBInsert(new List<Paper>()
                {
                    new Paper
                    {
                        ID = paperid.Text,
                        ParentID = "Null",
                        Title = papertitle.Text,
                        FileName = string.IsNullOrEmpty(pdfname.Text)? "Null" : pdfname.Text,
                        Link = string.IsNullOrEmpty(paperlink.Content as string)? "Null" : (paperlink.Content as string),
                        Authors = string.IsNullOrEmpty(paperauthor.Text)? "Null" : paperauthor.Text,
                        Note = string.IsNullOrEmpty(papernote.Text)? "Null" : papernote.Text,
                        Tags = string.IsNullOrEmpty(papertags.Text)? "Null" : papertags.Text,
                    }
                });
        }

        private async void DeletePaper(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("You are about to delete application data, please tell me that is not true.", "Operation confirming");
            messageDialog.Commands.Add(new UICommand("True", new UICommandInvokedHandler(this.DeleteInvokedHandler)));
            messageDialog.Commands.Add(new UICommand("Joke", new UICommandInvokedHandler(this.CancelInvokedHandler)));

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync();
        }

        private async void DeleteInvokedHandler(IUICommand command)
        {
            if (currentpaper != null) // paper
            {
                Paper.DBDeleteByID(currentpaper.ID);
                //papers.Remove(currentpaper);
                currentpaper = null;
            }
            if (!string.IsNullOrEmpty(pdfname.Text)) // file
            {
                // whether to record?
                //LocalStorage.GeneralDeleteAsync(await LocalStorage.GetPaperFolderAsync(), pdfname.Text);
                await (await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(pdfname.Text)).DeleteAsync();
            }

            paperid.Text = "";
            papertitle.Text = "";
            paperauthor.Text = "";
            paperlink.Content = "Link";
            paperlink.NavigateUri = null;
            papertags.Text = "";
        }

        private void CancelInvokedHandler(IUICommand command) { }

        #endregion

    }
}
