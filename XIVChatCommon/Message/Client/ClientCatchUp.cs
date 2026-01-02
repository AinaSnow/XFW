using System;
using MessagePack;

namespace XIVChatCommon.Message.Client {
    [MessagePackObject]
    public class ClientCatchUp : Encodable {
        [MessagePackFormatter(typeof(MillisecondsDateTimeFormatter))]
        [Key(0)]
        public DateTime After { get; set; }

        protected override byte Code => (byte) ClientOperation.CatchUp;

        public ClientCatchUp(DateTime after) {
            this.After = after;
        }

        public static ClientCatchUp Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<ClientCatchUp>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }
    }
}