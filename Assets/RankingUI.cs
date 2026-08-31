using UnityEngine;
using TMPro;

public class RankingUI : MonoBehaviour
{
    [Header("今回のタイム")]
    [SerializeField]
    private TMP_Text currentTimeText;

    [Header("ランキング")]
    [SerializeField]
    private TMP_Text rank1TimeText;

    [SerializeField]
    private TMP_Text rank2TimeText;

    [SerializeField]
    private TMP_Text rank3TimeText;

    private void Start()
    {
        float clearTime =
            GameResultData.ClearTime;

        currentTimeText.text =
            FormatTime(clearTime);

        // まずは表示テスト
        rank1TimeText.text = "00:58.21";
        rank2TimeText.text = "01:05.47";
        rank3TimeText.text = "01:18.92";
    }

    private string FormatTime(float time)
    {
        int minutes =
            Mathf.FloorToInt(time / 60f);

        float seconds =
            time % 60f;

        return string.Format(
            "{0:00}:{1:00.00}",
            minutes,
            seconds
        );
    }
}