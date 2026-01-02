using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XIVChatCommon.Message;

namespace XIVChatCommon {
    public static class XivString {
        private const byte Start = 2;
        private const byte End = 3;

        public static List<Chunk> ToChunks(byte[] bytes) {
            var chunks = new List<Chunk>();
            var stringBytes = new List<byte>();

            var italic = false;
            uint? foreground = null;
            uint? glow = null;

            void AppendCurrent(bool clear) {
                var text = Encoding.UTF8.GetString(stringBytes.ToArray());
                chunks.Add(new TextChunk(text) {
                    Foreground = foreground,
                    Glow = glow,
                    Italic = italic,
                });
                if (clear) {
                    stringBytes.Clear();
                }
            }

            var reader = new BinaryReader(new MemoryStream(bytes));
            while (reader.BaseStream.Position < reader.BaseStream.Length) {
                var b = reader.ReadByte();
                if (b == Start) {
                    var kind = reader.ReadByte(); // kind
                    var len = GetInteger(reader); // data length
                    var data = new BinaryReader(new MemoryStream(reader.ReadBytes((int) len))); // data
                    var end = reader.ReadByte(); // end
                    if (end != End) {
                        throw new ArgumentException("Input was not a valid XivString");
                    }

                    switch (kind) {
                        // icon processing
                        case 0x12:
                            var spriteIndex = GetInteger(data);
                            chunks.Add(new IconChunk {
                                index = (byte) spriteIndex,
                            });
                            break;
                        // italics processing
                        case 0x1a:
                            var newStatus = GetInteger(data) == 1;

                            var appendNow = (italic && !newStatus) || (!italic && newStatus);
                            if (!appendNow) {
                                break;
                            }

                            AppendCurrent(true);

                            italic = newStatus;
                            break;
                        // foreground
                        case 0x48:
                            break;
                        // glow
                        case 0x49:
                            break;
                    }

                    continue;
                }

                stringBytes.Add(b);
            }

            return chunks;
        }

        public static string GetText(byte[] bytes) {
            var stringBytes = new List<byte>();

            var reader = new BinaryReader(new MemoryStream(bytes));
            while (reader.BaseStream.Position < reader.BaseStream.Length) {
                var b = reader.ReadByte();
                if (b == Start) {
                    reader.ReadByte(); // kind
                    var len = GetInteger(reader); // data length
                    reader.ReadBytes((int) len); // data
                    var end = reader.ReadByte(); // end
                    if (end != End) {
                        throw new ArgumentException("Input was not a valid XivString");
                    }

                    continue;
                }

                stringBytes.Add(b);
            }

            return Encoding.UTF8.GetString(stringBytes.ToArray());
        }

        // Thanks, Dalamud

        public static uint GetInteger(BinaryReader input) {
            uint marker = input.ReadByte();
            if (marker < 0xD0) {
                return marker - 1;
            }

            // the game adds 0xF0 marker for values >= 0xCF
            // uasge of 0xD0-0xEF is unknown, should we throw here?
            // if (marker < 0xF0) throw new NotSupportedException();

            marker = (marker + 1) & 0b1111;

            var ret = new byte[4];
            for (var i = 3; i >= 0; i--) {
                ret[i] = (marker & (1 << i)) == 0 ? (byte) 0 : input.ReadByte();
            }

            return BitConverter.ToUInt32(ret, 0);
        }
    }
}
