using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("ゲームオブジェクト")]
    [SerializeField]
    private PlayerController player;

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

        if (Input.GetKeyDown(KeyCode.Space) && !isPlaying)
        {
            StartGame();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
    }

    public void StartGame()
    {
        if (player == null)
        {
            Debug.LogError("Playerが設定されていません。");
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

        SetStatusText("失敗！\nRキーでリトライ");

        Debug.Log("失敗しました。Rキーでリトライ");
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
            "タイム：" + elapsedTime.ToString("F2") + "秒\n" +
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

        if (player != null && startPoint != null)
        {
            player.ResetPlayer(startPoint.position);
        }
        else
        {
            Debug.LogWarning(
                "PlayerまたはStartPointが設定されていません。"
            );
        }

        UpdateTimerText();
        SetStatusText("SPACEキーでスタート");

        Debug.Log("SPACEキーでスタート");
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