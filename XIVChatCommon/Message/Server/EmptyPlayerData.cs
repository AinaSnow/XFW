using MessagePack;

namespace XIVChatCommon.Message.Server {
    [MessagePackObject]
    public class EmptyPlayerData : Encodable {
        public static EmptyPlayerData Instance { get; } = new();

        [IgnoreMember]
        protected override byte Code => (byte)ServerOperation.PlayerData;

        protected override byte[] PayloadEncode() {
            return [];
        }
    }
}