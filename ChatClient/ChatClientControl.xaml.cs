using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Meebey.SmartIrc4net;
using MetaBuilders.Irc;
using MetaBuilders.Irc.Messages;
using MetaBuilders.Irc.Network;
using SeveQsCustomControls;

namespace ChatClient
{
    /// <summary>
    /// Interaktionslogik für ChatClientControl.xaml
    /// </summary>
    public partial class ChatClientControl
    {
        private readonly Paragraph _chatParagraph = new Paragraph();
        private string _channel;

        #region Nebo

        private Client _neboClient;

        public void NeboSendChatMessage(string channel, string format, params object[] parameters)
        {
            Dispatcher.Invoke(new Action(() =>
                                             {
                                                 if (IsConnected)
                                                     _neboClient.SendChat(string.Format(format, parameters),
                                                                          (string.IsNullOrEmpty(channel)
                                                                               ? _channel
                                                                               : channel));
                                             }));
        }

        private void NeboInitialize()
        {
            _neboClient = new Client("haw.seveq.de", string.Format("HAW-Tool-{0}", Guid.NewGuid().ToString()))
                              {EnableAutoIdent = false};

            _neboClient.Connection.Connected += Connection_Connected;
            _neboClient.Messages.Welcome += Messages_Welcome;
            _neboClient.Messages.Chat += Messages_Chat;
            _neboClient.Messages.TimeRequest += Messages_TimeRequest;
            _neboClient.Messages.UserNotification += Messages_UserNotification;
            _neboClient.DataReceived += NeboClientDataReceived;
            _neboClient.Connection.Disconnected += Connection_Disconnected;
            _neboClient.MessageParsed += NeboClientMessageParsed;

            var bUserList = new Binding {Source = _neboClient.Peers};
            peerList.SetBinding(ItemsControl.ItemsSourceProperty, bUserList);
        }

        private void NeboClientMessageParsed(object sender, IrcMessageEventArgs<IrcMessage> e)
        {
            WriteInfo(Brushes.DarkGray, "Server: {0}", e.Message);
        }

        private void NeboClientDataReceived(object sender, ConnectionDataEventArgs e)
        {
            Console.WriteLine(@"IRC Data received: {0}", e.Data);
        }

        private void Messages_UserNotification(object sender, IrcMessageEventArgs<UserNotificationMessage> e)
        {
            WriteLine(e.Message.Sender.Nick, e.ToString());
        }

        private void Connection_Connected(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() => IsConnected = true));
        }

        private void Connection_Disconnected(object sender, ConnectionDataEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => IsConnected = false));
            WriteLine("Info", "Disconnected");
        }

        private void Messages_TimeRequest(object sender, IrcMessageEventArgs<TimeRequestMessage> e)
        {
            var reply = new TimeReplyMessage
                            {CurrentTime = DateTime.Now.ToLongTimeString(), Target = e.Message.Sender.Nick};
            _neboClient.Send(reply);
        }

        private void Messages_Chat(object sender, IrcMessageEventArgs<TextMessage> e)
        {
            WriteLine(e.Message.Sender.Nick, e.Message.Text);
        }

        private void Messages_Welcome(object sender, IrcMessageEventArgs<WelcomeMessage> e)
        {
            _neboClient.SendJoin(_channel);
        }

        private void NeboSetUsername(string username)
        {
            _neboClient.User.Nick = username;
        }

        private void NeboConnect()
        {
            _neboClient.Connection.Connect();
        }

        private void NeboJoinChan(string chan)
        {
            _channel = chan;
            Client cnt = _neboClient;
            if (cnt.Connection.Status == ConnectionStatus.Connected) cnt.SendJoin(_channel);
        }

        private void NeboSendChatMessage(string msg)
        {
            _neboClient.SendChat(msg, _channel);
        }

        #endregion

        #region Meebey

        private readonly ThreadSafeObservableCollection<string> _meebeyChatUsers =
            new ThreadSafeObservableCollection<string>();

        private IrcClient _meebeyClient;

        private void MeebeyInitialize()
        {
            ThreadSafeObservableCollection<string>.UIDispatcher = Dispatcher;

            _meebeyClient = new IrcClient {SendDelay = 400, ActiveChannelSyncing = true};

            _meebeyClient.OnQueryMessage += _meebeyClient_OnQueryMessage;
            _meebeyClient.OnError += _meebeyClient_OnError;
            _meebeyClient.OnJoin += _meebeyClient_OnJoin;
            _meebeyClient.OnAway += _meebeyClient_OnAway;
            _meebeyClient.OnNowAway += _meebeyClient_OnNowAway;
            _meebeyClient.OnQuit += _meebeyClient_OnQuit;
            _meebeyClient.OnUnAway += _meebeyClient_OnUnAway;
            // _meebeyClient.OnRawMessage += new IrcEventHandler(_meebeyClient_OnRawMessage);
            _meebeyClient.OnConnected += _meebyClient_OnConnected;
            _meebeyClient.OnChannelMessage += _meebyClient_OnChannelMessage;
            _meebeyClient.OnMotd += _meebyClient_OnMotd;
            _meebeyClient.OnChannelAction += _meebyClient_OnChannelAction;
            _meebeyClient.OnDisconnected += _meebyClient_OnDisconnected;
            _meebeyClient.OnNames += _meebyClient_OnNames;

            var bUserList = new Binding {Source = _meebeyChatUsers};
            peerList.SetBinding(ItemsControl.ItemsSourceProperty, bUserList);
        }

        private void _meebeyClient_OnUnAway(object sender, IrcEventArgs e)
        {
            MeebeyUserStateMessage(e.Data.Nick, "ist nicht mehr away");
        }

        private void _meebeyClient_OnQuit(object sender, QuitEventArgs e)
        {
            MeebeyUserStateMessage(e.Who, "hat uns verlassen. Nachricht: {0}", e.QuitMessage);
        }

        private void _meebeyClient_OnNowAway(object sender, IrcEventArgs e)
        {
            MeebeyUserStateMessage(e.Data.Nick, "ist jetzt away. Nachricht: {0}", e.Data.Message);
        }

        private void _meebeyClient_OnAway(object sender, AwayEventArgs e)
        {
            MeebeyUserStateMessage(e.Who, "ist away. Nachricht: {0}", e.AwayMessage);
        }

        private void _meebeyClient_OnJoin(object sender, JoinEventArgs e)
        {
            MeebeyUserStateMessage(e.Who, "hat den Kanal {0} betreten", e.Channel);
        }

        private void MeebeyUserStateMessage(string Who, string format, params object[] parameters)
        {
            WriteInfo(Brushes.DodgerBlue, "{0} {1}", Who, string.Format(format, parameters));
        }

        private void _meebeyClient_OnQueryMessage(object sender, IrcEventArgs e)
        {
            if (e.Data.MessageArray != null)
                foreach (string msg in e.Data.MessageArray)
                {
                    WriteInfo(Brushes.DarkViolet, "Query: {0}", msg);
                }
            else
                WriteInfo(Brushes.BlueViolet, "Query: {0}", e.Data.Message);
        }

        private void _meebeyClient_OnError(object sender, ErrorEventArgs e)
        {
            WriteInfo(Brushes.Red, "Error {0}", e.ErrorMessage);
        }

        private void _meebeyClient_OnRawMessage(object sender, IrcEventArgs e)
        {
            WriteInfo(Brushes.Gray, "Meebey Raw Message: {0}", e.Data.RawMessage);
        }

        private void MeebeyConnect()
        {
            _meebeyClient.Connect("haw.seveq.de", 6667);
            _meebeyClient.Login(UserName, "HAW-Client-" + Guid.NewGuid().ToString());
            MeebeyJoinChan(_channel);
            MeebeyJoinChan("#Aenderungen");
            new Task(() => _meebeyClient.Listen(true)).Start();
        }

        private void _meebyClient_OnNames(object sender, NamesEventArgs e)
        {
            _meebeyChatUsers.Clear();
            WriteInfo(Brushes.CornflowerBlue, "Auch diesem Kanal:");
            foreach (string name in e.UserList)
            {
                WriteInfo(Brushes.CornflowerBlue, name);
                _meebeyChatUsers.Add(name);
            }
            WriteInfo(Brushes.CornflowerBlue, "------------------------");
        }

        private void _meebyClient_OnDisconnected(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() => IsConnected = false));
            WriteInfo(Brushes.DarkRed, "Verbindung beendet!");
        }

        private void _meebyClient_OnChannelAction(object sender, ActionEventArgs e)
        {
            WriteInfo(Brushes.Sienna, "{0} <-- {1}", e.Data.Nick, e.ActionMessage);
        }

        private void _meebyClient_OnMotd(object sender, MotdEventArgs e)
        {
            WriteInfo(Brushes.DarkGreen, "{0}", e.MotdMessage);
        }

        private void _meebyClient_OnChannelMessage(object sender, IrcEventArgs e)
        {
            // WriteInfo(Brushes.CornflowerBlue, "Channel Message: {0}", e.Data.RawMessage);
            WriteLine(e.Data.Nick, e.Data.Message);
        }

        private void _meebyClient_OnConnected(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() => IsConnected = true));
            WriteInfo(Brushes.DarkGreen, "Verbindung hergestellt.");
        }

        private void MeebeySendChatMessage(string text)
        {
            _meebeyClient.SendMessage(SendType.Message, _channel, text);
        }

        private void MeebeyJoinChan(string chan)
        {
            if (_meebeyClient.IsConnected)
            {
                _meebeyClient.RfcJoin(chan);
            }
            _channel = chan;
        }

        #endregion

        #region Constructor

        public ChatClientControl()
        {
            InitializeComponent();
            ThreadSafeObservableCollection<User>.UIDispatcher = Dispatcher;

            // NeboInitialize();
            MeebeyInitialize();

            _channel = Channel;

            messageWindow.Document.Blocks.Add(_chatParagraph);
        }

        #endregion

        #region Important Properties

        public static readonly DependencyProperty IsConnectedProperty =
            DependencyProperty.Register("IsConnected", typeof (bool), typeof (ChatClientControl),
                                        new UIPropertyMetadata(false));

        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register("UserName", typeof (string), typeof (ChatClientControl),
                                        new UIPropertyMetadata("", NameChanged));

        public static readonly DependencyProperty ChannelProperty =
            DependencyProperty.Register("Channel", typeof (string), typeof (ChatClientControl),
                                        new UIPropertyMetadata("#ToolBase", ChannelChanged, CoerceChannelName));

        public bool IsConnected
        {
            get { return (bool) GetValue(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }

        public string UserName
        {
            get { return (string) GetValue(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }

        public string Channel
        {
            get { return (string) GetValue(ChannelProperty); }
            set { SetValue(ChannelProperty, value); }
        }

        private static void NameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ccc = (ChatClientControl) d;
            var username = (string) e.NewValue;
            // ccc.NeboSetUsername(username);
        }

        private static object CoerceChannelName(DependencyObject d, object basevalue)
        {
            var channelName = (string) basevalue;
            if (!channelName.StartsWith("#")) return "#" + channelName;
            return channelName;
        }

        private static void ChannelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ccc = (ChatClientControl) d;
            var chan = (string) e.NewValue;
            // ccc.NeboJoinChan(chan);
            ccc.MeebeyJoinChan(chan);
        }

        #endregion

        #region GUI Message Methods

        private void WriteLine(string user, string format, params object[] parameters)
        {
            string msg = string.Format(format, parameters);
            Dispatcher.Invoke(new Action<string, string>(AddMessageAction), user, msg);
        }

        private void WriteInfo(Brush foreground, string format, params object[] parameters)
        {
            string msg = string.Format(format, parameters);
            Dispatcher.Invoke(new Action<string, Brush>(AddInfoAction), msg, foreground);
        }

        private void AddInfoAction(string msg, Brush foreground = null)
        {
            _chatParagraph.Inlines.Add(new Run(msg)
                                           {
                                               Foreground = foreground,
                                               FontWeight = FontWeights.Bold,
                                               FontStyle = FontStyles.Italic
                                           });
            _chatParagraph.Inlines.Add(new LineBreak());
        }

        private void AddMessageAction(string user, string msg)
        {
            _chatParagraph.Inlines.Add(new Run(user) {Foreground = Brushes.Green, FontWeight = FontWeights.Bold});
            _chatParagraph.Inlines.Add(new Run(": "));
            _chatParagraph.Inlines.Add(new Run(msg));
            _chatParagraph.Inlines.Add(new LineBreak());
        }

        #endregion

        #region GUI Event Handlers

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // NeboConnect();
                MeebeyConnect();
            }
            catch (Exception exp)
            {
                WriteLine("Exception: {0}", exp.Message);
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // NeboSendChatMessage(messageBox.Text);
            MeebeySendChatMessage(messageBox.Text);
            WriteLine(UserName, messageBox.Text);
            messageBox.Text = "";
        }

        private void messageBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendButton_Click(sender, null);
        }

        #endregion
    }
}