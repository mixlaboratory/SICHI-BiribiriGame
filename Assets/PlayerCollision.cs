using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("参照")]
    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private GameManager gameManager;

    [Header("ダメージ")]
    [SerializeField]
    private int collisionDamage = 1;


    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth =
                GetComponent<PlayerHealth>();
        }

        if (playerController == null)
        {
            playerController =
                GetComponent<PlayerController>();
        }

        if (gameManager == null)
        {
            gameManager =
                FindFirstObjectByType<GameManager>();
        }
    }


    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        // Wallタグ以外は無視
        if (!collision.collider.CompareTag("Wall"))
        {
            return;
        }

        // ゲーム中以外はダメージなし
        if (gameManager != null &&
            !gameManager.IsPlaying)
        {
            return;
        }

        if (playerHealth == null)
        {
            return;
        }

        // 無敵時間中は追加ダメージなし
        if (playerHealth.IsInvincible)
        {
            return;
        }

        // =========================
        // ダメージ
        // =========================

        playerHealth.TakeDamage(
            collisionDamage
        );

        // =========================
        // Joy-Con振動
        // =========================

        if (playerController != null)
        {
            playerController.Rumble();
        }

        // HPが0ならGameOverになるので
        // ノックバックはしない
        if (playerHealth.CurrentHP <= 0)
        {
            return;
        }

        // =========================
        // ノックバック方向
        // =========================

        Vector2 knockbackDirection;

        if (collision.contactCount > 0)
        {
            ContactPoint2D contact =
                collision.GetContact(0);

            knockbackDirection =
                contact.normal;
        }
        else
        {
            // 念のため接触点が取れなかった場合
            knockbackDirection =
                (
                    transform.position -
                    collision.transform.position
                ).normalized;
        }

        // =========================
        // ノックバック
        // =========================

        if (playerController != null)
        {
            playerController.Knockback(
                knockbackDirection
            );
        }
    }


    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        // Goal
        if (other.CompareTag("Goal"))
        {
            if (gameManager != null)
            {
                gameManager.GameClear();
            }
        }
    }
}