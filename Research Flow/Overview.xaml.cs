using LogicService.Application;
using LogicService.Data;
using LogicService.Storage;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Overview : Page
    {
        public Overview()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            InitializeMessage();
            InitializeFlow();
            ApplicationMessage.MessageReceived += AppMessage_MessageReceived;
        }

        private async void AppMessage_MessageReceived(string message, ApplicationMessage.MessageType type)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (type == ApplicationMessage.MessageType.Chat)
                {
                    IdentifyMessage(new ChatBlock { Comment = message, IsSelf = false, Published = DateTimeOffset.Now });
                }
            });
        }

        private async void InitializeMessage()
        {
            try
            {
                chatlist = await LocalStorage.ReadJsonAsync<ObservableCollection<ChatBlock>>(
                    await LocalStorage.GetDataFolderAsync(), "chatlist");
            }
            catch
            {
                // for new user, remember to load default feed from file, not the follows
                chatlist = new ObservableCollection<ChatBlock>()
                {
                    new ChatBlock { Comment = "Hello", IsSelf = false },
                    new ChatBlock { Comment = "for new user, remember to load default feed from file, not the follows", IsSelf = true },
                };
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "chatlist", chatlist);
            }
            finally
            {
                chatview.ItemsSource = chatlist;
            }
        }

        private void InitializeFlow()
        {

        }

        #region Message Process

        public ObservableCollection<ChatBlock> chatlist { get; set; }

        private void chatview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            chatscroll.ChangeView(null, double.MaxValue, null, true);
        }

        private async void IdentifyMessage(ChatBlock chat)
        {
            if (chat.Comment.Equals("0x01"))
                ApplicationSetting.IsDeveloper = "true";
            if (chat.Comment.Equals("0x02"))
                ApplicationSetting.RemoveKey("IsDeveloper");
            chatlist.Add(chat);

            LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "chatlist", chatlist);
        }

        private void Submit_Chat(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(chatbox.Text))
            {
                IdentifyMessage(new ChatBlock { Comment = chatbox.Text, IsSelf = true, Published = DateTimeOffset.Now });
                chatbox.Text = "";
            }
        }

        #endregion

    }
}
