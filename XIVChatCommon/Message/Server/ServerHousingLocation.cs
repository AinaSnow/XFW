using MessagePack;

namespace XIVChatCommon.Message.Server {
    [MessagePackObject]
    public class ServerHousingLocation : Encodable {
        [Key(0)]
        public readonly ushort? ward;

        [Key(1)]
        public readonly ushort? plot;

        [Key(2)]
        public readonly bool plotExterior;

        [Key(3)]
        public readonly byte? apartmentWing;

        public ServerHousingLocation(ushort? ward, ushort? plot, bool plotExterior, byte? apartmentWing) {
            this.ward = ward;
            this.plot = plot;
            this.plotExterior = plotExterior;
            this.apartmentWing = apartmentWing;
        }

        [IgnoreMember]
        protected override byte Code => (byte) ServerOperation.HousingLocation;

        public static ServerHousingLocation Decode(byte[] bytes) {
            return MessagePackSerializer.Deserialize<ServerHousingLocation>(bytes);
        }

        protected override byte[] PayloadEncode() {
            return MessagePackSerializer.Serialize(this);
        }

        public override bool Equals(object? obj) {
            if (obj is not ServerHousingLocation other) {
                return false;
            }

            return this.ward == other.ward
                   && this.plot == other.plot
                   && this.plotExterior == other.plotExterior
                   && this.apartmentWing == other.apartmentWing;
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.ward.GetHashCode();
                hashCode = (hashCode * 397) ^ this.plot.GetHashCode();
                hashCode = (hashCode * 397) ^ this.plotExterior.GetHashCode();
                hashCode = (hashCode * 397) ^ this.apartmentWing.GetHashCode();
                return hashCode;
            }
        }
    }
}
