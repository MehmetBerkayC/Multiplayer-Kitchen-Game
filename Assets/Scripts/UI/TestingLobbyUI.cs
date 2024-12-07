using UnityEngine;
using UnityEngine.UI;

public class TestingLobbyUI : MonoBehaviour
{
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;


    private void Awake()
    {
        createGameButton.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectionScene);
        });

        // Client doesn't need to load the scene because it will load the scene when the server loads it
        joinGameButton.onClick.AddListener(() => KitchenGameMultiplayer.Instance.StartClient());

    }
}
