using MessagePack;

namespace XIVChatCommon.Message.Relay {
    [MessagePackObject]
    public class RelaySuccess : IFromRelay {
        [Key(0)]
        public bool Success { get; set; }

        [Key(1)]
        public string? Info { get; set; }
    }
}