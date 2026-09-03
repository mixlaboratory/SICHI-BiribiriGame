using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField]
    private PlayerController player;

    [Header("シーン")]
    [SerializeField]
    private string gameSceneName = "SampleScene";

    [SerializeField]
    private string brainwaveSceneName = "BrainwaveScene";


    private void Start()
    {
        /*
         * タイトル画面では
         * SPACEを押さなくても最初から操作可能
         */
        if (player != null)
        {
            player.StartMoving();
        }
    }


    public void StartGame()
    {
        Debug.Log("ゲーム開始");

        SceneManager.LoadScene(
            gameSceneName
        );
    }


    public void OpenBrainwaveMeasurement()
    {
        Debug.Log("脳波測定画面へ");

        SceneManager.LoadScene(
            brainwaveSceneName
        );
    }
}