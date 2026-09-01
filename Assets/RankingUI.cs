using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("リトライ")]
    [SerializeField]
    private string gameSceneName = "SampleScene";

    private List<float> rankingTimes =
        new List<float>();


    private void Start()
    {
        // 過去ランキングを読み込む
        LoadRanking();

        // 今回の結果を表示
        if (GameResultData.HasResult)
        {
            float clearTime =
                GameResultData.ClearTime;

            currentTimeText.text =
                FormatTime(clearTime);

            // 今回のタイムをランキングへ追加
            AddRanking(clearTime);

            // 二重登録防止
            GameResultData.HasResult = false;
        }
        else
        {
            currentTimeText.text =
                "--:--.--";
        }

        UpdateRankingUI();
    }


    private void Update()
    {
        // Rキーでゲーム本編へ戻る
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(
                gameSceneName
            );
        }
    }


    private void AddRanking(float time)
    {
        rankingTimes.Add(time);

        // タイムが短い順に並べる
        rankingTimes.Sort();

        // 上位3件だけ残す
        if (rankingTimes.Count > 3)
        {
            rankingTimes.RemoveRange(
                3,
                rankingTimes.Count - 3
            );
        }

        SaveRanking();
    }


    private void UpdateRankingUI()
    {
        SetRankText(
            rank1TimeText,
            0
        );

        SetRankText(
            rank2TimeText,
            1
        );

        SetRankText(
            rank3TimeText,
            2
        );
    }


    private void SetRankText(
        TMP_Text text,
        int index
    )
    {
        if (text == null)
        {
            return;
        }

        if (index < rankingTimes.Count)
        {
            text.text =
                FormatTime(
                    rankingTimes[index]
                );
        }
        else
        {
            text.text = "--:--.--";
        }
    }


    private void SaveRanking()
    {
        PlayerPrefs.SetInt(
            "RankCount",
            rankingTimes.Count
        );

        for (int i = 0;
             i < rankingTimes.Count;
             i++)
        {
            PlayerPrefs.SetFloat(
                "RankTime" + i,
                rankingTimes[i]
            );
        }

        PlayerPrefs.Save();
    }


    private void LoadRanking()
    {
        rankingTimes.Clear();

        int count =
            PlayerPrefs.GetInt(
                "RankCount",
                0
            );

        count = Mathf.Min(
            count,
            3
        );

        for (int i = 0;
             i < count;
             i++)
        {
            rankingTimes.Add(
                PlayerPrefs.GetFloat(
                    "RankTime" + i
                )
            );
        }

        rankingTimes.Sort();
    }


    private string FormatTime(float time)
    {
        int totalHundredths =
            Mathf.RoundToInt(
                time * 100f
            );

        int minutes =
            totalHundredths / 6000;

        int seconds =
            (totalHundredths / 100)
            % 60;

        int hundredths =
            totalHundredths % 100;

        return
            $"{minutes:00}:" +
            $"{seconds:00}." +
            $"{hundredths:00}";
    }
}