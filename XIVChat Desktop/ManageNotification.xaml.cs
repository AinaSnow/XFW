using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XIVChatCommon.Message;

namespace XIVChat_Desktop {
    public partial class ManageNotification {
        #region Commands

        public static readonly RoutedUICommand AddEmpty = new RoutedUICommand(
            "AddEmpty",
            "AddEmpty",
            typeof(ManageNotification)
        );

        private void AddEmpty_Execute(object sender, ExecutedRoutedEventArgs e) {
            if (!(e.Parameter is ObservableCollection<StringWrapper> list)) {
                return;
            }

            list.Add(new StringWrapper(string.Empty));
        }

        private void AddEmpty_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        #endregion

        public App App => (App)Application.Current;

        public Notification Notification { get; }

        private bool NewNotification { get; }

        public ObservableCollection<StringWrapper> Regexes { get; }
        public ObservableCollection<StringWrapper> Substrings { get; }

        public ManageNotification(Window owner, Notification? notification) {
            this.Owner = owner;

            this.NewNotification = notification == null;
            this.Notification = notification ?? new Notification("");
            this.Regexes = new ObservableCollection<StringWrapper>(this.Notification.Regexes.Select(regex => new StringWrapper(regex)));
            this.Substrings = new ObservableCollection<StringWrapper>(this.Notification.Substrings.Select(sub => new StringWrapper(sub)));

            this.InitializeComponent();
            this.DataContext = this;

            this.SetUpChannels();
        }

        private void SetUpChannels() {
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

            void SetAllChecked(bool isChecked) {
                foreach (var child in this.Channels.Children) {
                    if (!(child is CheckBox)) {
                        continue;
                    }

                    var check = (CheckBox)child;
                    check.IsChecked = isChecked;
                }
            }

            buttonsPanel.Children.Add(selectButton);
            buttonsPanel.Children.Add(deselectButton);

            this.Channels.Children.Add(buttonsPanel);
            this.Channels.Children.Add(new Separator());

            foreach (var type in (ChatType[])Enum.GetValues(typeof(ChatType))) {
                var check = new CheckBox {
                    Content = type.Name(),
                    IsChecked = this.Notification.Channels.Contains(type),
                };

                check.Checked += (sender, e) => {
                    this.Notification.Channels.Add(type);
                };
                check.Unchecked += (sender, e) => {
                    this.Notification.Channels.Remove(type);
                };

                this.Channels.Children.Add(check);
            }
        }

        private void ManageNotification_OnClosed(object? sender, EventArgs e) {
            this.Notification.Regexes = this.Regexes
                .Select(wrapper => wrapper.Value)
                .Where(regex => regex.Length > 0 && regex.IsValidRegex())
                .ToList();

            this.Notification.Substrings = this.Substrings
                .Select(wrapper => wrapper.Value)
                .Where(substring => substring.Length > 0)
                .ToList();

            if (this.NewNotification) {
                this.App.Config.Notifications.Add(this.Notification);
            }

            this.App.Config.Save();
        }
    }
}
