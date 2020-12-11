using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using Windows.UI.Notifications;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf;
using XIVChatCommon.Message;
using XIVChatCommon.Message.Server;

// TODO: search messages
// TODO: notifications for targeted messages (like emote targeting you)

namespace XIVChat_Desktop {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : INotifyPropertyChanged {
        public MainWindow Window { get; private set; } = null!;
        public Configuration Config { get; private set; } = null!;

        private Lazy<TaskbarIcon> TaskbarIcon { get; } = new Lazy<TaskbarIcon>(() => new TaskbarIcon {
            IconSource = new BitmapImage(new Uri("pack://application:,,,/Resources/logo.ico")),
        });

        public string? LastHost { get; set; }

        private Connection? connection;

        public Connection? Connection {
            get => this.connection;
            set {
                this.connection = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Connection)));
                this.ConnectionStatusChanged();
            }
        }

        public bool Connected => this.Connection != null;

        public event PropertyChangedEventHandler? PropertyChanged;

        private async void Application_Startup(object sender, StartupEventArgs e) {
            Notifications.Initialise();

            try {
                this.Config = Configuration.Load() ?? new Configuration();
            } catch (Exception ex) {
                var result = MessageBox.Show(
                    $"Could not load the configuration file: {ex.Message}. Do you want to create a new configuration file and overwrite the old one?",
                    "Error loading config",
                    MessageBoxButton.YesNo
                );

                if (result == MessageBoxResult.Yes) {
                    this.Config = new Configuration();
                } else {
                    this.Shutdown(1);
                    return;
                }
            }

            try {
                this.Config.Save();
            } catch (Exception ex) {
                MessageBox.Show($"Could not save configuration file. {ex.Message}");
            }

            this.Config.PropertyChanged += (o, args) => {
                if (args.PropertyName != nameof(Configuration.Theme)) {
                    return;
                }

                this.UpdateTheme();
            };

            this.UpdateTheme();

            LocaliseAllElements();

            // I guess this gets initialised where you call it the first time, so initialise it on the UI thread
            this.Dispatcher.Invoke(() => { });



            this.InitialiseWindow();
        }

        private static void LocaliseAllElements() {
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)
                )
            );
        }

        public void InitialiseWindow() {
            var wnd = new MainWindow();
            this.Window = wnd;

            wnd.Show();

            // initialise a config window to apply all our settings
            _ = new ConfigWindow(wnd, this.Config);
        }

        private void UpdateTheme() {
            ThemeManager.Current.ApplicationTheme = this.Config.Theme switch {
                Theme.System => null,
                Theme.Dark => ApplicationTheme.Dark,
                Theme.Light => ApplicationTheme.Light,
                _ => null,
            };
        }

        private void ConnectionStatusChanged() {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Connected)));
        }

        public void Connect(string host, ushort port) {
            if (this.Connected) {
                return;
            }

            this.Connection = new Connection(this, host, port);
            this.Connection.ReceiveMessage += this.OnReceiveMessage;
            Task.Run(this.Connection.Connect);
        }

        public void Disconnect() {
            if (!this.Connected) {
                return;
            }

            this.Connection?.Disconnect();
            this.Connection = null;
        }

        private static void Win10Notify(string title, string text, string? attribution) {
            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(text);

            if (attribution != null) {
                builder.AddAttributionText(attribution);
            }

            var content = builder.GetToastContent();

            var toast = new ToastNotification(content.GetXml());

            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }

        private void Notify(string title, string text) {
            this.TaskbarIcon.Value.ShowBalloonTip(title, text, BalloonIcon.None);
        }

        private void OnReceiveMessage(ServerMessage message) {
            if (!this.Config.Notifications.Any(notif => notif.Matches(message))) {
                return;
            }

            var sender = message.GetSenderPlayer();

            string title;
            if (sender != null) {
                var name = sender.Name;

                if (sender.Server != 0) {
                    name += $" ({Util.WorldName(sender.Server)})";
                }

                title = name;
            } else {
                title = "Notification";
            }

            var text = message.ContentText;
            var attribution = message.Channel.Name();

            if (Environment.OSVersion.Version.Major < 10) {
                this.Notify(title, text);
            } else {
                Win10Notify(title, text, attribution);
            }
        }
    }
}
