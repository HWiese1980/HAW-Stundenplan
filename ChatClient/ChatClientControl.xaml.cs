using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MetaBuilders.Irc.Messages;
using SeveQsCustomControls;
using MetaBuilders.Irc;
using System.Windows.Data;

namespace ChatClient
{
    /// <summary>
    /// Interaktionslogik für ChatClientControl.xaml
    /// </summary>
    public partial class ChatClientControl
    {
        private Paragraph _chatParagraph = new Paragraph();
        private string _channel;

        public bool IsConnected
        {
            get { return (bool)GetValue(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsConnected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsConnectedProperty =
            DependencyProperty.Register("IsConnected", typeof(bool), typeof(ChatClientControl), new UIPropertyMetadata(false));

        public void SendChatMessage(string channel, string format, params object[] parameters)
        {
            Dispatcher.Invoke(new Action(() =>
                                             {
                                                 if (IsConnected)
                                                     _chatClient.SendChat(string.Format(format, parameters),
                                                                          (string.IsNullOrEmpty(channel)
                                                                               ? _channel
                                                                               : channel));
                                             }));
        }

        public ChatClientControl()
        {
            InitializeComponent();
            ThreadSafeObservableCollection<User>.UIDispatcher = Dispatcher;

            _chatClient = new Client("haw.seveq.de", string.Format("HAW-Tool-{0}", Guid.NewGuid().ToString())) { EnableAutoIdent = false };

            _chatClient.Connection.Connected += Connection_Connected;
            _chatClient.Messages.Welcome += Messages_Welcome;
            _chatClient.Messages.Chat += Messages_Chat;
            _chatClient.Messages.TimeRequest += Messages_TimeRequest;
            _chatClient.Messages.UserNotification += Messages_UserNotification;
            _chatClient.DataReceived += _chatClient_DataReceived;
            _chatClient.Connection.Disconnected += Connection_Disconnected;

            var bUserList = new Binding { Source = _chatClient.Peers };
            peerList.SetBinding(ItemsControl.ItemsSourceProperty, bUserList);

            _channel = Channel;

            messageWindow.Document.Blocks.Add(_chatParagraph);
        }

        void _chatClient_DataReceived(object sender, MetaBuilders.Irc.Network.ConnectionDataEventArgs e)
        {
            Console.WriteLine(@"IRC Data received: {0}", e.Data);
        }

        void Messages_UserNotification(object sender, IrcMessageEventArgs<UserNotificationMessage> e)
        {
            WriteLine(e.Message.Sender.Nick, e.ToString());
        }

        void Connection_Connected(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() => IsConnected = true));
        }

        void WriteLine(string user, string format, params object[] parameters)
        {
            string msg = string.Format(format, parameters);
            Dispatcher.Invoke(new Action<string, string>(AddMessageAction), user, msg);
        }

        private void AddMessageAction(string user, string msg)
        {
            _chatParagraph.Inlines.Add(new Run(user) { Foreground = Brushes.Green, FontWeight = FontWeights.Bold });
            _chatParagraph.Inlines.Add(new Run(": "));
            _chatParagraph.Inlines.Add(new Run(msg));
            _chatParagraph.Inlines.Add(new LineBreak());
        }

        void Connection_Disconnected(object sender, MetaBuilders.Irc.Network.ConnectionDataEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => IsConnected = false));
            WriteLine("Info", "Disconnected");
        }

        void Messages_TimeRequest(object sender, IrcMessageEventArgs<TimeRequestMessage> e)
        {
            var reply = new TimeReplyMessage { CurrentTime = DateTime.Now.ToLongTimeString(), Target = e.Message.Sender.Nick };
            _chatClient.Send(reply);
        }

        void Messages_Chat(object sender, IrcMessageEventArgs<TextMessage> e)
        {
            WriteLine(e.Message.Sender.Nick, e.Message.Text);
        }

        void Messages_Welcome(object sender, IrcMessageEventArgs<WelcomeMessage> e)
        {
            _chatClient.SendJoin(_channel);
        }


        private Client _chatClient;

        public string UserName
        {
            get { return (string)GetValue(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register("UserName", typeof(string), typeof(ChatClientControl), new UIPropertyMetadata("", NameChanged));

        private static void NameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ccc = (ChatClientControl)d;
            ccc._chatClient.User.Nick = (string)e.NewValue;
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _chatClient.Connection.Connect();
            }
            catch (Exception exp)
            {
                WriteLine("Exception: {0}", exp.Message);
            }
        }

        public string Channel
        {
            get { return (string)GetValue(ChannelProperty); }
            set { SetValue(ChannelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Channel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChannelProperty =
            DependencyProperty.Register("Channel", typeof(string), typeof(ChatClientControl), new UIPropertyMetadata("#ToolBase", ChannelChanged, CoerceChannelName));

        private static object CoerceChannelName(DependencyObject d, object basevalue)
        {
            var channelName = (string)basevalue;
            if (!channelName.StartsWith("#")) return "#" + channelName;
            return channelName;
        }

        private static void ChannelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ccc = (ChatClientControl)d;
            var cnt = ccc._chatClient;
            ccc._channel = (string)e.NewValue;
            cnt.SendJoin(ccc._channel);
        }


        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            _chatClient.SendChat(messageBox.Text, _channel);
            WriteLine(UserName, messageBox.Text);
            messageBox.Text = "";
        }

        private void messageBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendButton_Click(sender, null);
        }
    }
}
