using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class KitchenGameLobby : MonoBehaviour
{
    public static KitchenGameLobby Instance { get; private set; }

    private Lobby _joinedLobby;
    private float _heartbeatTimer;


    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private async void InitializeUnityAuthentication()
    {
        // You cannot initialize services more than once, and since this object will be destroyed on MainMenuScene cleanup
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            // Lobby works based on the player ID that the auth service generates, and multiple builds on the same PC will have the same ID's
            // To be able to test, initialize with different options each time 
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(Random.Range(0, 1000).ToString());

            await UnityServices.InitializeAsync(initializationOptions);


            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void Update()
    {
        // Normally after 30 secs of inactivity lobbies are unable to be found
        // Heartbeat is a periodic "i am still here" call
        HandleHeartbeat();
    }

    private void HandleHeartbeat()
    {
        if (IsLobbyHost())
        {
            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer < 0)
            {
                float heatbeatTimerMax = 15f;
                _heartbeatTimer = heatbeatTimerMax;

                LobbyService.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
            }
        }
    }

    private bool IsLobbyHost()
    {
        return _joinedLobby != null && _joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(
                lobbyName,
                KitchenGameMultiplayer.MAX_PLAYER_LIMIT,
                new CreateLobbyOptions
                {
                    IsPrivate = isPrivate
                });

            // After creating a lobby, start host
            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectionScene);

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void QuickJoin()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();

            // After joining, start client
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinLobbyWithCode(string lobbyCode)
    {
        try
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            // After joining, start client
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public Lobby GetLobby()
    {
        return _joinedLobby;
    }
}
