using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CancelButtonHover : MonoBehaviour
{
    [Header("画像")]
    [SerializeField]
    private Image buttonImage;

    [SerializeField]
    private Sprite normalSprite;

    [SerializeField]
    private Sprite hoverSprite;

    [Header("Joy-Conカーソル")]
    [SerializeField]
    private RectTransform joyconCursor;

    [Header("滞在ゲージ")]
    [SerializeField]
    private Image holdProgress;

    [SerializeField]
    private float holdDuration = 1.5f;

    [Header("Canvas")]
    [SerializeField]
    private Canvas canvas;

    [Header("キャンセル後のシーン")]
    [SerializeField]
    private string cancelSceneName = "SampleScene";

    private RectTransform buttonRect;

    private float holdTimer = 0f;
    private bool isSelected = false;
    private bool activated = false;

    private void Awake()
    {
        buttonRect = GetComponent<RectTransform>();

        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }

        if (holdProgress != null)
        {
            holdProgress.fillAmount = 0f;
            holdProgress.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        CheckCursor();
        UpdateHold();
    }

    private void CheckCursor()
    {
        if (joyconCursor == null || buttonRect == null)
        {
            return;
        }

        Camera uiCamera = null;

        if (canvas != null &&
            canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        Vector2 cursorScreenPosition =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                joyconCursor.position
            );

        isSelected =
            RectTransformUtility.RectangleContainsScreenPoint(
                buttonRect,
                cursorScreenPosition,
                uiCamera
            );

        if (buttonImage != null)
        {
            buttonImage.sprite =
                isSelected
                    ? hoverSprite
                    : normalSprite;
        }
    }

    private void UpdateHold()
    {
        if (activated)
        {
            return;
        }

        if (isSelected)
        {
            holdTimer += Time.deltaTime;

            if (holdProgress != null)
            {
                holdProgress.gameObject.SetActive(true);

                holdProgress.fillAmount =
                    Mathf.Clamp01(
                        holdTimer / holdDuration
                    );
            }

            if (holdTimer >= holdDuration)
            {
                ActivateCancel();
            }
        }
        else
        {
            holdTimer = 0f;

            if (holdProgress != null)
            {
                holdProgress.fillAmount = 0f;
                holdProgress.gameObject.SetActive(false);
            }
        }
    }

    private void ActivateCancel()
    {
        activated = true;

        if (holdProgress != null)
        {
            holdProgress.fillAmount = 1f;
        }

        Debug.Log("キャンセル確定");

        SceneManager.LoadScene(
            cancelSceneName
        );
    }
}