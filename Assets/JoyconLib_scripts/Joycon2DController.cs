using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Joy-Conのスティックで2Dオブジェクトを移動し，
/// ボタン入力をConsoleへ表示して振動させるスクリプト．
/// </summary>
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Rigidbody2D))]
public class Joycon2DController : MonoBehaviour
{
    [Header("Joy-Con設定")]

    [Tooltip("使用するJoy-Conの番号．1台だけの場合は0")]
    [SerializeField]
    private int joyconIndex = 0;


    [Header("移動設定")]

    [SerializeField]
    private float moveSpeed = 5.0f;

    [SerializeField]
    private bool invertX = false;

    [SerializeField]
    private bool invertY = false;

    [Tooltip("この値未満のスティック入力を無視する")]
    [SerializeField, Range(0.0f, 1.0f)]
    private float stickDeadZone = 0.1f;

    [Header("ジャイロ設定")]

    [Tooltip("最大入力として扱う傾き角度")]
    [SerializeField]
    private float maxTiltDegrees = 25.0f;

    [Tooltip("この角度未満の傾きを無視する")]
    [SerializeField]
    private float gyroDeadZoneDegrees = 2.0f;

    private Quaternion neutralRotation = Quaternion.identity;
    private bool gyroCalibrated = false;


    [Header("振動設定")]

    [Tooltip("いずれかのボタンを押したときに振動させる")]
    [SerializeField]
    private bool rumbleOnAnyButton = true;

    [SerializeField, Range(0.0f, 1.0f)]
    private float rumbleStrength = 0.8f;

    [Tooltip("振動時間．単位はミリ秒")]
    [SerializeField]
    private int rumbleMilliseconds = 500;

    [SerializeField]
    private float rumbleLowFrequency = 160.0f;

    [SerializeField]
    private float rumbleHighFrequency = 320.0f;


    private Rigidbody2D rb;
    private Joycon joycon;

    private Vector2 moveInput;

    private Joycon.Button[] allButtons;

    private float nextSearchTime;

    private bool joyconFoundLogged = false;
    private bool managerWarningLogged = false;
    private bool joyconWarningLogged = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        allButtons =
            (Joycon.Button[])Enum.GetValues(
                typeof(Joycon.Button)
            );
    }


    private void Start()
    {
        TryGetJoycon();
    }


    private void Update()
    {
        /*
         * JoyconManagerやJoy-Conの取得が遅れた場合に備えて，
         * 0.5秒ごとに再検索する．
         */
        if (joycon == null)
        {
            if (Time.unscaledTime >= nextSearchTime)
            {
                nextSearchTime =
                    Time.unscaledTime + 0.5f;

                TryGetJoycon();
            }

            moveInput = Vector2.zero;
            return;
        }

        /*
         * Joy-Conとの接続が切れた場合は，
         * Joy-Conの参照を解除する．
         */
        if (joycon.state <= Joycon.state_.NO_JOYCONS)
        {
            Debug.LogWarning(
                $"Joy-Conとの接続が切れました．" +
                $"State={joycon.state}"
            );

            joycon = null;
            moveInput = Vector2.zero;

            joyconFoundLogged = false;
            joyconWarningLogged = false;

            return;
        }

        ReadGyroTilt();

        /*
        * スティック押し込みで中央位置を再設定する．
        */
        if (joycon.GetButtonDown(Joycon.Button.STICK))
        {
            neutralRotation = joycon.GetVector();

            Debug.Log(
                "Joy-Conの中央位置を再設定しました．"
            );
        }

        CheckButtons();
    }


    /// <summary>
    /// JoyconManagerから使用するJoy-Conを取得する．
    /// </summary>
    private void TryGetJoycon()
    {
        if (JoyconManager.Instance == null)
        {
            if (!managerWarningLogged)
            {
                Debug.LogWarning(
                    "JoyconManagerが見つかりません．" +
                    "HierarchyにJoyconManagerを配置し，" +
                    "JoyconManager.csを追加してください．"
                );

                managerWarningLogged = true;
            }

            return;
        }

        managerWarningLogged = false;

        List<Joycon> joycons =
            JoyconManager.Instance.j;

        if (joycons == null || joycons.Count == 0)
        {
            if (!joyconWarningLogged)
            {
                Debug.LogWarning(
                    "JoyconManagerは見つかりましたが，" +
                    "Joy-Conが認識されていません．" +
                    "Unityを再生する前に，WindowsのBluetooth設定で" +
                    "Joy-Conを接続してください．"
                );

                joyconWarningLogged = true;
            }

            return;
        }

        joyconWarningLogged = false;

        /*
         * 指定された番号が範囲外の場合は，
         * 最初のJoy-Conを使用する．
         */
        if (joyconIndex < 0 ||
            joyconIndex >= joycons.Count)
        {
            Debug.LogWarning(
                $"Joycon Index={joyconIndex}は範囲外です．" +
                "Index=0のJoy-Conを使用します．"
            );

            joyconIndex = 0;
        }

        Joycon candidate = joycons[joyconIndex];

        /*
        * Joy-Conの入力通信が開始されるまで待つ．
        */
        if (candidate.state != Joycon.state_.IMU_DATA_OK)
        {
            if (!joyconWarningLogged)
            {
                Debug.Log(
                    $"Joy-Conの初期化を待っています．" +
                    $"Index={joyconIndex}，" +
                    $"State={candidate.state}"
                );

                joyconWarningLogged = true;
            }

            return;
        }

        joyconWarningLogged = false;
        joycon = candidate;

        neutralRotation = joycon.GetVector();
        gyroCalibrated = true;

        Debug.Log(
            "Joy-Conの現在の姿勢を中央位置として設定しました．"
        );

        if (!joyconFoundLogged)
        {
            string side =
                joycon.isLeft
                    ? "左Joy-Con"
                    : "右Joy-Con";

            Debug.Log(
                $"{side}の準備が完了しました．" +
                $"Index={joyconIndex}，" +
                $"State={joycon.state}"
            );

            joyconFoundLogged = true;
        }
    }


    /// <summary>
    /// Joy-Conのスティック入力を取得する．
    /// </summary>
    private void ReadStick()
    {
        float[] stick = joycon.GetStick();

        if (stick == null || stick.Length < 2)
        {
            moveInput = Vector2.zero;
            return;
        }

        float x =
            invertX
                ? -stick[0]
                : stick[0];

        float y =
            invertY
                ? -stick[1]
                : stick[1];

        moveInput = new Vector2(x, y);

        /*
         * スティックの小さな揺れを無視する．
         */
        if (moveInput.magnitude < stickDeadZone)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            /*
             * 斜め入力時に速度が速くなりすぎないようにする．
             */
            moveInput =
                Vector2.ClampMagnitude(
                    moveInput,
                    1.0f
                );
        }
    }

    /// <summary>
    /// Joy-Conの傾きから2D移動量を取得する．
    /// </summary>
    private void ReadGyroTilt()
    {
        if (!gyroCalibrated)
        {
            moveInput = Vector2.zero;
            return;
        }

        Quaternion currentRotation =
            joycon.GetVector();

        /*
        * 最初に記録した姿勢から，
        * 現在どの程度傾いているかを求める．
        */
        Quaternion relativeRotation =
            Quaternion.Inverse(neutralRotation) *
            currentRotation;

        Vector3 angles =
            relativeRotation.eulerAngles;

        /*
        * 0～360度の角度を，
        * -180～180度へ変換する．
        */
        float horizontalTilt =
            Mathf.DeltaAngle(
                0.0f,
                angles.z
            );

        float verticalTilt =
            Mathf.DeltaAngle(
                0.0f,
                angles.x
            );

        /*
        * 小さな手振れを無視する．
        */
        if (
            Mathf.Abs(horizontalTilt) <
            gyroDeadZoneDegrees
        )
        {
            horizontalTilt = 0.0f;
        }

        if (
            Mathf.Abs(verticalTilt) <
            gyroDeadZoneDegrees
        )
        {
            verticalTilt = 0.0f;
        }

        /*
        * 傾き角度を-1～1の移動入力へ変換する．
        */
        float x = Mathf.Clamp(
            horizontalTilt / maxTiltDegrees,
            -1.0f,
            1.0f
        );

        float y = Mathf.Clamp(
            -verticalTilt / maxTiltDegrees,
            -1.0f,
            1.0f
        );

        if (invertX)
        {
            x = -x;
        }

        if (invertY)
        {
            y = -y;
        }

        moveInput =
            new Vector2(x, y);
    }


    /// <summary>
    /// すべてのJoy-Conボタンを確認する．
    /// </summary>
    private void CheckButtons()
    {
        foreach (Joycon.Button button in allButtons)
        {
            if (!joycon.GetButtonDown(button))
            {
                continue;
            }

            Debug.Log(
                $"Joy-Con入力：{button}，" +
                $"State={joycon.state}，" +
                $"Left={joycon.isLeft}"
            );

            if (rumbleOnAnyButton)
            {
                StartRumble();
            }
        }
    }


    /// <summary>
    /// Joy-Conを振動させる．
    /// </summary>
    private void StartRumble()
    {
        /*
         * Joycon.csではATTACHED以下の状態では，
         * SetRumbleが実行されない．
         */
        if (joycon.state <= Joycon.state_.ATTACHED)
        {
            Debug.LogWarning(
                "Joy-Conは検出されていますが，" +
                "入力データの受信状態になっていません．" +
                $"現在のState={joycon.state}"
            );

            return;
        }

        joycon.SetRumble(
            rumbleLowFrequency,
            rumbleHighFrequency,
            rumbleStrength,
            rumbleMilliseconds
        );

        Debug.Log(
            $"振動命令を実行しました．" +
            $"強度={rumbleStrength:F2}，" +
            $"時間={rumbleMilliseconds}ms"
        );
    }


    private void FixedUpdate()
    {
        if (joycon == null)
        {
            return;
        }

        Vector2 nextPosition =
            rb.position +
            moveInput *
            moveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);
    }


    private void OnDisable()
    {
        moveInput = Vector2.zero;
    }
}