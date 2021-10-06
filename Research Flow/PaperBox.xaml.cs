using LogicService.Application;
using LogicService.Data;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                        paperid.Text = ""; // TODO: crawer
                        papertitle.Text = feed.Title;
                        paperdate.Text = feed.Published;
                        paperauthor.Text = ""; // TODO: crawer
                        paperlink.Text = feed.Link;
                    }
                }
            }

            UpdatePaper();
        }

        private void UpdatePaper()
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
            pdfname.Text = "";

            foreach (PaperFile file in PaperFile.DBSelectByID(currentpaper.ID))
            {
                pdfname.Text = file.FileName;
            }
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
        }

        private void SavePaper(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(paperid.Text) || string.IsNullOrEmpty(papertitle.Text))
            {
                ApplicationMessage.SendMessage(new MessageEventArgs { Title = "PaperWarning", Content = "There must be Title and ID", Type = MessageType.InApp, Time = DateTimeOffset.Now });
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
                        Authors = paperauthor.Text
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

            UpdatePaper();
        }

        private async void SavePdfFile(object sender, RoutedEventArgs e)
        {
            if (currentfile != null) // file is selected
            {
                if (!currentfile.Equals(pdfname.Text)) // rename the file
                {
                    try
                    {
                        var file = await LocalStorage.GetPaperFolderAsync().GetFileAsync(currentfile);
                        await file.RenameAsync(pdfname.Text); // whether to record
                        pdfs.Remove(currentfile);
                        pdfs.Add(pdfname.Text);
                    }
                    catch (Exception ex)
                    {
                        ApplicationMessage.SendMessage(new MessageEventArgs { Title = "PdfException", Content = ex.Message, Type = MessageType.InApp, Time = DateTimeOffset.Now });
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
                    await (await LocalStorage.GetPaperFolderAsync().GetFileAsync(currentfile)).DeleteAsync();
                    pdfs.Remove(currentfile);
                    currentfile = "";
                    pdfname.Text = "";
                }
                catch (Exception ex)
                {
                    ApplicationMessage.SendMessage(new MessageEventArgs { Title = "PdfException", Content = ex.Message, Type = MessageType.InApp, Time = DateTimeOffset.Now });
                }
            }

            CleanPaperPanel();
        }

        private void CancelInvokedHandler(IUICommand command) { }

        #endregion

    }
}
