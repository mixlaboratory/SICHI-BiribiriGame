using UnityEngine;

public class TitleSelectZone : MonoBehaviour
{
    public enum SelectType
    {
        StartGame,
        BrainwaveMeasurement
    }

    [Header("選択内容")]
    [SerializeField]
    private SelectType selectType;

    [Header("参照")]
    [SerializeField]
    private TitleSceneManager titleManager;

    [Header("決定までの時間")]
    [Tooltip("この時間ゾーン内にいたら決定")]
    [SerializeField]
    private float holdDuration = 3f;

    [Header("選択中の見た目")]
    [Tooltip("ゾーンに入ったとき何倍に大きくするか")]
    [SerializeField]
    private float selectedScale = 1.1f;

    [Tooltip("大きさが変化する速さ")]
    [SerializeField]
    private float scaleSpeed = 8f;

    private bool playerInside = false;
    private bool selected = false;

    private float holdTimer = 0f;

    private Vector3 originalScale;
    private Vector3 targetScale;


    private void Awake()
    {
        if (titleManager == null)
        {
            titleManager =
                FindFirstObjectByType<TitleSceneManager>();
        }

        originalScale =
            transform.localScale;

        targetScale =
            originalScale;
    }


    private void Update()
    {
        // =========================
        // サイズを滑らかに変更
        // =========================

        transform.localScale =
            Vector3.Lerp(
                transform.localScale,
                targetScale,
                scaleSpeed * Time.deltaTime
            );


        // =========================
        // 3秒カウント
        // =========================

        if (!playerInside || selected)
        {
            return;
        }

        holdTimer += Time.deltaTime;

        if (holdTimer >= holdDuration)
        {
            Select();
        }
    }


    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;
        holdTimer = 0f;

        // 少し大きくする
        targetScale =
            originalScale * selectedScale;

        Debug.Log(
            selectType +
            " 選択中..."
        );
    }


    private void OnTriggerExit2D(
        Collider2D other
    )
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = false;
        holdTimer = 0f;

        // 元のサイズへ戻す
        targetScale =
            originalScale;

        Debug.Log(
            selectType +
            " 選択キャンセル"
        );
    }


    private void Select()
    {
        if (selected)
        {
            return;
        }

        selected = true;

        Debug.Log(
            selectType +
            " 決定！"
        );

        switch (selectType)
        {
            case SelectType.StartGame:

                titleManager?.StartGame();

                break;


            case SelectType.BrainwaveMeasurement:

                titleManager?.
                    OpenBrainwaveMeasurement();

                break;
        }
    }
}