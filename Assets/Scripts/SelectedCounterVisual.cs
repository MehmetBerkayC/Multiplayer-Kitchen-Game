using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjects;

    private void Start()
    {
        // Changes on Multiplayer
        // Player.Instance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
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