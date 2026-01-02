using System.Collections.Generic;
using MessagePack;

namespace XIVChatCommon.Message.Relay {
    [MessagePackObject]
    public class RelayNewClient : IFromRelay {
        [Key(0)]
        public List<byte> PublicKey { get; set; }

        [Key(1)]
        public string Address { get; set; }
    }
}