using MessagePack;

namespace XIVChatCommon.Message.Relay {
    [Union(1, typeof(RelayRegister))]
    [Union(2, typeof(RelayedMessage))]
    [Union(3, typeof(RelayClientDisconnect))]
    public interface IToRelay {
    }

    [Union(1, typeof(RelaySuccess))]
    [Union(2, typeof(RelayNewClient))]
    [Union(3, typeof(RelayedMessage))]
    [Union(4, typeof(RelayClientDisconnect))]
    public interface IFromRelay {
    }
}
