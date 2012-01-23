using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using SeveQsCustomControls;
using HAW_Tool.HAW.Depending;
using System.Windows.Data;

namespace HAW_Tool.HAW.ChatClient
{
    [TemplatePart(Name = "PART_UserList", Type= typeof(Selector))]
    [TemplatePart(Name = "PART_ChatLog", Type = typeof(RichTextBox))]
    public class ChatRoom : Control
    {
        public ChatRoom()
        {
            Users = new ThreadSafeObservableCollection<string>();
        }

        public string ChannelName
        {
            get { return (string)GetValue(ChannelNameProperty); }
            set { SetValue(ChannelNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChannelNameProperty =
            DependencyProperty.Register("ChannelName", typeof(string), typeof(ChatRoom), new UIPropertyMetadata(""));

        private RichTextBox _chatLog;
        private Paragraph _chatLogPgr;

        private Selector _chatUserList;

        public override void OnApplyTemplate()
        {
            _chatLogPgr = new Paragraph();

            _chatLog = GetTemplateChild("PART_ChatLog") as RichTextBox;
            _chatLog.Document = new FlowDocument();
            _chatLog.Document.Blocks.Add(_chatLogPgr);

            _chatUserList = GetTemplateChild("PART_UserList") as Selector;

            _chatUserList.SetBinding(ItemsControl.ItemsSourceProperty, new Binding {Source = Users});

            base.OnApplyTemplate();
        }

        private ThreadSafeObservableCollection<string> Users { get; set; }


        public void WriteChatMessage(string user, string message)
        {
            WriteFormatted(FontWeights.Bold, Brushes.CadetBlue, Brushes.Transparent, "{0} : ", user);
            WriteFormattedLine(FontWeights.Normal, Brushes.CadetBlue, Brushes.Transparent, message);
        }

        private void WriteFormattedLine(FontWeight weight, Brush foreground, Brush background, string format, params object[] parameters)
        {
            WriteFormatted(weight, foreground, background, format, parameters);
            WriteNewLine();
        }

        private void WriteNewLine()
        {
            Dispatcher.Invoke(new Action(() => _chatLogPgr.Inlines.Add(new LineBreak())));
        }

        private void WriteFormatted(FontWeight weight, Brush foreground, Brush background, string format, params object[] parameters)
        {
            Dispatcher.Invoke(new Action(() =>
                                             {
                                                 var r = new Run(string.Format(format, parameters)) { Foreground = foreground, Background = background };
                                                 _chatLogPgr.Inlines.Add(r);
                                             }));
        }
    }
}
