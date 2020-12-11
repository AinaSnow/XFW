using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XIVChatCommon.Message;
using XIVChatCommon.Message.Server;

namespace XIVChat_Desktop {
    public class MessageFormatter {
        private static readonly BitmapFrame FontIcon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/fonticon_ps4.tex.png"));

        public static IEnumerable<Inline> ChunksToTextBlock(ServerMessage message, double lineHeight, bool processMarkdown, bool showTimestamp) {
            var elements = new List<Inline>();

            if (showTimestamp) {
                var timestampString = message.Timestamp.ToLocalTime().ToString("t", CultureInfo.CurrentUICulture);
                elements.Add(new Run($"[{timestampString}]") {
                    Foreground = new SolidColorBrush(Colors.White),
                });
            }

            foreach (var chunk in message.Chunks) {
                switch (chunk) {
                    case TextChunk textChunk:
                        var colour = textChunk.Foreground ?? textChunk.FallbackColour ?? 0;

                        var r = (byte)((colour >> 24) & 0xFF);
                        var g = (byte)((colour >> 16) & 0xFF);
                        var b = (byte)((colour >> 8) & 0xFF);
                        var a = (byte)(colour & 0xFF);

                        var brush = new SolidColorBrush(Color.FromArgb(a, r, g, b));
                        var style = textChunk.Italic ? FontStyles.Italic : FontStyles.Normal;

                        if (processMarkdown) {
                            var inlines = Markdown.MarkdownToInlines(textChunk.Content);

                            foreach (var inline in inlines) {
                                inline.Foreground = brush;
                                if (inline.FontStyle == FontStyles.Normal) {
                                    inline.FontStyle = style;
                                }

                                elements.Add(inline);
                            }
                        } else {
                            elements.Add(new Run(textChunk.Content) {
                                Foreground = brush,
                                FontStyle = style,
                            });
                        }

                        break;
                    case IconChunk iconChunk:
                        var bounds = GetBounds(iconChunk.index);
                        if (bounds == null) {
                            break;
                        }

                        var width = lineHeight / bounds.Value.Height * bounds.Value.Width;

                        var cropped = new CroppedBitmap(FontIcon, bounds.Value);
                        var image = new Image {
                            Source = cropped,
                            Width = width,
                            Height = lineHeight,
                        };
                        elements.Add(new InlineUIContainer(image) {
                            BaselineAlignment = BaselineAlignment.Bottom,
                        });
                        break;
                }
            }

            return elements;
        }

        private static Int32Rect? GetBounds(byte id) => id switch {
            1 => new Int32Rect(0, 0, 20, 20),
            2 => new Int32Rect(20, 0, 20, 20),
            3 => new Int32Rect(40, 0, 20, 20),
            4 => new Int32Rect(60, 0, 20, 20),
            5 => new Int32Rect(80, 0, 20, 20),
            6 => new Int32Rect(0, 20, 20, 20),
            7 => new Int32Rect(20, 20, 20, 20),
            8 => new Int32Rect(40, 20, 20, 20),
            9 => new Int32Rect(60, 20, 20, 20),
            10 => new Int32Rect(80, 20, 20, 20),
            11 => new Int32Rect(0, 40, 20, 20),
            12 => new Int32Rect(20, 40, 20, 20),
            13 => new Int32Rect(40, 40, 20, 20),
            14 => new Int32Rect(60, 40, 20, 20),
            15 => new Int32Rect(80, 40, 20, 20),
            16 => new Int32Rect(60, 100, 20, 20),
            17 => new Int32Rect(80, 100, 20, 20),
            18 => new Int32Rect(0, 60, 54, 20),
            19 => new Int32Rect(54, 60, 54, 20),
            20 => new Int32Rect(60, 80, 20, 20),
            21 => new Int32Rect(0, 80, 28, 20),
            22 => new Int32Rect(28, 80, 32, 20),
            23 => new Int32Rect(80, 80, 20, 20),
            24 => new Int32Rect(0, 100, 28, 20),
            25 => new Int32Rect(28, 100, 32, 20),
            51 => new Int32Rect(124, 0, 20, 20),
            52 => new Int32Rect(144, 0, 20, 20),
            53 => new Int32Rect(164, 0, 20, 20),
            54 => new Int32Rect(100, 0, 12, 20),
            55 => new Int32Rect(112, 0, 12, 20),
            56 => new Int32Rect(100, 20, 20, 20),
            57 => new Int32Rect(120, 20, 20, 20),
            58 => new Int32Rect(140, 20, 20, 20),
            59 => new Int32Rect(100, 40, 20, 20),
            60 => new Int32Rect(120, 40, 20, 20),
            61 => new Int32Rect(140, 40, 20, 20),
            62 => new Int32Rect(160, 20, 20, 20),
            63 => new Int32Rect(160, 40, 20, 20),
            64 => new Int32Rect(184, 0, 20, 20),
            65 => new Int32Rect(204, 0, 20, 20),
            66 => new Int32Rect(224, 0, 20, 20),
            67 => new Int32Rect(180, 20, 20, 20),
            68 => new Int32Rect(200, 20, 20, 20),
            69 => new Int32Rect(236, 236, 20, 20),
            70 => new Int32Rect(180, 40, 20, 20),
            71 => new Int32Rect(200, 40, 20, 20),
            72 => new Int32Rect(220, 40, 20, 20),
            73 => new Int32Rect(220, 20, 20, 20),
            74 => new Int32Rect(108, 60, 20, 20),
            75 => new Int32Rect(128, 60, 20, 20),
            76 => new Int32Rect(148, 60, 20, 20),
            77 => new Int32Rect(168, 60, 20, 20),
            78 => new Int32Rect(188, 60, 20, 20),
            79 => new Int32Rect(208, 60, 20, 20),
            80 => new Int32Rect(228, 60, 20, 20),
            81 => new Int32Rect(100, 80, 20, 20),
            82 => new Int32Rect(120, 80, 20, 20),
            83 => new Int32Rect(140, 80, 20, 20),
            84 => new Int32Rect(160, 80, 20, 20),
            85 => new Int32Rect(180, 80, 20, 20),
            86 => new Int32Rect(200, 80, 20, 20),
            87 => new Int32Rect(220, 80, 20, 20),
            88 => new Int32Rect(100, 100, 20, 20),
            89 => new Int32Rect(120, 100, 20, 20),
            90 => new Int32Rect(140, 100, 20, 20),
            91 => new Int32Rect(160, 100, 20, 20),
            92 => new Int32Rect(180, 100, 20, 20),
            93 => new Int32Rect(200, 100, 20, 20),
            94 => new Int32Rect(220, 100, 20, 20),
            95 => new Int32Rect(0, 120, 20, 20),
            96 => new Int32Rect(20, 120, 20, 20),
            97 => new Int32Rect(40, 120, 20, 20),
            98 => new Int32Rect(60, 120, 20, 20),
            99 => new Int32Rect(80, 120, 20, 20),
            100 => new Int32Rect(100, 120, 20, 20),
            101 => new Int32Rect(120, 120, 20, 20),
            102 => new Int32Rect(140, 120, 20, 20),
            103 => new Int32Rect(160, 120, 20, 20),
            104 => new Int32Rect(180, 120, 20, 20),
            105 => new Int32Rect(200, 120, 20, 20),
            106 => new Int32Rect(220, 120, 20, 20),
            107 => new Int32Rect(0, 140, 20, 20),
            108 => new Int32Rect(20, 140, 20, 20),
            109 => new Int32Rect(40, 140, 20, 20),
            110 => new Int32Rect(60, 140, 20, 20),
            111 => new Int32Rect(80, 140, 20, 20),
            _ => null,
        };
    }
}
