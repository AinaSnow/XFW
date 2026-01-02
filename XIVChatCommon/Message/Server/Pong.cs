using MessagePack;

namespace XIVChatCommon.Message.Server {
    public class Pong : Encodable {
        public static Pong Instance { get; } = new();

        [IgnoreMember]
        protected override byte Code => (byte)ServerOperation.Pong;

        protected override byte[] PayloadEncode() {
            return [];
        }
    }
}