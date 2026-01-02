using System.Collections.Generic;
using MessagePack;

namespace XIVChatCommon.Message.Relay {
    [MessagePackObject]
    public class RelayedMessage : IFromRelay, IToRelay {
        [Key(0)]
        public List<byte> PublicKey { get; set; }

        [Key(1)]
        public List<byte> Message { get; set; }
    }
}