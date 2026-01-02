using MessagePack;

namespace XIVChatCommon.Message.Server {
    [MessagePackObject]
    public class PlayerData : Encodable {
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
}
