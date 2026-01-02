using MessagePack;

namespace XIVChatCommon.Message.Client {
    [MessagePackObject]
    public class ClientBacklog : Encodable {
        [Key(0)]
        public ushort Amount { get; set; }

        protected override byte Code => (byte) ClientOperation.Backlog;

        public static ClientBacklog Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<ClientBacklog>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }
    }
}