using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;

    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }

    [SerializeField] private Transform playerPrefab;

    private NetworkVariable<State> _state = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<float> _countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> _gamePlayingTimer = new NetworkVariable<float>(300f);
    private float _gamePlayingTimerMax = 300f;

    private bool _isLocalPlayerReady;
    private bool _isLocalGamePaused = false;

    private NetworkVariable<bool> _isGamePaused = new NetworkVariable<bool>(false);

    private Dictionary<ulong, bool> _playerReadyDictionary;
    private Dictionary<ulong, bool> _playerPausedDictionary;

    private bool _autoTestGamePausedState = false;

    private void Awake()
    {
        Instance = this;

        _playerReadyDictionary = new Dictionary<ulong, bool>();
        _playerPausedDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        _state.OnValueChanged += State_OnValueChanged;
        _isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

            // Spawning players
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        // If a player pauses and disconnects, check remaining players and move on
        // BUT!! The client will still be on the connectedClient list on the NetworkManager because
        // while this callback is invoked the client is still connected
        // So, we just need to wait for 1 more frame until the player actually disconnects to check
        _autoTestGamePausedState = true;
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if (_isGamePaused.Value)
        {
            Time.timeScale = 0f;
            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (_state.Value == State.WaitingToStart)
        {
            _isLocalPlayerReady = true;

            OnLocalPlayerReadyChanged?.Invoke(this, new EventArgs());

            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
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
            _state.Value = State.CountdownToStart;
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        if (!IsServer) return; // Only the server will run the code below

        switch (_state.Value)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                _countdownToStartTimer.Value -= Time.deltaTime;
                if (_countdownToStartTimer.Value < 0f)
                {
                    _state.Value = State.GamePlaying;
                    _gamePlayingTimer.Value = _gamePlayingTimerMax;
                }
                break;
            case State.GamePlaying:
                _gamePlayingTimer.Value -= Time.deltaTime;
                if (_gamePlayingTimer.Value < 0f)
                {
                    _state.Value = State.GameOver;
                }
                break;
            case State.GameOver:
                break;
        }
        //Debug.Log(_state);
    }

    private void LateUpdate()
    {
        if (_autoTestGamePausedState)
        {
            _autoTestGamePausedState = false;
            TestGamePausedState();
        }
    }

    public bool IsGamePlaying()
    {
        return _state.Value == State.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return _state.Value == State.CountdownToStart;
    }

    public bool IsGameOver()
    {
        return _state.Value == State.GameOver;
    }

    public bool IsLocalPlayerReady()
    {
        return _isLocalPlayerReady;
    }

    public bool IsWaitingToStart()
    {
        return _state.Value == State.WaitingToStart;
    }

    public float GetCountdownToStartTimer()
    {
        return _countdownToStartTimer.Value;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return _gamePlayingTimer.Value / _gamePlayingTimerMax; // Timer Counts DOWN
    }

    public void TogglePauseGame()
    {
        _isLocalGamePaused = !_isLocalGamePaused;
        if (_isLocalGamePaused)
        {
            //Time.timeScale = 0f;
            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            //Time.timeScale = 1f;
            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
        // Could just remove timescale from the code above to remove collective pausing
        // Only the local player would see the pause menu to change settings, quit etc. without pausing other players
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        _playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;

        TestGamePausedState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        _playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;

        TestGamePausedState();
    }

    private void TestGamePausedState()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (_playerPausedDictionary.ContainsKey(clientId) && _playerPausedDictionary[clientId])
            {
                // Player paused
                _isGamePaused.Value = true;
                return;
            }
        }

        // All players are unpaused
        _isGamePaused.Value = false;
    }
}
