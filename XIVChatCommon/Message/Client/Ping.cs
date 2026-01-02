using MessagePack;

namespace XIVChatCommon.Message.Client {
    public class Ping : Encodable {
        public static Ping Instance { get; } = new();

        [IgnoreMember]
        protected override byte Code => (byte) ClientOperation.Ping;

        protected override byte[] PayloadEncode() {
            return [];
        }
    }
}