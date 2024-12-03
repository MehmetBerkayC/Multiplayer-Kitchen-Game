using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjects;

    private void Start()
    {
        // Changes on Multiplayer
        if (Player.LocalInstance != null)
        {
            // Player might not spawn before this line is called so you need a static event
            Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        }
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }
    }
    private void Player_OnAnyPlayerSpawned(object sender, System.EventArgs e)
    {
        if (Player.LocalInstance != null)
        {
            // This logic will be called multiple times because with every player spawn it is invoked
            // previous spawns will trigger too,
            // so we make sure not to pile up event listeners
            Player.LocalInstance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
            Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        }
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if (e.selectedCounter == baseCounter)
        {
            Show();
        }
        else { Hide(); }
    }

    private void Show()
    {
        foreach (GameObject visual in visualGameObjects)
        {
            if (visual != null)
                visual.SetActive(true);
        }
    }

    private void Hide()
    {
        foreach (GameObject visual in visualGameObjects)
        {
            if (visual != null)
                visual.SetActive(false);
        }
    }
}