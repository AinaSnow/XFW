using System;
using System.Collections.Generic;
using System.Reflection;
using MessagePack;

namespace XIVChatCommon.Message.Client {
    public enum ClientOperation : byte {
        Ping = 1,
        Message = 2,
        Shutdown = 3,
        Backlog = 4,
        CatchUp = 5,
        PlayerList = 6,
        LinkshellList = 7,
        Preferences = 8,
    }

    #region Ping

    public class Ping : IEncodable {
        public static Ping Instance { get; } = new Ping();

        [IgnoreMember]
        protected override byte Code => (byte)ClientOperation.Ping;

        protected override byte[] PayloadEncode() {
            return new byte[0];
        }
    }

    #endregion

    #region Message

    [MessagePackObject]
    public class ClientMessage : IEncodable {
        [Key(0)]
        public string Content { get; set; }

        [IgnoreMember]
        protected override byte Code => (byte)ClientOperation.Message;

        public ClientMessage(string content) {
            this.Content = content;
        }

        public static ClientMessage Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<ClientMessage>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }
    }

    #endregion

    #region Shutdown

    public class ClientShutdown : IEncodable {
        public static ClientShutdown Instance { get; } = new ClientShutdown();

        [IgnoreMember]
        protected override byte Code => (byte)ClientOperation.Shutdown;

        protected override byte[] PayloadEncode() {
            return new byte[0];
        }
    }

    #endregion

    #region Backlog/catch-up

    [MessagePackObject]
    public class ClientBacklog : IEncodable {
        [Key(0)]
        public ushort Amount { get; set; }

        protected override byte Code => (byte)ClientOperation.Backlog;

        public static ClientBacklog Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<ClientBacklog>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }
    }

    [MessagePackObject]
    public class ClientCatchUp : IEncodable {
        [MessagePackFormatter(typeof(MillisecondsDateTimeFormatter))]
        [Key(0)]
        public DateTime After { get; set; }

        protected override byte Code => (byte)ClientOperation.CatchUp;

        public ClientCatchUp(DateTime after) {
            this.After = after;
        }

        public static ClientCatchUp Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<ClientCatchUp>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }
    }

    #endregion

    #region Player list

    [MessagePackObject]
    public class ClientPlayerList : IEncodable {
        [Key(0)]
        public PlayerListType Type { get; set; }

        protected override byte Code => (byte)ClientOperation.PlayerList;

        public static ClientPlayerList Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<ClientPlayerList>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }
    }

    #endregion

    #region Preferences

    [MessagePackObject]
    public class ClientPreferences : IEncodable {
        [Key(0)]
        public Dictionary<ClientPreference, object> Preferences { get; set; } = new Dictionary<ClientPreference, object>();

        protected override byte Code => (byte)ClientOperation.Preferences;

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }

        public static ClientPreferences Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<ClientPreferences>(bytes);
        }
    }

    public enum ClientPreference {
        [Preference(typeof(bool))]
        BacklogNewestMessagesFirst,
    }

    public static class ClientPreferencesExtension {
        public static bool TryGetValue<T>(this ClientPreferences prefs, ClientPreference pref, out T value) {
            value = default!;

            if (!prefs.Preferences.TryGetValue(pref, out var obj)) {
                return false;
            }

            var attr = pref
                .GetType()
                .GetField(pref.ToString())
                ?.GetCustomAttribute<PreferenceAttribute>(false);

            if (obj.GetType() != typeof(T) || obj.GetType() != attr?.ValueType) {
                return false;
            }

            value = (T)obj;
            return true;
        }
    }

    public class PreferenceAttribute : Attribute {
        public Type ValueType { get; }

        public PreferenceAttribute(Type valueType) {
            this.ValueType = valueType;
        }
    }

    #endregion
}
