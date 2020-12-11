using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using Microsoft.Win32;
using XIVChatCommon.Message.Server;

namespace XIVChat_Desktop {
    public partial class Export : INotifyPropertyChanged {
        public App App => (App)Application.Current;

        public Tab ExportTab { get; }

        public ExportFilter Filter => (ExportFilter)this.ExportTab.Filter;

        public ObservableCollection<ServerMessage.SenderPlayer> Senders { get; } = new ObservableCollection<ServerMessage.SenderPlayer>();

        private bool showTimestamps = true;

        public bool ShowTimestamps {
            get => this.showTimestamps;
            set {
                this.showTimestamps = value;
                this.OnPropertyChanged(nameof(this.ShowTimestamps));
            }
        }

        public Export(Window owner) {
            this.Owner = owner;

            this.ExportTab = new Tab("Export") {
                Filter = new ExportFilter {
                    Types = Tab.GeneralFilter().Types,
                },
            };

            this.Repopulate();

            this.InitializeComponent();
            this.DataContext = this;

            this.SetUpFilters();
        }

        private void SetUpFilters() {
            foreach (var category in (FilterCategory[])Enum.GetValues(typeof(FilterCategory))) {
                var tabContent = new WrapPanel {
                    Margin = new Thickness(8),
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };

                var buttonsPanel = new WrapPanel {
                    Margin = new Thickness(0, 0, 0, 4),
                };

                var selectButton = new Button {
                    Content = "Select all",
                };
                selectButton.Click += (sender, e) => SetAllChecked(true);

                var deselectButton = new Button {
                    Content = "Deselect all",
                    Margin = new Thickness(4, 0, 0, 0),
                };
                deselectButton.Click += (sender, e) => SetAllChecked(false);

                var doingMultiple = false;

                void SetAllChecked(bool isChecked) {
                    doingMultiple = true;

                    foreach (var child in tabContent.Children) {
                        if (!(child is CheckBox)) {
                            continue;
                        }

                        var check = (CheckBox)child;
                        check.IsChecked = isChecked;
                    }

                    this.Repopulate();

                    doingMultiple = false;
                }

                buttonsPanel.Children.Add(selectButton);
                buttonsPanel.Children.Add(deselectButton);

                tabContent.Children.Add(buttonsPanel);
                tabContent.Children.Add(new Separator());

                foreach (var type in category.Types()) {
                    var check = new CheckBox {
                        Content = type.Name(),
                        IsChecked = this.ExportTab.Filter.Types.Contains(type),
                    };

                    check.Checked += (sender, e) => {
                        this.ExportTab.Filter.Types.Add(type);

                        if (!doingMultiple) {
                            this.Repopulate();
                        }
                    };
                    check.Unchecked += (sender, e) => {
                        this.ExportTab.Filter.Types.Remove(type);

                        if (!doingMultiple) {
                            this.Repopulate();
                        }
                    };

                    tabContent.Children.Add(check);
                }

                var tabItem = new TabItem {
                    Header = new TextBlock(new Run(category.Name())),
                    Content = tabContent,
                };

                this.Tabs.Items.Add(tabItem);
            }
        }

        private void Repopulate() {
            this.ExportTab.RepopulateMessages(this.App.Window.Messages);
            this.SetUpSenders();
        }

        private void SetUpSenders() {
            // var senders = this.ExportTab.Messages
            var senders = this.App.Window.Messages
                .Where(msg => ((ExportFilter)this.ExportTab.Filter).AllowedMinusSenders(msg))
                .Select(msg => msg.GetSenderPlayer())
                .Where(sender => sender != null)
                .ToImmutableSortedSet();

            this.Senders.Clear();

            foreach (var sender in senders) {
                this.Senders.Add(sender!);
            }
        }

        public class ExportFilter : Filter {
            public ObservableCollection<ServerMessage.SenderPlayer> Senders { get; } = new ObservableCollection<ServerMessage.SenderPlayer>();

            public DateTime? Before { get; set; }
            public DateTime? After { get; set; }

            public void AddSender(ServerMessage.SenderPlayer sender) {
                if (this.Senders.Contains(sender)) {
                    return;
                }

                this.Senders.Add(sender);
            }

            [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
            public bool AllowedMinusSenders(ServerMessage message) {
                if (!base.Allowed(message)) {
                    return false;
                }

                if (this.Before != null && message.Timestamp > this.Before) {
                    return false;
                }

                if (this.After != null && message.Timestamp < this.After) {
                    return false;
                }

                return true;
            }

            [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
            public override bool Allowed(ServerMessage message) {
                if (!this.AllowedMinusSenders(message)) {
                    return false;
                }

                // check sender if any senders are selected
                var sender = message.GetSenderPlayer();
                if (this.Senders.Count != 0 && sender != null && !this.Senders.Contains(sender)) {
                    return false;
                }

                // our stuff
                return true;
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected void OnPropertyChanged1([CallerMemberName] string? propertyName = null) {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Markdown_Checked(object sender, RoutedEventArgs e) => this.SetMarkdownProcessing(true);

        private void Markdown_Unchecked(object sender, RoutedEventArgs e) => this.SetMarkdownProcessing(false);

        private void SetMarkdownProcessing(bool on) {
            this.ExportTab.ProcessMarkdown = on;
        }

        private void RightArrow_Click(object sender, RoutedEventArgs e) {
            var idx = this.SendersFilterSource.SelectedIndex;
            if (idx == -1) {
                return;
            }

            var player = this.Senders[idx];

            var filter = (ExportFilter)this.ExportTab.Filter;
            filter.AddSender(player);

            this.Repopulate();
        }

        private void LeftArrow_Click(object sender, RoutedEventArgs e) {
            var idx = this.SenderFiltersDest.SelectedIndex;
            if (idx == -1) {
                return;
            }

            var filter = (ExportFilter)this.ExportTab.Filter;

            var player = filter.Senders.ElementAt(idx);

            filter.Senders.Remove(player);

            this.Repopulate();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void Save_Click(object sender, RoutedEventArgs e) {
            // create a new flowdocument for saving
            var flow = new FlowDocument();

            // turn every message in the tab into a paragraph in the flowdocument
            foreach (var message in this.ExportTab.Messages) {
                this.Dispatch(DispatcherPriority.Background, () => {
                    var paragraph = new Paragraph();
                    // this has to be done on the main thread
                    var inlines = MessageFormatter.ChunksToTextBlock(message, this.App.Config.FontSize, this.ExportTab.ProcessMarkdown, this.ShowTimestamps);
                    paragraph.Inlines.AddRange(inlines);
                    flow.Blocks.Add(paragraph);
                });
            }

            // ask the user where to save
            var saveDialog = new SaveFileDialog {
                Filter = $"{DataFormats.Text} (*.txt)|*.txt|{DataFormats.Rtf} (*.rtf)|*.rtf",
            };

            if (saveDialog.ShowDialog(this) != true) {
                return;
            }

            var ext = saveDialog.FileName.Split('.').Last().ToLowerInvariant();
            string dataFormat = ext switch {
                "rtf" => DataFormats.Rtf,
                _ => DataFormats.Text,
            };
            // save the data into memory (this apparently has to happen on the main thread or we'd save directly into a
            // file)
            await using var memoryStream = new MemoryStream();
            new TextRange(flow.ContentStart, flow.ContentEnd).Save(memoryStream, dataFormat);

            // write the saved data to a file on another thread
            await Task.Run(async () => {
                await using var stream = new FileStream(saveDialog.FileName, FileMode.Create);
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(stream);
            });

            // show completion box
            MessageBox.Show("Exported successfully.");
        }

        private bool ignoreDateChanges;

        private void AfterDatePicker_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e) {
            if (this.ignoreDateChanges) {
                return;
            }

            var datePicker = (DatePicker)sender!;
            this.Filter.After = UpdateDate(this.Filter.After?.ToLocalTime(), datePicker.SelectedDate)?.ToUniversalTime();

            this.ignoreDateChanges = true;
            this.AfterTimePicker.SelectedDateTime = this.Filter.After?.ToLocalTime();
            this.ignoreDateChanges = false;

            this.Repopulate();
        }

        private void AfterTimePicker_OnSelectedDateTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e) {
            if (this.ignoreDateChanges) {
                return;
            }

            this.Filter.After = UpdateTime(this.Filter.After?.ToLocalTime(), e.NewValue)?.ToUniversalTime();

            this.AfterDatePicker.SelectedDate = this.Filter.After?.ToLocalTime();
        }

        private void BeforeDatePicker_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e) {
            if (this.ignoreDateChanges) {
                return;
            }

            var datePicker = (DatePicker)sender!;
            this.Filter.Before = UpdateDate(this.Filter.Before?.ToLocalTime(), datePicker.SelectedDate)?.ToUniversalTime();

            this.ignoreDateChanges = true;
            this.BeforeTimePicker.SelectedDateTime = this.Filter.Before?.ToLocalTime();
            this.ignoreDateChanges = false;

            this.Repopulate();
        }

        private void BeforeTimePicker_OnSelectedDateTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e) {
            if (this.ignoreDateChanges) {
                return;
            }

            this.Filter.Before = UpdateTime(this.Filter.Before?.ToLocalTime(), e.NewValue)?.ToUniversalTime();

            this.BeforeDatePicker.SelectedDate = this.Filter.Before?.ToLocalTime();
        }

        private static DateTime? UpdateTime(DateTime? dest, DateTime? source) {
            switch (dest) {
                case null when source == null:
                    return null;
                case null:
                    return source;
            }

            if (source == null) {
                return dest;
            }

            var newValue = source.Value;

            return new DateTime(dest.Value.Year, dest.Value.Month, dest.Value.Day, newValue.Hour, newValue.Minute, newValue.Second);
        }

        private static DateTime? UpdateDate(DateTime? dest, DateTime? source) {
            switch (dest) {
                case null when source == null:
                    return null;
                case null:
                    return source;
            }

            if (source == null) {
                return dest;
            }

            var newValue = source.Value;

            return new DateTime(newValue.Year, newValue.Month, newValue.Day, dest.Value.Hour, dest.Value.Minute, dest.Value.Second);
        }

        private void BeforeClear_Click(object sender, RoutedEventArgs e) {
            this.ignoreDateChanges = true;
            this.BeforeDatePicker.SelectedDate = null;
            this.ignoreDateChanges = false;
            this.BeforeTimePicker.SelectedDateTime = null;
        }

        private void AfterClear_Click(object sender, RoutedEventArgs e) {
            this.ignoreDateChanges = true;
            this.AfterDatePicker.SelectedDate = null;
            this.ignoreDateChanges = false;
            this.AfterTimePicker.SelectedDateTime = null;
        }
    }
}
