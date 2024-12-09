using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] MeshRenderer headMeshRenderer;
    [SerializeField] MeshRenderer bodyMeshRenderer;

    private Material _material;

    private void Awake()
    {
        // Cloning material in real time so that each player has a different material/color
        _material = new Material(headMeshRenderer.material);

        headMeshRenderer.material = _material;
        bodyMeshRenderer.material = _material;
    }

    public void SetPlayerColor(Color color)
    {
        _material.color = color;
    }
}
