using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XIVChatCommon.Message;
using XIVChatCommon.Message.Server;

namespace XIVChat_Desktop {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged {
        #region commands

        private void AlwaysTrue_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        public static readonly RoutedUICommand EditTab = new RoutedUICommand(
            "EditTab",
            "EditTab",
            typeof(MainWindow)
        );

        private void EditTab_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (!(e.Parameter is Tab tab)) {
                return;
            }

            new ManageTab(this, tab).Show();
        }

        public static readonly RoutedUICommand DeleteTab = new RoutedUICommand(
            "DeleteTab",
            "DeleteTab",
            typeof(MainWindow)
        );

        private void DeleteTab_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (!(e.Parameter is Tab tab)) {
                return;
            }

            this.App.Config.Tabs.Remove(tab);
            this.App.Config.Save();
        }

        public static readonly RoutedUICommand AddTab = new RoutedUICommand(
            "AddTab",
            "AddTab",
            typeof(MainWindow)
        );

        private void AddTab_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            new ManageTab(this, null).Show();
        }

        public static readonly RoutedUICommand ManageTabs = new RoutedUICommand(
            "ManageTabs",
            "ManageTabs",
            typeof(MainWindow)
        );

        private void ManageTabs_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            new ManageTabs(this).Show();
        }

        public static readonly RoutedUICommand MessageSendTell = new RoutedUICommand(
            "MessageSendTell",
            "MessageSendTell",
            typeof(MainWindow)
        );

        private void MessageSendTell_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            if (!(e.Parameter is ServerMessage message)) {
                return;
            }

            e.CanExecute = message.GetSenderPlayer() != null;
        }

        private void MessageSendTell_OnExecuted(object eventSender, ExecutedRoutedEventArgs e) {
            if (!(e.Parameter is ServerMessage message)) {
                return;
            }

            var sender = message.GetSenderPlayer();
            if (sender == null) {
                return;
            }

            var worldName = Util.WorldName(sender.Server);
            if (worldName == null) {
                return;
            }

            this.InsertTellCommand(sender.Name, worldName);
        }


        public static readonly RoutedUICommand ChangeChannel = new RoutedUICommand(
            "ChangeChannel",
            "ChangeChannel",
            typeof(MainWindow)
        );

        private void ChangeChannel_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = this.App.Connected;
        }

        private void ChangeChannel_Execute(object sender, ExecutedRoutedEventArgs e) {
            if (!(e.Parameter is InputChannel)) {
                return;
            }

            var param = (InputChannel) e.Parameter;
            this.App.Connection?.ChangeChannel(param);
        }

        #endregion

        public App App => (App) Application.Current;

        public List<ServerMessage> Messages { get; } = new List<ServerMessage>();
        public ObservableCollection<Player> FriendList { get; } = new ObservableCollection<Player>();

        private int historyIndex = -1;

        private int HistoryIndex {
            get => this.historyIndex;
            set {
                var idx = Math.Min(this.History.Count - 1, Math.Max(-1, value));
                this.historyIndex = idx;
            }
        }

        private int ReverseHistoryIndex => this.HistoryIndex == -1 ? -1 : Math.Max(-1, this.History.Count - this.HistoryIndex - 1);

        private string? HistoryBuffer { get; set; }

        private List<string> History { get; } = new List<string>();

        public string InputPlaceholder => this.App.Connection?.Available == true ? "Typing words…" : "聊天当前不可用";

        public MainWindow() {
            this.InitializeComponent();
            this.DataContext = this;
        }

        private T? FindElementByName<T>(DependencyObject element, string sChildName) where T : FrameworkElement {
            T? childElement = null;
            var nChildCount = VisualTreeHelper.GetChildrenCount(element);
            for (var i = 0; i < nChildCount; i++) {
                if (!(VisualTreeHelper.GetChild(element, i) is FrameworkElement child)) {
                    continue;
                }

                if (child is T t && child.Name.Equals(sChildName)) {
                    childElement = t;
                    break;
                }

                childElement = this.FindElementByName<T>(child, sChildName);

                if (childElement != null) {
                    break;
                }
            }

            return childElement;
        }

        public void ClearAllMessages() {
            this.Messages.Clear();
            foreach (var tab in this.App.Config.Tabs) {
                tab.ClearMessages();
            }
        }

        public void AddSystemMessage(string content) {
            var message = new ServerMessage(
                DateTime.UtcNow,
                0,
                new byte[0],
                Encoding.UTF8.GetBytes(content),
                new List<Chunk> {
                    new TextChunk(content) {
                        Foreground = 0xb38cffff,
                    },
                }
            );
            this.AddMessage(message);
        }

        private int lastSequence = -1;
        private int insertAt;

        public void AddReversedChunk(ServerMessage[] messages, int sequence) {
            if (sequence != this.lastSequence) {
                this.lastSequence = sequence;
                this.insertAt = this.Messages.Count;
            }

            // detect if scroller is at the bottom
            var scroller = this.FindElementByName<ScrollViewer>(this.Tabs, "scroller");
            var scrollerOffset = scroller!.VerticalOffset;
            var scrollerHeight = scroller.ScrollableHeight;
            var wasAtBottom = Math.Abs(scroller.VerticalOffset - scrollerHeight) < .0001;

            // add messages to main list
            this.Messages.InsertRange(this.insertAt, messages);
            // add message to each tab if the filter allows for it
            foreach (var tab in this.App.Config.Tabs) {
                tab.AddReversedChunk(messages, sequence, this.App.Config);
            }

            var diff = this.Messages.Count - this.App.Config.LocalBacklogMessages;
            if (diff > 0) {
                this.Messages.RemoveRange(0, (int) diff);
            }

            // scroll to the bottom if previously at the bottom
            if (wasAtBottom) {
                scroller.ScrollToBottom();
            } else {
                scroller.UpdateLayout();
                var scrollDiff = scroller.ScrollableHeight - scrollerHeight;
                scroller.ScrollToVerticalOffset(scrollerOffset + scrollDiff);
            }
        }

        public void AddMessage(ServerMessage message) {
            // detect if scroller is at the bottom
            var scroller = this.FindElementByName<ScrollViewer>(this.Tabs, "scroller");
            var verticalOffset = scroller!.VerticalOffset;
            var wasAtBottom = Math.Abs(verticalOffset - scroller.ScrollableHeight) < .0001;

            // add message to main list
            this.Messages.Add(message);
            // add message to each tab if the filter allows for it
            foreach (var tab in this.App.Config.Tabs) {
                tab.AddMessage(message, this.App.Config);
            }

            var diff = this.Messages.Count - this.App.Config.LocalBacklogMessages;
            if (diff > 0) {
                this.Messages.RemoveRange(0, (int) diff);
            }

            // scroll to the bottom if previously at the bottom
            if (wasAtBottom) {
                scroller.ScrollToBottom();
            } else {
                scroller.ScrollToVerticalOffset(verticalOffset);
            }
        }

        public void InsertTellCommand(string name, string world, bool focus = true) {
            var input = this.App.Window.GetCurrentInputBox();
            if (input == null) {
                return;
            }

            var tell = $"/tell {name}@{world} ";

            input.Text = input.Text.Insert(0, tell);
            input.SelectionStart = tell.Length;
            input.SelectionLength = input.Text.Length - tell.Length;

            if (focus) {
                input.Focus();
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e) {
            new ConnectDialog(this).ShowDialog();
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e) {
            this.App.Disconnect();
        }

        private void Input_Submit(object sender, KeyEventArgs e) {
            if (!(sender is TextBox textBox)) {
                return;
            }

            switch (e.Key) {
                case Key.Return:
                    this.Submit(textBox);
                    break;
                case Key.Up:
                    this.ArrowNavigate(textBox, true);
                    break;
                case Key.Down:
                    this.ArrowNavigate(textBox, false);
                    break;
            }
        }

        private void Submit(TextBox textBox) {
            var conn = this.App.Connection;
            if (conn == null) {
                return;
            }

            conn.SendMessage(textBox.Text);
            this.History.Add(textBox.Text);
            while (this.History.Count > 100) {
                this.History.RemoveAt(0);
            }

            textBox.Text = "";
        }

        private void ArrowNavigate(TextBox textBox, bool up) {
            if (this.History.Count == 0) {
                return;
            }

            var caretLine = textBox.GetLineIndexFromCharacterIndex(textBox.CaretIndex);
            var inFirstLine = caretLine == 0;
            var inLastLine = caretLine == textBox.LineCount - 1;

            if (this.HistoryIndex == -1) {
                this.HistoryBuffer = textBox.Text;
            }

            if (up && inFirstLine) {
                // go up in history
                this.HistoryIndex += 1;
                textBox.Text = this.History[this.ReverseHistoryIndex];
            } else if (!up && inLastLine) {
                // go down in history
                this.HistoryIndex -= 1;

                if (this.HistoryIndex == -1) {
                    textBox.Text = this.HistoryBuffer;
                    this.HistoryBuffer = null;
                } else {
                    textBox.Text = this.History[this.ReverseHistoryIndex];
                }
            }
        }

        private void Configuration_Click(object sender, RoutedEventArgs e) {
            new ConfigWindow(this, this.App.Config).Show();
        }

        private void Tabs_Loaded(object sender, RoutedEventArgs e) {
            this.Tabs.SelectedIndex = 0;
        }

        public TextBox? GetCurrentInputBox() {
            return this.FindElementByName<TextBox>(this.Tabs, "InputBox");
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var scroller = this.FindElementByName<ScrollViewer>(this.Tabs, "scroller");
            scroller?.ScrollToBottom();
        }

        private void Scan_Click(object sender, RoutedEventArgs e) {
            new ServerScan(this).Show();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        internal void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Export_Click(object sender, RoutedEventArgs e) {
            new Export(this).Show();
        }

        private void Exit_Click(object sender, RoutedEventArgs e) {
            this.Close();
            this.App.Shutdown();
        }

        private void FriendList_Click(object sender, RoutedEventArgs e) {
            new FriendList(this).Show();
        }

        private void Channel_MouseDown(object sender, MouseButtonEventArgs e) {
            e.Handled = true;

            if (e.ChangedButton != MouseButton.Left) {
                return;
            }

            var channel = (TextBlock) sender;
            channel.ContextMenu!.PlacementTarget = channel;
            channel.ContextMenu!.IsOpen = true;
        }

    }
}
