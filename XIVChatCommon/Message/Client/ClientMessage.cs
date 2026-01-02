using MessagePack;

namespace XIVChatCommon.Message.Client {
    [MessagePackObject]
    public class ClientMessage : Encodable {
        [Key(0)]
        public string Content { get; set; }

        [IgnoreMember]
        protected override byte Code => (byte) ClientOperation.Message;

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
}