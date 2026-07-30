using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("ëÄçÏê›íË")]
    [SerializeField]
    private float movementSpeed = 15f;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private Vector2 targetPosition;

    private bool canMove;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        targetPosition = rb.position;
    }

    private void Update()
    {
        if (!canMove || mainCamera == null)
        {
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(mousePosition);

        targetPosition = worldPosition;
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(
            rb.position,
            targetPosition,
            movementSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);
    }

    public void StartMoving()
    {
        canMove = true;
    }

    public void StopMoving()
    {
        canMove = false;
        rb.linearVelocity = Vector2.zero;
    }

    public void ResetPlayer(Vector2 position)
    {
        StopMoving();
        rb.position = position;
        targetPosition = position;
    }
}