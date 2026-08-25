using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("ゲームオブジェクト")]
    [SerializeField]
    private PlayerController player;

    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private Transform startPoint;

    [Header("UI")]
    [SerializeField]
    private Text timerText;

    [SerializeField]
    private Text statusText;

    private bool isPlaying;
    private float elapsedTime;

    public float ElapsedTime => elapsedTime;
    public bool IsPlaying => isPlaying;

    private void Start()
    {
        ResetGame();
    }

    private void Update()
    {
        if (isPlaying)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerText();
        }

        // SPACEキーでゲーム開始
        if (Input.GetKeyDown(KeyCode.Space) && !isPlaying)
        {
            StartGame();
        }

        // Rキーでリセット
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
    }

    public void StartGame()
    {
        if (player == null)
        {
            Debug.LogError(
                "GameManagerにPlayerが設定されていません。"
            );

            return;
        }

        // HPが0の状態ではSPACEで再開できない
        // Rキーでリセットしてから開始する
        if (playerHealth != null &&
            playerHealth.CurrentHP <= 0)
        {
            SetStatusText(
                "Rキーでリトライ"
            );

            return;
        }

        elapsedTime = 0f;
        isPlaying = true;

        player.StartMoving();

        SetStatusText("");
        UpdateTimerText();

        Debug.Log("ゲーム開始");
    }

    public void GameOver()
    {
        if (!isPlaying)
        {
            return;
        }

        isPlaying = false;

        if (player != null)
        {
            player.StopMoving();
        }

        SetStatusText(
            "ゲームオーバー！\n" +
            "Rキーでリトライ"
        );

        Debug.Log("ゲームオーバー");
    }

    public void GameClear()
    {
        if (!isPlaying)
        {
            return;
        }

        isPlaying = false;

        if (player != null)
        {
            player.StopMoving();
        }

        SetStatusText(
            "クリア！\n" +
            "タイム：" +
            elapsedTime.ToString("F2") +
            "秒\n" +
            "Rキーでリトライ"
        );

        Debug.Log(
            "クリア！ タイム：" +
            elapsedTime.ToString("F2") +
            "秒"
        );
    }

    public void ResetGame()
    {
        isPlaying = false;
        elapsedTime = 0f;

        // Playerをスタート位置へ戻す
        if (player != null && startPoint != null)
        {
            player.ResetPlayer(
                startPoint.position
            );
        }
        else
        {
            Debug.LogWarning(
                "PlayerまたはStartPointが設定されていません。"
            );
        }

        // HPを最大まで回復
        if (playerHealth != null)
        {
            playerHealth.ResetHealth();
        }
        else
        {
            Debug.LogWarning(
                "PlayerHealthが設定されていません。"
            );
        }

        UpdateTimerText();

        SetStatusText(
            "SPACEキーでスタート"
        );

        Debug.Log(
            "SPACEキーでスタート"
        );
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text =
                "TIME： " +
                elapsedTime.ToString("F2");
        }
    }

    private void SetStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}