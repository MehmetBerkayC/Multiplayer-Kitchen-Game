using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float movementSpeed = 7f;
    [SerializeField] GameInput gameInput;

    public bool IsWalking { get; private set; }

    private void Update()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 movementDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        float movementDistance = movementSpeed * Time.deltaTime;
        float PlayerRadius = .7f;
        float playerHeight = 2f;

        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, PlayerRadius, movementDirection, movementDistance);

        if (!canMove) // Cant move towards movementDirection
        {
            // Attempt only X movement
            Vector3 moveDirX = new Vector3(movementDirection.x, 0f, 0f).normalized; // standardize input to 1 (comment to see difference)
            canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, PlayerRadius, moveDirX, movementDistance);
            if (canMove)
            {
                // can only move on X
                movementDirection = moveDirX;
            }
            else // cannot move only on X
            {
                //Attempt only Z movement
                Vector3 moveDirZ = new Vector3(0f, 0f, movementDirection.z).normalized;
                canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, PlayerRadius, moveDirZ, movementDistance);
                if (canMove)
                {
                    // can only move on Y
                    movementDirection = moveDirZ;
                }
                else
                {
                    //Cannot move on any direction
                }
            }
        }

        if (canMove)
        {
            transform.position += movementDirection * movementDistance;
        }

        IsWalking = movementDirection != Vector3.zero;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, movementDirection, Time.deltaTime * rotateSpeed);
    }

}
