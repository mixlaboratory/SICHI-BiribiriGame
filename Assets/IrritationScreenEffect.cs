using UnityEngine;
using UnityEngine.UI;

public class IrritationScreenEffect : MonoBehaviour
{
    [Header("参照")]
    [SerializeField]
    private IrritationManager irritationManager;

    [SerializeField]
    private Image redOverlay;

    [Header("赤背景")]
    [Tooltip("この値から赤みが出始める")]
    [SerializeField, Range(0f, 100f)]
    private float redStartThreshold = 40f;

    [Tooltip("通常時の最大透明度")]
    [SerializeField, Range(0f, 1f)]
    private float maxRedAlpha = 0.18f;

    [Header("点滅")]
    [Tooltip("この値以上で点滅開始")]
    [SerializeField, Range(0f, 100f)]
    private float blinkThreshold = 80f;

    [Tooltip("点滅の速さ")]
    [SerializeField]
    private float blinkSpeed = 5f;

    [Tooltip("点滅時の最大透明度")]
    [SerializeField, Range(0f, 1f)]
    private float blinkMaxAlpha = 0.32f;

    private void Start()
    {
        if (irritationManager == null)
        {
            irritationManager =
                FindFirstObjectByType<IrritationManager>();
        }

        SetOverlayAlpha(0f);
    }

    private void Update()
    {
        if (irritationManager == null ||
            redOverlay == null)
        {
            return;
        }

        float irritation =
            irritationManager.CurrentIrritation;

        // 低いときは赤背景なし
        if (irritation < redStartThreshold)
        {
            SetOverlayAlpha(0f);
            return;
        }

        // 高イライラ時は点滅
        if (irritation >= blinkThreshold)
        {
            UpdateBlink(irritation);
            return;
        }

        // 中程度では普通に赤くなる
        UpdateNormalRed(irritation);
    }

    private void UpdateNormalRed(
        float irritation
    )
    {
        float ratio = Mathf.InverseLerp(
            redStartThreshold,
            blinkThreshold,
            irritation
        );

        float alpha = Mathf.Lerp(
            0f,
            maxRedAlpha,
            ratio
        );

        SetOverlayAlpha(alpha);
    }

    private void UpdateBlink(
        float irritation
    )
    {
        // 0～1でゆっくり点滅
        float blink =
            (Mathf.Sin(
                Time.time * blinkSpeed
            ) + 1f) / 2f;

        float irritationRatio =
            Mathf.InverseLerp(
                blinkThreshold,
                100f,
                irritation
            );

        float maximumAlpha =
            Mathf.Lerp(
                maxRedAlpha,
                blinkMaxAlpha,
                irritationRatio
            );

        float alpha =
            Mathf.Lerp(
                maxRedAlpha * 0.4f,
                maximumAlpha,
                blink
            );

        SetOverlayAlpha(alpha);
    }

    private void SetOverlayAlpha(
        float alpha
    )
    {
        Color color =
            redOverlay.color;

        color.r = 1f;
        color.g = 0f;
        color.b = 0f;
        color.a = alpha;

        redOverlay.color = color;
    }
}