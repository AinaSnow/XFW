using System;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace XIVChat_Desktop {
    /// <summary>
    /// Interaction logic for FiltersSelection.xaml
    /// </summary>
    public partial class ManageTab {
        public App App => (App)Application.Current;

        public Tab Tab { get; }

        private readonly bool isNewTab;
        private readonly IImmutableSet<FilterType> oldFilters;

        public ManageTab(Window owner, Tab? tab) {
            this.Owner = owner;
            this.isNewTab = tab == null;
            this.Tab = tab ?? new Tab("") {
                Filter = Tab.GeneralFilter(),
            };
            this.oldFilters = this.Tab.Filter.Types.ToImmutableHashSet();

            this.InitializeComponent();
            this.DataContext = this;

            if (this.isNewTab) {
                this.Title = "Add tab";
            }

            foreach (var category in (FilterCategory[])Enum.GetValues(typeof(FilterCategory))) {
                var panel = new WrapPanel {
                    Margin = new Thickness(8),
                    Orientation = Orientation.Vertical,
                };

                var tabContent = new ScrollViewer {
                    Content = panel,
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

                void SetAllChecked(bool isChecked) {
                    foreach (var child in panel.Children) {
                        if (!(child is CheckBox)) {
                            continue;
                        }

                        var check = (CheckBox)child;
                        check.IsChecked = isChecked;
                    }
                }

                buttonsPanel.Children.Add(selectButton);
                buttonsPanel.Children.Add(deselectButton);

                panel.Children.Add(buttonsPanel);
                panel.Children.Add(new Separator());

                foreach (var type in category.Types()) {
                    var check = new CheckBox {
                        Content = type.Name(),
                        IsChecked = this.Tab.Filter.Types.Contains(type),
                    };

                    check.Checked += (sender, e) => {
                        this.Tab.Filter.Types.Add(type);
                    };
                    check.Unchecked += (sender, e) => {
                        this.Tab.Filter.Types.Remove(type);
                    };

                    panel.Children.Add(check);
                }

                var tabItem = new TabItem {
                    Header = new TextBlock(new Run(category.Name())),
                    Content = tabContent,
                };

                this.Tabs.Items.Add(tabItem);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            if (this.TabName.Text.Length == 0) {
                MessageBox.Show("Tab must have a name.");
                return;
            }

            this.Tab.Name = this.TabName.Text;
            this.Tab.ProcessMarkdown = this.MarkdownToggle.IsChecked ?? false;

            if (this.isNewTab) {
                this.App.Config.Tabs.Add(this.Tab);
            }

            if (this.isNewTab || !this.oldFilters.SetEquals(this.Tab.Filter.Types)) {
                this.Tab.RepopulateMessages(this.App.Window.Messages);
            }

            this.App.Config.Save();
            this.Close();
        }
    }
}
