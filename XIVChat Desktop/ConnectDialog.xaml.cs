using System.Windows;

namespace XIVChat_Desktop {
    /// <summary>
    /// Interaction logic for ConnectDialog.xaml
    /// </summary>
    public partial class ConnectDialog {
        public App App => (App)Application.Current;

        public ConnectDialog(Window owner) {
            this.Owner = owner;

            this.InitializeComponent();
            this.DataContext = this;
        }

        private void Connect_Clicked(object? sender, RoutedEventArgs e) {
            this.ConnectTo(this.Servers.SelectedServer);
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Servers_ItemDoubleClick(SavedServer? server) {
            this.ConnectTo(server);
        }

        private void ConnectTo(SavedServer? server) {
            if (server == null) {
                return;
            }

            this.App.Connect(server.Host, server.Port);

            this.Close();
        }
    }
}
