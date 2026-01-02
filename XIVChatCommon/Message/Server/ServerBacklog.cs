using MessagePack;

namespace XIVChatCommon.Message.Server {
    [MessagePackObject]
    public class ServerBacklog : Encodable {
        [Key(0)]
        public readonly ServerMessage[] messages;

        [Key(1)]
        public readonly uint sequence;

        protected override byte Code => (byte)ServerOperation.Backlog;

        public ServerBacklog(ServerMessage[] messages, uint sequence) {
            this.messages = messages;
            this.sequence = sequence;
        }

        public static ServerBacklog Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<ServerBacklog>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }
    }
}
