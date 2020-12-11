using System;
using System.Windows;
using System.Windows.Media;

namespace XIVChat_Desktop {
    public class XivChatWindow : Window {
        // ReSharper disable once MemberCanBeProtected.Global
        public XivChatWindow() {
            this.SetValue(TextOptions.TextRenderingModeProperty, TextRenderingMode.Auto);
            this.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
            this.SetValue(TextOptions.TextHintingModeProperty, TextHintingMode.Fixed);

            this.ContentRendered += this.FixRendering;
        }

        private void FixRendering(object? sender, EventArgs eventArgs) {
            this.InvalidateVisual();
        }
    }
}
