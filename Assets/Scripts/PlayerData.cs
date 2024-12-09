using System;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong ClientID;
    public int ColorID;

    public bool Equals(PlayerData other)
    {
        return ClientID == other.ClientID
            && ColorID == other.ColorID;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientID);
        serializer.SerializeValue(ref ColorID);
    }
}
