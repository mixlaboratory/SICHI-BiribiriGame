using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RotatingObstacle : MonoBehaviour
{
    [Header("âÒì]ê›íË")]
    [Tooltip("1ïbä‘Ç…âÒì]Ç∑ÇÈäpìx")]
    [SerializeField]
    private float rotationSpeed = 90f;

    [Tooltip("ONÇ»ÇÁéûåvâÒÇË")]
    [SerializeField]
    private bool clockwise = true;

    [Header("ÉQÅ[ÉÄÇ∆ÇÃòAåg")]
    [SerializeField]
    private GameManager gameManager;

    [Tooltip("ÉQÅ[ÉÄäJénëOÇ‚èIóπå„Ç‡âÒì]Ç≥ÇπÇÈ")]
    [SerializeField]
    private bool rotateOutsideGame = false;

    [Tooltip("ÉQÅ[ÉÄí‚é~éûÇ…ç≈èâÇÃäpìxÇ÷ñﬂÇ∑")]
    [SerializeField]
    private bool returnToStartWhenStopped = true;

    [Header("ìÆçÏèÛë‘")]
    [SerializeField]
    private bool rotationEnabled = true;

    private Rigidbody2D rb;
    private float startRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startRotation = rb.rotation;

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    private void FixedUpdate()
    {
        bool gameAllowsRotation =
            rotateOutsideGame ||
            gameManager == null ||
            gameManager.IsPlaying;

        bool canRotate =
            rotationEnabled &&
            gameAllowsRotation;

        if (canRotate)
        {
            Rotate();
        }
        else if (returnToStartWhenStopped)
        {
            ReturnToStartRotation();
        }
    }

    private void Rotate()
    {
        float direction = clockwise ? -1f : 1f;

        float nextRotation =
            rb.rotation +
            direction *
            rotationSpeed *
            Time.fixedDeltaTime;

        rb.MoveRotation(nextRotation);
    }

    private void ReturnToStartRotation()
    {
        float nextRotation = Mathf.MoveTowardsAngle(
            rb.rotation,
            startRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(nextRotation);
    }

    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = Mathf.Max(0f, newSpeed);
    }

    public void SetClockwise(bool value)
    {
        clockwise = value;
    }

    public void SetRotationEnabled(bool enabled)
    {
        rotationEnabled = enabled;
    }

    public void ResetObstacle()
    {
        rb.SetRotation(startRotation);
    }
}