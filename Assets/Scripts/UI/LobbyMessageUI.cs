using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button returnButton;

    private void Awake()
    {
        returnButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        KitchenGameMultiplayer.Instance.OnFailedToJoinGame += KitchenGameMultiplayer_OnFailedToJoinGame;
        KitchenGameLobby.Instance.OnCreateLobbyStarted += KitchenGameLobby_OnCreateLobbyStarted;
        KitchenGameLobby.Instance.OnCreateLobbyFailed += KitchenGameLobby_OnCreateLobbyFailed;
        KitchenGameLobby.Instance.OnJoinStarted += KitchenGameLobby_OnJoinStarted;
        KitchenGameLobby.Instance.OnQuickJoinFailed += KitchenGameLobby_OnQuickJoinFailed;
        KitchenGameLobby.Instance.OnJoinFailed += KitchenGameLobby_OnJoinFailed;
        KitchenGameLobby.Instance.OnJoinWithCodeMissingCode += KitchenGameLobby_OnJoinWithCodeMissingCode; ;


        Hide();
    }

    private void KitchenGameLobby_OnJoinWithCodeMissingCode(object sender, EventArgs e)
    {
        ShowMessage("Lobby code is missing or written wrong!");
    }

    private void KitchenGameLobby_OnJoinFailed(object sender, EventArgs e)
    {
        ShowMessage("Failed to join The Lobby!");
    }

    private void KitchenGameLobby_OnQuickJoinFailed(object sender, EventArgs e)
    {
        ShowMessage("Could not find a Lobby to Quick Join!");

    }

    private void KitchenGameLobby_OnJoinStarted(object sender, EventArgs e)
    {
        ShowMessage("Joining a Lobby...");
    }

    private void KitchenGameLobby_OnCreateLobbyFailed(object sender, EventArgs e)
    {
        ShowMessage("Failed to create The Lobby...");
    }

    private void KitchenGameLobby_OnCreateLobbyStarted(object sender, EventArgs e)
    {
        ShowMessage("Creating The Lobby...");
    }

    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnFailedToJoinGame -= KitchenGameMultiplayer_OnFailedToJoinGame;
    }

    private void KitchenGameMultiplayer_OnFailedToJoinGame(object sender, EventArgs e)
    {
        if (NetworkManager.Singleton.DisconnectReason == "")
        {
            ShowMessage("Failed to connect!");
        }
        else
        {
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
        }
        Show();

        messageText.text = NetworkManager.Singleton.DisconnectReason;

        if (messageText.text == "") // if the text comes back empty, client might have been timed out
        {
            //Change attempts using MaxConnectionAttempts setting in NetworkManager
            //Total Attempt Time that will be spent is ConnectTimeoutMS * MaxConnectionAttempts
            messageText.text = "Failed to connect!";
        }
    }

    private void ShowMessage(string message)
    {
        //Change attempts using MaxConnectionAttempts setting in NetworkManager
        //Total Attempt Time that will be spent is ConnectTimeoutMS * MaxConnectionAttempts

        // if the text comes back empty, client might have been timed out
        Show();
        messageText.text = message;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
