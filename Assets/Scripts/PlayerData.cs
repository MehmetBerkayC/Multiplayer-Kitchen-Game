using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong ClientID;
    public int ColorID;
    public FixedString64Bytes PlayerName; // Cant use nullable types
    public FixedString64Bytes PlayerID; // Cant use nullable types

    public bool Equals(PlayerData other)
    {
        return ClientID == other.ClientID
            && ColorID == other.ColorID
            && PlayerName == other.PlayerName
            && PlayerID == other.PlayerID;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientID);
        serializer.SerializeValue(ref ColorID);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref PlayerID);
    }
}
