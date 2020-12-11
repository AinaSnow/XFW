using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace XIVChat_Desktop {
    /// <summary>
    /// Interaction logic for TrustDialog.xaml
    /// </summary>
    public partial class TrustDialog {
        private readonly ChannelWriter<bool> trustChannel;
        private readonly byte[] remoteKey;

        private App App => (App)Application.Current;

        public TrustDialog(Window owner, ChannelWriter<bool> trustChannel, byte[] remoteKey) {
            this.Owner = owner;
            this.trustChannel = trustChannel;
            this.remoteKey = remoteKey;

            this.InitializeComponent();

            this.ClientPublicKey.Text = ToHexString(this.App.Config.KeyPair.PublicKey);
            var clientColours = BreakIntoColours(this.App.Config.KeyPair.PublicKey);
            for (int i = 0; i < this.ClientPublicKeyColours.Children.Count; i++) {
                var rect = (Rectangle)this.ClientPublicKeyColours.Children[i];
                rect.Fill = new SolidColorBrush(clientColours[i]);
            }

            this.ServerPublicKey.Text = ToHexString(remoteKey);
            var serverColours = BreakIntoColours(remoteKey);
            for (int i = 0; i < this.ServerPublicKeyColours.Children.Count; i++) {
                var rect = (Rectangle)this.ServerPublicKeyColours.Children[i];
                rect.Fill = new SolidColorBrush(serverColours[i]);
            }
        }

        private static List<Color> BreakIntoColours(IEnumerable<byte> key) {
            var colours = new List<Color>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var chunk in key.ToList().Chunks(3)) {
                var r = chunk[0];
                var g = chunk.Count > 1 ? chunk[1] : (byte)0;
                var b = chunk.Count > 2 ? chunk[2] : (byte)0;

                colours.Add(Color.FromRgb(r, g, b));
            }

            return colours;
        }

        private static string ToHexString(IEnumerable<byte> bytes) {
            return string.Join("", bytes.Select(b => b.ToString("X2")));
        }

        private async void Yes_Click(object sender, RoutedEventArgs e) {
            var keyName = this.KeyName.Text;
            if (keyName.Length == 0) {
                MessageBox.Show("You must give this key a name.");
                return;
            }

            var trustedKey = new TrustedKey(keyName, this.remoteKey);
            this.App.Config.TrustedKeys.Add(trustedKey);
            this.App.Config.Save();
            await this.trustChannel.WriteAsync(true);
            this.Close();
        }

        private async void No_Click(object sender, RoutedEventArgs e) {
            await this.trustChannel.WriteAsync(false);
            this.Close();
        }
    }
}
