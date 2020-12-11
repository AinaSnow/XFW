using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MessagePack;

namespace XIVChatCommon.Message.Server {
    public enum ServerOperation : byte {
        Pong = 1,
        Message = 2,
        Shutdown = 3,
        PlayerData = 4,
        Availability = 5,
        Channel = 6,
        Backlog = 7,
        PlayerList = 8,
        LinkshellList = 9,
    }

    #region Pong

    public class Pong : IEncodable {
        public static Pong Instance { get; } = new Pong();

        [IgnoreMember]
        protected override byte Code => (byte)ServerOperation.Pong;

        protected override byte[] PayloadEncode() {
            return new byte[0];
        }
    }

    #endregion

    #region Message

    [MessagePackObject]
    public class ServerMessage : IEncodable {
        [MessagePackFormatter(typeof(MillisecondsDateTimeFormatter))]
        [Key(0)]
        public DateTime Timestamp { get; set; }

        [Key(1)]
        public ChatType Channel { get; set; }

        [Key(2)]
        public byte[] Sender { get; set; }

        [Key(3)]
        public byte[] Content { get; set; }

        [Key(4)]
        public List<Chunk> Chunks { get; set; }

        [IgnoreMember]
        public string ContentText => XivString.GetText(this.Content);

        [IgnoreMember]
        public string SenderText => XivString.GetText(this.Sender);

        [IgnoreMember]
        protected override byte Code => (byte)ServerOperation.Message;

        public ServerMessage(DateTime timestamp, ChatType channel, byte[] sender, byte[] content, List<Chunk> chunks) {
            this.Timestamp = timestamp;
            this.Channel = channel;
            this.Sender = sender;
            this.Content = content;
            this.Chunks = chunks;
        }

        public static ServerMessage Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<ServerMessage>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }

        public SenderPlayer? GetSenderPlayer() {
            using var stream = new MemoryStream(this.Sender);
            using var reader = new BinaryReader(stream);

            var text = new List<byte>();

            while (reader.BaseStream.Position < reader.BaseStream.Length) {
                var b = reader.ReadByte();

                // read payloads
                if (b == 2) {
                    var chunkType = reader.ReadByte();
                    var chunkLen = XivString.GetInteger(reader);

                    // interactive
                    if (chunkType == 0x27) {
                        var subType = reader.ReadByte();
                        // player name
                        if (subType == 0x01) {
                            // unk
                            reader.ReadByte();

                            var serverId = (ushort)XivString.GetInteger(reader);

                            // unk
                            reader.ReadBytes(2);

                            var nameLen = (int)XivString.GetInteger(reader);
                            var playerName = Encoding.UTF8.GetString(reader.ReadBytes(nameLen));

                            return new SenderPlayer(playerName, serverId);
                        }
                    }

                    reader.ReadBytes((int)chunkLen);
                    continue;
                }

                // read text
                text.Add(b);
            }

            if (text.Count == 0) {
                return null;
            }

            var name = Encoding.UTF8.GetString(text.ToArray());

            // remove the party position if present
            var chars = name.ToCharArray();
            if (chars.Length > 0 && PartyChars.Contains((chars[0]))) {
                name = name.Substring(1);
            }

            return new SenderPlayer(name, 0);
        }

        private static readonly char[] PartyChars = {
            '\ue090', '\ue091', '\ue092', '\ue093', '\ue094', '\ue095', '\ue096', '\ue097',
        };

        public class SenderPlayer : IComparable<SenderPlayer>, IComparable {
            public string Name { get; }
            public ushort Server { get; }

            public SenderPlayer(string name, ushort server) {
                this.Name = name;
                this.Server = server;
            }

            protected bool Equals(SenderPlayer other) {
                return this.Name == other.Name && this.Server == other.Server;
            }

            public override bool Equals(object? obj) {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }

                if (ReferenceEquals(this, obj)) {
                    return true;
                }

                return obj.GetType() == this.GetType() && this.Equals((SenderPlayer)obj);
            }

            public override int GetHashCode() {
                unchecked {
                    return (this.Name.GetHashCode() * 397) ^ (int)this.Server;
                }
            }

            public int CompareTo(SenderPlayer? other) {
                if (ReferenceEquals(this, other)) {
                    return 0;
                }

                if (ReferenceEquals(null, other)) {
                    return 1;
                }

                var nameComparison = string.Compare(this.Name, other.Name, StringComparison.Ordinal);
                return nameComparison != 0 ? nameComparison : this.Server.CompareTo(other.Server);
            }

            public int CompareTo(object? obj) {
                if (ReferenceEquals(null, obj)) {
                    return 1;
                }

                if (ReferenceEquals(this, obj)) {
                    return 0;
                }

                return obj is SenderPlayer other ? this.CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(SenderPlayer)}");
            }
        }
    }

    #endregion

    #region Shutdown

    public class ServerShutdown : IEncodable {
        public static ServerShutdown Instance { get; } = new ServerShutdown();

        [IgnoreMember]
        protected override byte Code => (byte)ServerOperation.Shutdown;

        protected override byte[] PayloadEncode() {
            return new byte[0];
        }
    }

    #endregion

    #region Player data

    [MessagePackObject]
    public class PlayerData : IEncodable {
        [Key(0)]
        public readonly string homeWorld;

        [Key(1)]
        public readonly string currentWorld;

        [Key(2)]
        public readonly string location;

        [Key(3)]
        public readonly string name;

        public PlayerData(string homeWorld, string currentWorld, string location, string name) {
            this.homeWorld = homeWorld;
            this.currentWorld = currentWorld;
            this.location = location;
            this.name = name;
        }

        [IgnoreMember]
        protected override byte Code => (byte)ServerOperation.PlayerData;

        public static PlayerData Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<PlayerData>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }
    }

    [MessagePackObject]
    public class EmptyPlayerData : IEncodable {
        public static EmptyPlayerData Instance { get; } = new EmptyPlayerData();

        [IgnoreMember]
        protected override byte Code => (byte)ServerOperation.PlayerData;

        protected override byte[] PayloadEncode() {
            return new byte[0];
        }
    }

    #endregion

    #region Availability

    [MessagePackObject]
    public class Availability : IEncodable {
        [Key(0)]
        public readonly bool available;

        public Availability(bool available) {
            this.available = available;
        }

        [IgnoreMember]
        protected override byte Code => (byte)ServerOperation.Availability;

        public static Availability Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<Availability>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }
    }

    #endregion

    #region Channel

    [MessagePackObject]
    public class ServerChannel : IEncodable {
        [Key(0)]
        public readonly byte channel;

        [Key(1)]
        public readonly string name;

        [IgnoreMember]
        public InputChannel InputChannel => (InputChannel)this.channel;

        protected override byte Code => (byte)ServerOperation.Channel;

        public ServerChannel(InputChannel channel, string name) : this((byte)channel, name) { }

        public ServerChannel(byte channel, string name) {
            this.channel = channel;
            this.name = name;
        }

        public static ServerChannel Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<ServerChannel>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }
    }

    #endregion

    #region Backlog

    [MessagePackObject]
    public class ServerBacklog : IEncodable {
        [Key(0)]
        public readonly ServerMessage[] messages;

        protected override byte Code => (byte)ServerOperation.Backlog;

        public ServerBacklog(ServerMessage[] messages) {
            this.messages = messages;
        }

        public static ServerBacklog Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<ServerBacklog>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }
    }

    #endregion

    #region Player list

    [MessagePackObject]
    public class ServerPlayerList : IEncodable {
        [Key(0)]
        public PlayerListType Type { get; set; }

        [Key(1)]
        public Player[] Players { get; set; }

        protected override byte Code => (byte)ServerOperation.PlayerList;

        public ServerPlayerList(PlayerListType type, Player[] players) {
            this.Type = type;
            this.Players = players;
        }

        public static ServerPlayerList Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<ServerPlayerList>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }
    }

    #endregion
}
