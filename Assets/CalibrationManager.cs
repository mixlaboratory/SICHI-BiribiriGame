using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CalibrationManager : MonoBehaviour
{
    [Header("計測時間")]
    [SerializeField]
    private float calibrationTime = 45f;

    [Header("UI")]
    [SerializeField]
    private TMP_Text countdownText;

    [SerializeField]
    private TMP_Text statusText;

    [SerializeField]
    private Image progressFill;

    [Header("次のシーン")]
    [SerializeField]
    private string nextSceneName = "SampleScene";

    private float remainingTime;
    private bool isCalibrating;

    private void Start()
    {
        remainingTime = calibrationTime;
        isCalibrating = true;

        if (statusText != null)
        {
            statusText.text =
                "基準値を取得中…終了までお待ちください";
        }

        UpdateUI();
    }

    private void Update()
    {
        if (!isCalibrating)
        {
            return;
        }

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isCalibrating = false;

            UpdateUI();

            if (statusText != null)
            {
                statusText.text = "計測完了";
            }

            SceneManager.LoadScene(nextSceneName);
            return;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        int seconds =
            Mathf.CeilToInt(remainingTime);

        if (countdownText != null)
        {
            countdownText.text =
                $"00:{seconds:00}";
        }

        if (progressFill != null)
        {
            float progress =
                1f -
                (remainingTime / calibrationTime);

            progressFill.fillAmount =
                progress;
        }
    }
}