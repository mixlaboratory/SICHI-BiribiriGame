using System;
using UnityEngine;
using UnityEngine.UI;

public class IrritationManager : MonoBehaviour
{
    [Header("自動テスト")]
    [Tooltip("3秒ごとにイライラ度を自動変更する")]
    [SerializeField]
    private bool autoChange = true;

    [Tooltip("値を変更する間隔")]
    [SerializeField]
    private float changeInterval = 3f;

    [Tooltip("ランダム値の最小値")]
    [SerializeField, Range(0f, 100f)]
    private float minimumIrritation = 0f;

    [Tooltip("ランダム値の最大値")]
    [SerializeField, Range(0f, 100f)]
    private float maximumIrritation = 100f;

    [Tooltip("ゲーム中だけ自動変更する")]
    [SerializeField]
    private bool changeOnlyWhilePlaying = true;

    [Header("値の変化")]
    [SerializeField, Range(0f, 100f)]
    private float initialIrritation = 0f;

    [Tooltip("現在値が目標値へ近づく速さ")]
    [SerializeField]
    private float responseSpeed = 40f;

    [Header("UI")]
    [SerializeField]
    private Text irritationText;

    [SerializeField]
    private Image irritationFill;

    [Header("参照")]
    [SerializeField]
    private GameManager gameManager;

    public event Action<float> OnIrritationChanged;

    private float targetIrritation;
    private float currentIrritation;
    private float changeTimer;
    private float lastNotifiedValue = -1f;

    public float CurrentIrritation => currentIrritation;

    public float NormalizedIrritation =>
        currentIrritation / 100f;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        initialIrritation = Mathf.Clamp(
            initialIrritation,
            0f,
            100f
        );

        currentIrritation = initialIrritation;
        targetIrritation = initialIrritation;
        changeTimer = changeInterval;

        UpdateUI();
    }

    private void Update()
    {
        UpdateAutomaticChange();
        UpdateCurrentValue();
    }

    private void UpdateAutomaticChange()
    {
        if (!autoChange)
        {
            return;
        }

        bool canChange =
            !changeOnlyWhilePlaying ||
            gameManager == null ||
            gameManager.IsPlaying;

        if (!canChange)
        {
            changeTimer = changeInterval;
            return;
        }

        changeTimer -= Time.deltaTime;

        if (changeTimer <= 0f)
        {
            SetRandomIrritation();
            changeTimer = changeInterval;
        }
    }

    private void UpdateCurrentValue()
    {
        currentIrritation = Mathf.MoveTowards(
            currentIrritation,
            targetIrritation,
            responseSpeed * Time.deltaTime
        );

        if (Mathf.Abs(
                currentIrritation - lastNotifiedValue
            ) < 0.05f)
        {
            return;
        }

        UpdateUI();

        OnIrritationChanged?.Invoke(
            currentIrritation
        );

        lastNotifiedValue = currentIrritation;
    }

    private void SetRandomIrritation()
    {
        float minimum = Mathf.Min(
            minimumIrritation,
            maximumIrritation
        );

        float maximum = Mathf.Max(
            minimumIrritation,
            maximumIrritation
        );

        targetIrritation = UnityEngine.Random.Range(
            minimum,
            maximum
        );

        Debug.Log(
            "イライラ度の目標値を変更：" +
            Mathf.RoundToInt(targetIrritation)
        );
    }

    private void UpdateUI()
    {
        float normalized =
            currentIrritation / 100f;

        if (irritationText != null)
        {
            irritationText.text =
                "イライラ度：" +
                Mathf.RoundToInt(currentIrritation);
        }

        if (irritationFill != null)
        {
            irritationFill.fillAmount = normalized;
            irritationFill.color =
                GetGaugeColor(normalized);
        }
    }

    private Color GetGaugeColor(float normalized)
    {
        if (normalized < 0.5f)
        {
            return Color.Lerp(
                Color.green,
                Color.yellow,
                normalized * 2f
            );
        }

        return Color.Lerp(
            Color.yellow,
            Color.red,
            (normalized - 0.5f) * 2f
        );
    }

    /// <summary>
    /// 外部から0～100の値を設定する。
    /// </summary>
    public void SetIrritation(float value)
    {
        targetIrritation = Mathf.Clamp(
            value,
            0f,
            100f
        );
    }

    /// <summary>
    /// 外部から0～1の値を設定する。
    /// </summary>
    public void SetNormalizedIrritation(float value)
    {
        SetIrritation(
            Mathf.Clamp01(value) * 100f
        );
    }

    public void SetAutoChange(bool enabled)
    {
        autoChange = enabled;
        changeTimer = changeInterval;
    }
}