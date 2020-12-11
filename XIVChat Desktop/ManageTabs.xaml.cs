using System.Windows;
using System.Windows.Input;

namespace XIVChat_Desktop {
    /// <summary>
    /// Interaction logic for ManageTabs.xaml
    /// </summary>
    public partial class ManageTabs {
        public App App => (App)Application.Current;

        private Tab? SelectedTab {
            get {
                var item = this.Tabs.SelectedItem;

                return item as Tab;
            }
        }

        public ManageTabs(Window owner) {
            this.Owner = owner;
            this.InitializeComponent();
            this.DataContext = this;
        }

        private void AddTab_Click(object sender, RoutedEventArgs e) {
            new ManageTab(this, null).ShowDialog();
        }

        private void EditTab_Click(object sender, RoutedEventArgs e) {
            var tab = this.SelectedTab;
            if (tab == null) {
                return;
            }
            new ManageTab(this, tab).ShowDialog();
        }

        private void DeleteTab_Click(object sender, RoutedEventArgs e) {
            var tab = this.SelectedTab;
            if (tab == null) {
                return;
            }

            this.App.Config.Tabs.Remove(tab);
            this.App.Config.Save();

        }

        private void Tab_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var item = ((FrameworkElement)e.OriginalSource).DataContext;
            if (!(item is Tab)) {
                return;
            }

            var tab = item as Tab;

            new ManageTab(this, tab).ShowDialog();
        }
    }
}
