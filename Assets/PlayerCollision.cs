using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private int collisionDamage = 1;

    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth =
                GetComponent<PlayerHealth>();
        }
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (collision.collider.CompareTag("Wall"))
        {
            playerHealth?.TakeDamage(
                collisionDamage
            );
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (other.CompareTag("Goal"))
        {
            GameManager gameManager =
                FindFirstObjectByType<GameManager>();

            gameManager?.GameClear();
        }
    }
}