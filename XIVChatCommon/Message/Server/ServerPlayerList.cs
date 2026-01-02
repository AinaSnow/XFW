using MessagePack;

namespace XIVChatCommon.Message.Server {
    [MessagePackObject]
    public class ServerPlayerList : Encodable {
        [Key(0)]
        public PlayerListType Type { get; set; }

        [Key(1)]
        public Player[] Players { get; set; }

        protected override byte Code => (byte) ServerOperation.PlayerList;

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
}
