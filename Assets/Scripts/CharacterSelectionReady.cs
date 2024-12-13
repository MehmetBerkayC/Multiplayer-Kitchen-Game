using System;
using System.Collections.Generic;
using Unity.Netcode;

public class CharacterSelectionReady : NetworkBehaviour
{
    public static CharacterSelectionReady Instance { get; private set; }

    public event EventHandler OnReadyChanged;

    private Dictionary<ulong, bool> _playerReadyDictionary; // Not a NetworkVariable

    private void Awake()
    {
        Instance = this;

        _playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // The dictionary to set player ready state is not synced - not a network variable
        // Ready game object on CharacterSelectionPlayer needs to know who set themselves as ready
        // This client rpc is required to sync that data
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);

        _playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;


        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // Player is not ready or doesn't exist in the dictionary yet
            if (!_playerReadyDictionary.ContainsKey(clientId) || !_playerReadyDictionary[clientId])
            {
                // Player is not ready
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            KitchenGameLobby.Instance.DeleteLobby();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        _playerReadyDictionary[clientId] = true;

        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        // Check if clientId is valid then get the ready value
        return _playerReadyDictionary.ContainsKey(clientId) && _playerReadyDictionary[clientId];
    }
}
