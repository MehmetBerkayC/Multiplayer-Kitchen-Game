using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float movementSpeed = 7f;

    private Vector2 inputVector = Vector2.zero;

    public bool IsWalking { get; private set; }

    private void Update()
    {
        inputVector = Vector2.zero;

        // Player Inputs
        if (Input.GetKey(KeyCode.W))
        {
            inputVector.y = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputVector.y = -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputVector.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputVector.x = 1;
        }

        // Uniform movement
        inputVector = inputVector.normalized;

        Vector3 movementDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        IsWalking = movementDirection != Vector3.zero;

        // Position and Rotation
        transform.position += movementDirection * movementSpeed * Time.deltaTime;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, movementDirection, Time.deltaTime * rotateSpeed);
    }

}
