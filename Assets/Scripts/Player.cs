using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float movementSpeed = 7f;
    [SerializeField] GameInput gameInput;

    private Vector2 inputVector = Vector2.zero;

    public bool IsWalking { get; private set; }

    private void Update()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 movementDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        IsWalking = movementDirection != Vector3.zero;

        // Position and Rotation
        transform.position += movementDirection * movementSpeed * Time.deltaTime;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, movementDirection, Time.deltaTime * rotateSpeed);
    }

}
