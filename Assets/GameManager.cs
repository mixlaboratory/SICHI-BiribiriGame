using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("�Q�[���I�u�W�F�N�g")]
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

    [SerializeField]
    private IrritationManager irritationManager;

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

        // SPACE�L�[�ŃQ�[���J�n
        if (Input.GetKeyDown(KeyCode.Space) && !isPlaying)
        {
            StartGame();
        }

        // R�L�[�Ń��Z�b�g
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

        // HP��0�̏�Ԃł�SPACE�ōĊJ�ł��Ȃ�
        // R�L�[�Ń��Z�b�g���Ă���J�n����
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

        Debug.Log("�Q�[���J�n");
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

        GameResultData.ClearTime = elapsedTime;

        GameResultData.ClearTime = elapsedTime;
        GameResultData.HasResult = true;

        SceneManager.LoadScene("RankingScene");


        SceneManager.LoadScene("RankingScene");
    }

    public void ResetGame()
    {
        isPlaying = false;
        elapsedTime = 0f;

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

        if (irritationManager != null)
        {
            irritationManager.ResetIrritation();
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
                "TIME: " +
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