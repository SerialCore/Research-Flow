using LogicService.Application;
using LogicService.Data;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
                    if (!papertitle.Text.Equals(feed.Title))
                    {
                        paperid.Text = Feed.GetID(feed.Nodes);
                        papertitle.Text = feed.Title;
                        paperdate.Text = feed.Published;
                        paperauthor.Text = Feed.GetAuthor(feed.Nodes);
                        paperlink.Text = feed.Link;
                        papertags.Text = feed.Tags;
                    }
                }
            }

            InitializePaper();
            InitializePdf();
        }

        private async void InitializePdf()
        {
            pdfs.Clear();
            var filelist = await (await LocalStorage.GetPaperFolderAsync()).GetFilesAsync();
            foreach (var file in filelist)
            {
                pdfs.Add(file.Name);
            }
            pdftree.ItemsSource = pdfs;
        }

        private void InitializePaper()
        {
            papers = Paper.DBSelectByLimit(100);
            paperlist.ItemsSource = papers;
        }

        #region List Operation

        private string currentfile = null; // the original name, must be faithfull // pdfname.Text can be new

        private ObservableCollection<string> pdfs = new ObservableCollection<string>();

        private void Pdftree_ItemClick(object sender, ItemClickEventArgs e)
        {
            var filename = e.ClickedItem as string;
            currentfile = filename;
            pdfname.Text = filename;

            // there is only one paper in list or nothing
            string nameid = "";
            foreach (PaperFile file in PaperFile.DBSelectByFile(filename))
            {
                nameid = file.ID;
            }
            foreach (Paper paper in Paper.DBSelectByID(nameid))
            {
                CleanPaperPanel();

                paperid.Text = paper.ID;
                papertitle.Text = paper.Title;
                paperauthor.Text = paper.Authors;
                paperdate.Text = paper.Published;
                paperlink.Text = paper.Link;
                papernote.Text = paper.Note;
                papertags.Text = paper.Tags;
                currentpaper = paper;
            }
        }

        private void PaperList_ItemClick(object sender, ItemClickEventArgs e)
        {
            CleanPaperPanel();

            currentpaper = e.ClickedItem as Paper;
            paperid.Text = currentpaper.ID;
            papertitle.Text = currentpaper.Title;
            paperauthor.Text = currentpaper.Authors;
            paperdate.Text = currentpaper.Published;
            paperlink.Text = currentpaper.Link;
            papernote.Text = currentpaper.Note;
            papertags.Text = currentpaper.Tags;
            pdfname.Text = "";

            foreach (PaperFile file in PaperFile.DBSelectByID(currentpaper.ID))
            {
                pdfname.Text = file.FileName;
            }
        }

        #endregion

        #region Pdf Operation

        private async void Paper_UnCompress(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".zip");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
                LocalStorage.UnCompression(file, await LocalStorage.GetPaperFolderAsync());
        }

        private async void Pdf_Import(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".pdf");
            var files = await picker.PickMultipleFilesAsync();
            if (files != null)
            {
                foreach (var file in files)
                {
                    await file.CopyAsync(await LocalStorage.GetPaperFolderAsync());
                    pdfs.Add(file.Name);
                }
            }
        }

        private async void Paper_Compress(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add(".zip");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
                LocalStorage.Compression(await LocalStorage.GetPaperFolderAsync(), folder);
        }

        private async void Pdf_Export(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentfile))
                return;

            try
            {
                var file = await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(currentfile);
                FolderPicker picker = new FolderPicker();
                picker.FileTypeFilter.Add(".pdf");
                StorageFolder folder = await picker.PickSingleFolderAsync();
                if (folder != null)
                    await file.CopyAsync(folder);
            }
            catch (Exception ex)
            {
                ApplicationMessage.SendMessage(new ShortMessage { Title = "PdfException", Content = ex.Message, Time = DateTimeOffset.Now },
                    ApplicationMessage.MessageType.InApp);
            }
        }

        private async void Pdf_Open(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentfile))
                return;

            //int newViewId = 0;
            //await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
            //    Frame frame = new Frame();
            //    frame.Navigate(typeof(PdfViewer), currentfile);
            //    Window.Current.Content = frame;
            //    // You have to activate the window in order to show it later.
            //    Window.Current.Activate();

            //    newViewId = ApplicationView.GetForCurrentView().Id;
            //});
            //await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
            await Launcher.LaunchFileAsync(await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(currentfile));
        }

        private void Pdf_Share(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentfile))
                return;

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

            try
            {
                StorageFile file = await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(currentfile);

                var storage = new List<IStorageItem>();
                storage.Add(file);
                request.Data.SetStorageItems(storage);
            }
            catch (Exception ex)
            {
                ApplicationMessage.SendMessage(new ShortMessage { Title = "PdfException", Content = ex.Message, Time = DateTimeOffset.Now }, ApplicationMessage.MessageType.InApp);
            }

            deferral.Complete();
        }

        private void Pdf_Download(object sender, RoutedEventArgs e)
        {

        }
        
        private void Pdf_Extract(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region Paper Management

        private Paper currentpaper = null;

        private List<Paper> papers = new List<Paper>();

        private void PaperLink_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(paperlink.Text))
                this.Frame.Navigate(typeof(SearchEngine), paperlink.Text);
        }

        private void CleanPaperPanel()
        {
            paperid.Text = "";
            papertitle.Text = "";
            paperauthor.Text = "";
            paperdate.Text = "";
            paperlink.Text = "";
            papernote.Text = "";
            papertags.Text = "";
        }

        private void SavePaper(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(paperid.Text) || string.IsNullOrEmpty(papertitle.Text))
            {
                ApplicationMessage.SendMessage(new ShortMessage { Title = "PaperWarning", Content = "There must be Title and ID", Time = DateTimeOffset.Now }, 
                    ApplicationMessage.MessageType.InApp);
                return;
            }

            if (Paper.DBSelectByID(paperid.Text).Count != 0)
                Paper.DBDeleteByID(paperid.Text); // modify or delete?
            Paper.DBInsert(new List<Paper>()
                {
                    new Paper
                    {
                        ID = paperid.Text,
                        ParentID = "",
                        Title = papertitle.Text,                       
                        Link = paperlink.Text,
                        Published = paperdate.Text,
                        Authors = paperauthor.Text,
                        Note = papernote.Text,
                        Tags = papertags.Text,
                    }
                });

            if (string.IsNullOrEmpty(pdfname.Text))
                PaperFile.DBDeleteByID(paperid.Text);
            else
            {
                if (PaperFile.DBSelectByID(paperid.Text).Count != 0) // select by id?
                    PaperFile.DBDeleteByID(paperid.Text); // modify or delete?
                PaperFile.DBInsert(new List<PaperFile>
                {
                    new PaperFile
                    {
                        ID = paperid.Text,
                        FileName = pdfname.Text,
                    }
                });
            }

            InitializePaper();
            if (!string.IsNullOrEmpty(papertags.Text))
                Topic.SaveTag(papertags.Text);
        }

        private async void SavePdfFile(object sender, RoutedEventArgs e)
        {
            if (currentfile != null) // file is selected
            {
                if (!currentfile.Equals(pdfname.Text)) // rename the file
                {
                    try
                    {
                        var file = await(await LocalStorage.GetPaperFolderAsync()).GetFileAsync(currentfile);
                        await file.RenameAsync(pdfname.Text); // whether to record
                        pdfs.Remove(currentfile);
                        pdfs.Add(pdfname.Text);
                        InitializePdf();
                    }
                    catch (Exception ex)
                    {
                        ApplicationMessage.SendMessage(new ShortMessage { Title = "PdfException", Content = ex.Message, Time = DateTimeOffset.Now },
                            ApplicationMessage.MessageType.InApp);
                    }
                }
            }
        }

        private async void DeletePaper(object sender, RoutedEventArgs e)
        {
            if (currentpaper == null && string.IsNullOrEmpty(currentfile))
                return;

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
                PaperFile.DBDeleteByID(currentpaper.ID);
                //papers.Remove(currentpaper); // modify list from database
                currentpaper = null;
            }
            if (!string.IsNullOrEmpty(currentfile)) // file // before the name is modified
            {
                // whether to record? if don't record, currentfile can be unfaithfull, and one must use try{}
                //LocalStorage.GeneralDeleteAsync(await LocalStorage.GetPaperFolderAsync(), pdfname.Text);
                try
                {
                    await (await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(currentfile)).DeleteAsync();
                    pdfs.Remove(currentfile);
                    currentfile = "";
                    pdfname.Text = "";
                }
                catch (Exception ex)
                {
                    ApplicationMessage.SendMessage(new ShortMessage { Title = "PdfException", Content = ex.Message, Time = DateTimeOffset.Now }, 
                        ApplicationMessage.MessageType.InApp);
                }
            }

            CleanPaperPanel();
        }

        private void CancelInvokedHandler(IUICommand command) { }

        #endregion

    }
}
