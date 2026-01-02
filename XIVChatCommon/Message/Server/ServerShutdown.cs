using MessagePack;

namespace XIVChatCommon.Message.Server {
    public class ServerShutdown : Encodable {
        public static ServerShutdown Instance { get; } = new();

        [IgnoreMember]
        protected override byte Code => (byte)ServerOperation.Shutdown;

        protected override byte[] PayloadEncode() {
            return [];
        }
    }
}