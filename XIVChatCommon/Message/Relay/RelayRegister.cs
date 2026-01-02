using MessagePack;

namespace XIVChatCommon.Message.Relay {
    [MessagePackObject]
    public class RelayRegister : IToRelay {
        [Key(0)]
        public string AuthToken { get; set; }

        [Key(1)]
        public byte[] PublicKey { get; set; }
    }
}