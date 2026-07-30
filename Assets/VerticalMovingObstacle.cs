using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class VerticalMovingObstacle : MonoBehaviour
{
    [Header("à⁄ìÆîÕàÕ")]
    [SerializeField]
    private Transform topPoint;

    [SerializeField]
    private Transform bottomPoint;

    [Header("à⁄ìÆê›íË")]
    [SerializeField]
    private float moveSpeed = 2f;

    [SerializeField]
    private bool startMovingUp = true;

    [Header("ÉQÅ[ÉÄä«óù")]
    [SerializeField]
    private GameManager gameManager;

    [Tooltip("ÉQÅ[ÉÄäJénëOÇ‚é∏îså„Ç‡ìÆÇ©Ç∑èÍçáÇÕON")]
    [SerializeField]
    private bool moveOutsideGame = false;

    [Header("í‚é~éû")]
    [Tooltip("ÉQÅ[ÉÄí‚é~íÜÇ…èâä˙à íuÇ÷ñﬂÇ∑")]
    [SerializeField]
    private bool returnToStartWhenStopped = true;

    private Rigidbody2D rb;
    private Vector2 startPosition;
    private bool movingUp;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        startPosition = rb.position;
        movingUp = startMovingUp;

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    private void FixedUpdate()
    {
        if (topPoint == null || bottomPoint == null)
        {
            return;
        }

        bool canMove =
            moveOutsideGame ||
            gameManager == null ||
            gameManager.IsPlaying;

        if (!canMove)
        {
            if (returnToStartWhenStopped)
            {
                ReturnToStart();
            }

            return;
        }

        MoveObstacle();
    }

    private void MoveObstacle()
    {
        Vector2 targetPosition;

        if (movingUp)
        {
            targetPosition = topPoint.position;
        }
        else
        {
            targetPosition = bottomPoint.position;
        }

        Vector2 nextPosition = Vector2.MoveTowards(
            rb.position,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);

        float distance =
            Vector2.Distance(rb.position, targetPosition);

        if (distance <= 0.02f)
        {
            movingUp = !movingUp;
        }
    }

    private void ReturnToStart()
    {
        Vector2 nextPosition = Vector2.MoveTowards(
            rb.position,
            startPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);
        movingUp = startMovingUp;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
    }

    public void SetMoving(bool enabled)
    {
        moveOutsideGame = enabled;
    }

    public void ResetObstacle()
    {
        rb.position = startPosition;
        movingUp = startMovingUp;
    }

    private void OnDrawGizmos()
    {
        if (topPoint == null || bottomPoint == null)
        {
            return;
        }

        Gizmos.DrawLine(
            topPoint.position,
            bottomPoint.position
        );

        Gizmos.DrawWireSphere(
            topPoint.position,
            0.1f
        );

        Gizmos.DrawWireSphere(
            bottomPoint.position,
            0.1f
        );
    }
}