using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Meebey.SmartIrc4net;
using SeveQsCustomControls;

namespace HAW_Tool.HAW.ChatClient
{
    /// <summary>
    /// Interaktionslogik für ChatClientControl.xaml
    /// </summary>
    public partial class ChatClientControl
    {
        private readonly Paragraph _chatParagraph = new Paragraph();
        private string _channel;

        #region Meebey

        private readonly ThreadSafeObservableCollection<string> _meebeyChatUsers =
            new ThreadSafeObservableCollection<string>();

        private IrcClient _meebeyClient;

        private void MeebeyInitialize()
        {
            ThreadSafeObservableCollection<string>.UIDispatcher = Dispatcher;
            Channels = new ThreadSafeObservableCollection<string>();

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

        private void MeebeyUserStateMessage(string who, string format, params object[] parameters)
        {
            WriteInfo(Brushes.DodgerBlue, "{0} {1}", who, string.Format(format, parameters));
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

        private void MeebeyConnect()
        {
            _meebeyClient.Connect("haw.seveq.de", 6667);
            _meebeyClient.Login(UserName, "HAW-Client-" + Guid.NewGuid().ToString());
            MeebeyJoinChan(_channel);
            new Task(() => _meebeyClient.Listen(true)).Start();
        }

        private void _meebyClient_OnNames(object sender, NamesEventArgs e)
        {
            _meebeyChatUsers.Clear();
            WriteInfo(Brushes.CornflowerBlue, "Auch diesem Kanal:");
            foreach (string name in e.UserList)
            {
                if (name == "") continue;
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
            var tab = GetChatWindow(e.Data.Channel);
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
                Channels.Add(chan);
                _meebeyClient.RfcJoin(chan);
            }
            _channel = chan;
        }

        #endregion

        #region Constructor

        public ChatClientControl()
        {
            InitializeComponent();
            MeebeyInitialize();
            MeebeyAddVirtualChatRoom("#Server");
            _channel = Channel;

            // messageWindow.Document.Blocks.Add(_chatParagraph);
        }

        private void MeebeyAddVirtualChatRoom(string name)
        {
            Channels.Add(name);
        }

        private RichTextBox GetChatWindow(string channel)
        {
            return null;
        }

        public ThreadSafeObservableCollection<string> Channels { get; set; }

        #endregion

        #region Important Properties

        public static readonly DependencyProperty IsConnectedProperty =
            DependencyProperty.Register("IsConnected", typeof (bool), typeof (ChatClientControl),
                                        new UIPropertyMetadata(false));

        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register("UserName", typeof (string), typeof (ChatClientControl),
                                        new UIPropertyMetadata(""));

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