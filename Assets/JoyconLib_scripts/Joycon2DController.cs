using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Joy-Conをポインタとして使用し，
/// Joy-Conが指している方向へ2Dオブジェクトを移動する．
/// </summary>
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Rigidbody2D))]
public class Joycon2DController : MonoBehaviour
{
    [Header("Joy-Con設定")]

    [Tooltip("使用するJoy-Conの番号．1台だけの場合は0")]
    [SerializeField]
    private int joyconIndex = 0;


    [Header("ポインタ設定")]

    [Tooltip("Joy-Conから仮想平面までの距離．大きいほど感度が高くなる")]
    [SerializeField]
    private float pointerDistance = 10.0f;

    [Tooltip("ポインタ移動量の倍率")]
    [SerializeField]
    private float pointerSensitivity = 1.0f;

    [Tooltip("この値未満の小さな動きを無視する")]
    [SerializeField]
    private float pointerDeadZone = 0.03f;

    [Tooltip("基準位置から移動できる最大距離")]
    [SerializeField]
    private float maximumPointerOffset = 15.0f;

    [Tooltip("Playerがポインタへ追従する速度")]
    [SerializeField]
    private float followSpeed = 30.0f;

    [Tooltip("左右と上下の入力軸を交換する")]
    [SerializeField]
    private bool swapAxes = false;

    [SerializeField]
    private bool invertX = false;

    [SerializeField]
    private bool invertY = false;


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

    private Quaternion neutralRotation = Quaternion.identity;

    /*
     * ポインタの基準となるPlayerの位置．
     * スティック押し込みで現在位置へ再設定する．
     */
    private Vector2 pointerOrigin;
    private Vector2 targetPosition;

    private bool pointerCalibrated = false;

    private Joycon.Button[] allButtons;

    private float nextSearchTime;

    private bool joyconFoundLogged = false;
    private bool managerWarningLogged = false;
    private bool joyconWarningLogged = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        pointerOrigin = rb.position;
        targetPosition = rb.position;

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
         * JoyconManagerやJoy-Conの準備が遅れた場合に備え，
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

            return;
        }

        /*
         * Joy-Conとの接続が切れた場合．
         */
        if (joycon.state <= Joycon.state_.NO_JOYCONS)
        {
            Debug.LogWarning(
                $"Joy-Conとの接続が切れました．" +
                $"State={joycon.state}"
            );

            joycon = null;
            pointerCalibrated = false;

            joyconFoundLogged = false;
            joyconWarningLogged = false;

            return;
        }

        /*
         * スティック押し込みで，
         * 現在のJoy-Conの向きとPlayer位置を中央に設定する．
         */
        if (joycon.GetButtonDown(Joycon.Button.STICK))
        {
            CalibratePointer();

            Debug.Log(
                "ポインタの中央位置を再設定しました．"
            );
        }

        ReadPointer();

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
                    "HierarchyにJoyconManagerを1つ配置してください．"
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
                    "Joy-Conが認識されていません．" +
                    "WindowsのBluetooth接続を確認してください．"
                );

                joyconWarningLogged = true;
            }

            return;
        }

        if (joyconIndex < 0 ||
            joyconIndex >= joycons.Count)
        {
            Debug.LogWarning(
                $"Joycon Index={joyconIndex}は範囲外です．" +
                "Index=0を使用します．"
            );

            joyconIndex = 0;
        }

        Joycon candidate =
            joycons[joyconIndex];

        /*
         * IMUデータの受信開始まで待つ．
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

        CalibratePointer();

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
    /// 現在のJoy-Conの向きとPlayer位置を基準にする．
    /// </summary>
    private void CalibratePointer()
    {
        neutralRotation =
            joycon.GetVector();

        pointerOrigin =
            rb.position;

        targetPosition =
            rb.position;

        pointerCalibrated = true;
    }


    /// <summary>
    /// Joy-Conが指している方向と仮想平面との交点を求める．
    /// </summary>
    private void ReadPointer()
    {
        if (!pointerCalibrated)
        {
            return;
        }

        Quaternion currentRotation =
            joycon.GetVector();

        /*
         * 基準姿勢からの相対的な回転を求める．
         */
        Quaternion relativeRotation =
            Quaternion.Inverse(neutralRotation) *
            currentRotation;

        /*
         * Joy-Conが指している方向を求める．
         * 中央姿勢ではVector3.forwardになる．
         */
        Vector3 pointerDirection =
            relativeRotation *
            Vector3.forward;

        /*
         * Joy-Conが仮想平面と反対方向を向いた場合は，
         * 交点を計算しない．
         */
        if (pointerDirection.z <= 0.05f)
        {
            return;
        }

        /*
         * z=pointerDistanceにある仮想平面との交点を計算する．
         */
        float intersectionDistance =
            pointerDistance /
            pointerDirection.z;

        float x =
            pointerDirection.x *
            intersectionDistance;

        float y =
            pointerDirection.y *
            intersectionDistance;

        /*
         * Joy-Conの持ち方に合わせて軸を交換できるようにする．
         */
        if (swapAxes)
        {
            float temporary = x;
            x = y;
            y = temporary;
        }

        if (invertX)
        {
            x = -x;
        }

        if (invertY)
        {
            y = -y;
        }

        x *= pointerSensitivity;
        y *= pointerSensitivity;

        /*
         * 小さな手振れを無視する．
         */
        if (Mathf.Abs(x) < pointerDeadZone)
        {
            x = 0.0f;
        }

        if (Mathf.Abs(y) < pointerDeadZone)
        {
            y = 0.0f;
        }

        Vector2 pointerOffset =
            new Vector2(x, y);

        pointerOffset =
            Vector2.ClampMagnitude(
                pointerOffset,
                maximumPointerOffset
            );

        /*
         * 基準位置からポインタ分だけ移動した場所を，
         * Playerの移動先にする．
         */
        targetPosition =
            pointerOrigin +
            pointerOffset;
    }


    /// <summary>
    /// ボタン入力と振動を処理する．
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


    private void StartRumble()
    {
        if (joycon.state <= Joycon.state_.ATTACHED)
        {
            return;
        }

        joycon.SetRumble(
            rumbleLowFrequency,
            rumbleHighFrequency,
            rumbleStrength,
            rumbleMilliseconds
        );
    }


    private void FixedUpdate()
    {
        if (joycon == null ||
            !pointerCalibrated)
        {
            return;
        }

        /*
         * ポインタ位置へPlayerを追従させる．
         */
        Vector2 nextPosition =
            Vector2.MoveTowards(
                rb.position,
                targetPosition,
                followSpeed *
                Time.fixedDeltaTime
            );

        rb.MovePosition(nextPosition);
    }


    private void OnDisable()
    {
        if (rb != null)
        {
            targetPosition = rb.position;
        }
    }
}