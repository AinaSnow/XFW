using MessagePack;

namespace XIVChatCommon.Message.Server {
    [MessagePackObject]
    public class Availability : Encodable {
        [Key(0)]
        public readonly bool available;

        public Availability(bool available) {
            this.available = available;
        }

        [IgnoreMember]
        protected override byte Code => (byte)ServerOperation.Availability;

        public static Availability Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<Availability>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }
    }
}