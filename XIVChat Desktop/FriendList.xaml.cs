using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using XIVChatCommon.Message;

namespace XIVChat_Desktop {
    public partial class FriendList : INotifyPropertyChanged {
        public static readonly RoutedUICommand SendTell = new RoutedUICommand(
            "SendTell",
            "SendTell",
            typeof(FriendList)
        );

        public App App => (App)Application.Current;

        private bool waiting;

        public bool Waiting {
            get => this.waiting;
            set {
                this.waiting = value;
                this.OnPropertyChanged(nameof(this.Waiting));
            }
        }

        public FriendList(Window owner) {
            this.Owner = owner;

            this.InitializeComponent();
            this.DataContext = this;

            this.App.Window.FriendList.CollectionChanged += this.OnFriendListChanged;
        }

        private void SendTell_Executed(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs) {
            if (!(executedRoutedEventArgs.Parameter is Player player)) {
                return;
            }

            var name = player.Name;
            var world = player.HomeWorldName;

            if (name == null || world == null) {
                return;
            }

            this.App.Window.InsertTellCommand(name, world);

            this.Close();
        }

        private void SendTell_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = this.App.Connected;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) {
            var conn = this.App.Connection;
            if (conn == null) {
                return;
            }

            this.Waiting = true;
            conn.RequestFriendList();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnFriendListChanged(object sender, NotifyCollectionChangedEventArgs e) {
            this.Waiting = false;
        }

        private void FriendList_OnClosed(object? sender, EventArgs e) {
            this.App.Window.FriendList.CollectionChanged -= this.OnFriendListChanged;
        }
    }

    public class FriendListStatusConverter : IValueConverter {
        private static readonly BitmapFrame ImageOnline = ImageOf("app_status_online");
        private static readonly BitmapFrame ImageOffline = ImageOf("app_status_offline");
        private static readonly BitmapFrame ImageAfk = ImageOf("app_status_afk");
        private static readonly BitmapFrame ImageContents = ImageOf("app_status_contents");
        private static readonly BitmapFrame ImageContentsSimilar = ImageOf("app_status_contents_similar");
        private static readonly BitmapFrame ImageContentsSame = ImageOf("app_status_contents_same");
        private static readonly BitmapFrame ImageCrossPartyLeader = ImageOf("app_status_cross_party_leader");
        private static readonly BitmapFrame ImageCrossPartyMember = ImageOf("app_status_cross_party_member");
        private static readonly BitmapFrame ImagePartyLeader = ImageOf("app_status_party_leader");
        private static readonly BitmapFrame ImagePartyMember = ImageOf("app_status_party_member");
        private static readonly BitmapFrame ImageRoleplaying = ImageOf("app_status_roleplaying");

        private static BitmapFrame ImageOf(string file) {
            var uri = $"pack://application:,,,/Resources/status/{file}.png";
            return BitmapFrame.Create(new Uri(uri));
        }

        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is Player player)) {
                return null;
            }

            if (player.HasStatus(PlayerStatus.SharingDuty)) {
                return ImageContentsSame;
            }

            if (player.HasStatus(PlayerStatus.SimilarDuty)) {
                return ImageContentsSimilar;
            }

            if (player.HasStatus(PlayerStatus.AnotherWorld)) {
                return ImageContents;
            }

            if (player.HasStatus(PlayerStatus.AwayFromKeyboard)) {
                return ImageAfk;
            }

            if (player.HasStatus(PlayerStatus.RolePlaying)) {
                return ImageRoleplaying;
            }

            if (player.HasStatus(PlayerStatus.PartyLeaderCrossWorld)) {
                return ImageCrossPartyLeader;
            }

            if (player.HasStatus(PlayerStatus.PartyMemberCrossWorld)) {
                return ImageCrossPartyMember;
            }

            if (player.HasStatus(PlayerStatus.PartyLeader)) {
                return ImagePartyLeader;
            }

            if (player.HasStatus(PlayerStatus.PartyMember)) {
                return ImagePartyMember;
            }

            if (player.HasStatus(PlayerStatus.Online)) {
                return ImageOnline;
            }

            return ImageOffline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
