using MessagePack;

namespace XIVChatCommon.Message.Client {
    public class ClientShutdown : Encodable {
        public static ClientShutdown Instance { get; } = new();

        [IgnoreMember]
        protected override byte Code => (byte) ClientOperation.Shutdown;

        protected override byte[] PayloadEncode() {
            return [];
        }
    }
}