using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(
            "衝突：" +
            collision.collider.gameObject.name +
            " / Tag：" +
            collision.collider.tag
        );

        if (collision.collider.CompareTag("Wall"))
        {
            gameManager?.GameOver();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Goal"))
        {
            gameManager?.GameClear();
            return;
        }

        if (other.CompareTag("Hazard"))
        {
            gameManager?.GameOver();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // プレイヤーがレーザー位置にいる最中に
        // レーザーが点灯した場合にも失敗させる
        if (other.CompareTag("Hazard"))
        {
            gameManager?.GameOver();
        }
    }
}