using System.Collections.Generic;
using System.Windows;

namespace XIVChat_Desktop.Controls {
    /// <summary>
    /// Interaction logic for SavedServers.xaml
    /// </summary>
    public partial class SavedServers {
        public App App => (App)Application.Current;
        private Configuration Config => this.App.Config;
        private Window Window => Window.GetWindow(this)!;

        public IEnumerable<SavedServer> ItemsSource {
            get { return (IEnumerable<SavedServer>)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        public Visibility ControlsVisibility {
            get { return (Visibility)this.GetValue(ControlsVisibilityProperty); }
            set { this.SetValue(ControlsVisibilityProperty, value); }
        }

        public SavedServer? SelectedServer {
            get {
                var item = this.Servers.SelectedItem;

                return item as SavedServer;
            }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(IEnumerable<SavedServer>),
            typeof(SavedServers)
        );

        public static readonly DependencyProperty ControlsVisibilityProperty = DependencyProperty.Register(
            "ControlsVisibility",
            typeof(Visibility),
            typeof(SavedServers)
        );

        public SavedServers() {
            this.InitializeComponent();
        }

        private void AddServer_Click(object sender, RoutedEventArgs e) {
            new ManageServer(this.Window, null).ShowDialog();
        }

        private void DeleteServer_Click(object sender, RoutedEventArgs e) {
            var server = this.SelectedServer;
            if (server == null) {
                return;
            }

            this.Config.Servers.Remove(server);
            this.Config.Save();
        }

        private void EditServer_Click(object sender, RoutedEventArgs e) {
            var server = this.SelectedServer;
            if (server == null) {
                return;
            }

            new ManageServer(this.Window, server).ShowDialog();
        }

        private void Item_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var server = ((FrameworkElement)e.OriginalSource).DataContext;
            if (!(server is SavedServer)) {
                return;
            }

            this.ItemDoubleClick?.Invoke((SavedServer)server);
        }

        public delegate void MouseDoubleClickHandler(SavedServer server);

        public event MouseDoubleClickHandler? ItemDoubleClick;
    }
}
