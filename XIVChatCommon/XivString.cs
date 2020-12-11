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
                    var data = new BinaryReader(new MemoryStream(reader.ReadBytes((int)len))); // data
                    var end = reader.ReadByte(); // end
                    if (end != End) {
                        throw new ArgumentException("Input was not a valid XivString");
                    }

                    switch (kind) {
                        // icon processing
                        case 0x12:
                            var spriteIndex = GetInteger(data);
                            chunks.Add(new IconChunk {
                                index = (byte)spriteIndex,
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
                    reader.ReadBytes((int)len); // data
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

        private enum IntegerType {
            // used as an internal marker; sometimes single bytes are bare with no marker at all
            None = 0,

            Byte = 0xF0,
            ByteTimes256 = 0xF1,
            Int16 = 0xF2,
            ByteShl16 = 0xF3,
            Int16Packed = 0xF4, // seen in map links, seemingly 2 8-bit values packed into 2 bytes with only one marker
            Int16Shl8 = 0xF5,
            Int24Special = 0xF6, // unsure how different form Int24 - used for hq items that add 1 million, also used for normal 24-bit values in map links
            Int8Shl24 = 0xF7,
            Int8Shl8Int8 = 0xF8,
            Int8Shl8Int8Shl8 = 0xF9,
            Int24 = 0xFA,
            Int16Shl16 = 0xFB,
            Int24Packed = 0xFC, // used in map links- sometimes short+byte, sometimes... not??
            Int16Int8Shl8 = 0xFD,
            Int32 = 0xFE,
        }

        public static uint GetInteger(BinaryReader input) {
            var t = input.ReadByte();
            var type = (IntegerType)t;
            return GetInteger(input, type);
        }

        private static uint GetInteger(BinaryReader input, IntegerType type) {
            const byte byteLengthCutoff = 0xF0;

            var t = (byte)type;
            if (t < byteLengthCutoff) {
                return (uint)(t - 1);
            }

            switch (type) {
                case IntegerType.Byte:
                    return input.ReadByte();

                case IntegerType.ByteTimes256:
                    return input.ReadByte() * (uint)256;
                case IntegerType.ByteShl16:
                    return (uint)(input.ReadByte() << 16);
                case IntegerType.Int8Shl24:
                    return (uint)(input.ReadByte() << 24);
                case IntegerType.Int8Shl8Int8: {
                    var v = 0;
                    v |= input.ReadByte() << 24;
                    v |= input.ReadByte();
                    return (uint)v;
                }
                case IntegerType.Int8Shl8Int8Shl8: {
                    var v = 0;
                    v |= input.ReadByte() << 24;
                    v |= input.ReadByte() << 8;
                    return (uint)v;
                }


                case IntegerType.Int16:
                // fallthrough - same logic
                case IntegerType.Int16Packed: {
                    var v = 0;
                    v |= input.ReadByte() << 8;
                    v |= input.ReadByte();
                    return (uint)v;
                }
                case IntegerType.Int16Shl8: {
                    var v = 0;
                    v |= input.ReadByte() << 16;
                    v |= input.ReadByte() << 8;
                    return (uint)v;
                }
                case IntegerType.Int16Shl16: {
                    var v = 0;
                    v |= input.ReadByte() << 24;
                    v |= input.ReadByte() << 16;
                    return (uint)v;
                }

                case IntegerType.Int24Special:
                // Fallthrough - same logic
                case IntegerType.Int24Packed:
                // fallthrough again
                case IntegerType.Int24: {
                    var v = 0;
                    v |= input.ReadByte() << 16;
                    v |= input.ReadByte() << 8;
                    v |= input.ReadByte();
                    return (uint)v;
                }
                case IntegerType.Int16Int8Shl8: {
                    var v = 0;
                    v |= input.ReadByte() << 24;
                    v |= input.ReadByte() << 16;
                    v |= input.ReadByte() << 8;
                    return (uint)v;
                }
                case IntegerType.Int32: {
                    var v = 0;
                    v |= input.ReadByte() << 24;
                    v |= input.ReadByte() << 16;
                    v |= input.ReadByte() << 8;
                    v |= input.ReadByte();
                    return (uint)v;
                }

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
