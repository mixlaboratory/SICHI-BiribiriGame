using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public enum ControlMode
    {
        Mouse,
        JoyConTilt
    }

    [Header("操作モード")]
    [SerializeField]
    private ControlMode controlMode = ControlMode.JoyConTilt;


    // ==================================================
    // マウス
    // ==================================================

    [Header("マウス設定")]
    [SerializeField]
    private float mouseFollowSpeed = 15f;


    // ==================================================
    // Joy-Con
    // ==================================================

    [Header("Joy-Con設定")]
    [Tooltip("Joy-Conが1台なら0")]
    [SerializeField]
    private int joyconIndex = 0;

    [Header("傾き操作設定")]

    [Tooltip("Playerの最大移動速度")]
    [SerializeField]
    private float tiltMoveSpeed = 6f;

    [Tooltip("小さい傾きを無視する角度")]
    [SerializeField]
    private float tiltDeadZone = 4f;

    [Tooltip("この角度で最大入力になる")]
    [SerializeField]
    private float maximumTiltAngle = 30f;

    [Tooltip("傾きの感度")]
    [SerializeField]
    private float tiltSensitivity = 1f;

    [Header("軸調整")]
    [SerializeField]
    private bool swapAxes = false;

    [SerializeField]
    private bool invertX = false;

    [SerializeField]
    private bool invertY = false;


    // ==================================================
    // ノックバック
    // ==================================================

    [Header("ノックバック")]
    [SerializeField]
    private float knockbackPower = 4f;

    [SerializeField]
    private float knockbackDuration = 0.18f;


    // ==================================================
    // Joy-Con振動
    // ==================================================

    [Header("Joy-Con振動")]
    [SerializeField, Range(0f, 1f)]
    private float rumbleStrength = 0.5f;

    [SerializeField]
    private int rumbleMilliseconds = 250;

    [SerializeField]
    private float rumbleLowFrequency = 160f;

    [SerializeField]
    private float rumbleHighFrequency = 320f;


    // ==================================================
    // 内部変数
    // ==================================================

    private Rigidbody2D rb;
    private Camera mainCamera;

    private Joycon joycon;

    // Joy-Conを普通に持っている状態
    private Quaternion neutralRotation =
        Quaternion.identity;

    // Joy-Conから得た移動入力
    private Vector2 tiltInput;

    // マウス用
    private Vector2 mouseTargetPosition;

    private bool joyconCalibrated = false;
    private bool canMove = false;
    private bool isKnockback = false;

    private Coroutine knockbackCoroutine;

    private float nextSearchTime;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        mainCamera = Camera.main;

        mouseTargetPosition =
            rb.position;
    }


    private void Start()
    {
        if (controlMode ==
            ControlMode.JoyConTilt)
        {
            TryGetJoycon();
        }
    }


    private void Update()
    {
        if (controlMode ==
            ControlMode.Mouse)
        {
            if (canMove &&
                !isKnockback)
            {
                ReadMouse();
            }

            return;
        }


        // ============================
        // Joy-Con検索
        // ============================

        if (joycon == null)
        {
            if (Time.unscaledTime >=
                nextSearchTime)
            {
                nextSearchTime =
                    Time.unscaledTime + 0.5f;

                TryGetJoycon();
            }

            return;
        }


        // ============================
        // 接続切れ
        // ============================

        if (joycon.state <=
            Joycon.state_.NO_JOYCONS)
        {
            Debug.LogWarning(
                "Joy-Conとの接続が切れました"
            );

            joycon = null;
            joyconCalibrated = false;

            return;
        }


        // ============================
        // スティック押し込みで
        // 中央姿勢を再設定
        // ============================

        if (joycon.GetButtonDown(
            Joycon.Button.STICK))
        {
            CalibrateJoycon();

            Debug.Log(
                "Joy-Con中央姿勢を再設定"
            );
        }


        if (!canMove ||
            isKnockback)
        {
            tiltInput = Vector2.zero;
            return;
        }


        ReadTilt();
    }


    private void FixedUpdate()
    {
        if (!canMove ||
            isKnockback)
        {
            return;
        }


        // ==================================================
        // マウス
        // ==================================================

        if (controlMode ==
            ControlMode.Mouse)
        {
            Vector2 nextPosition =
                Vector2.MoveTowards(
                    rb.position,
                    mouseTargetPosition,
                    mouseFollowSpeed *
                    Time.fixedDeltaTime
                );

            rb.MovePosition(
                nextPosition
            );

            return;
        }


        // ==================================================
        // Joy-Con傾き
        // ==================================================

        if (joycon == null ||
            !joyconCalibrated)
        {
            return;
        }


        Vector2 movement =
            tiltInput *
            tiltMoveSpeed *
            Time.fixedDeltaTime;


        rb.MovePosition(
            rb.position +
            movement
        );
    }


    // ==================================================
    // Joy-Con取得
    // ==================================================

    private void TryGetJoycon()
    {
        if (JoyconManager.Instance == null)
        {
            Debug.LogWarning(
                "JoyconManagerが見つかりません"
            );

            return;
        }


        List<Joycon> joycons =
            JoyconManager.Instance.j;


        if (joycons == null ||
            joycons.Count == 0)
        {
            Debug.LogWarning(
                "Joy-Conが認識されていません"
            );

            return;
        }


        if (joyconIndex < 0 ||
            joyconIndex >= joycons.Count)
        {
            joyconIndex = 0;
        }


        Joycon candidate =
            joycons[joyconIndex];


        // IMUデータ準備待ち
        if (candidate.state !=
            Joycon.state_.IMU_DATA_OK)
        {
            return;
        }


        joycon = candidate;

        CalibrateJoycon();


        string side =
            joycon.isLeft
            ? "左Joy-Con"
            : "右Joy-Con";


        Debug.Log(
            side +
            " 傾き操作準備完了"
        );
    }


    // ==================================================
    // 中央姿勢
    // ==================================================

    private void CalibrateJoycon()
    {
        if (joycon == null)
        {
            return;
        }


        /*
         * 今持っているJoy-Conの向きを
         * 「傾き0」として登録
         */
        neutralRotation =
            joycon.GetVector();


        tiltInput =
            Vector2.zero;


        joyconCalibrated =
            true;
    }


    // ==================================================
    // 傾き取得
    // ==================================================

    private void ReadTilt()
    {
        if (!joyconCalibrated)
        {
            return;
        }


        Quaternion currentRotation =
            joycon.GetVector();


        /*
         * 中央姿勢との差
         */
        Quaternion relativeRotation =
            Quaternion.Inverse(
                neutralRotation
            ) *
            currentRotation;


        Vector3 angles =
            relativeRotation.eulerAngles;


        /*
         * Euler角は0～360になるため
         * -180～180へ変換
         */
        float tiltX =
            Mathf.DeltaAngle(
                0f,
                angles.z
            );


        float tiltY =
            Mathf.DeltaAngle(
                0f,
                angles.x
            );


        if (swapAxes)
        {
            float temp = tiltX;

            tiltX = tiltY;
            tiltY = temp;
        }


        if (invertX)
        {
            tiltX = -tiltX;
        }


        if (invertY)
        {
            tiltY = -tiltY;
        }


        /*
         * デッドゾーン
         */
        tiltX =
            ApplyTiltDeadZone(
                tiltX
            );


        tiltY =
            ApplyTiltDeadZone(
                tiltY
            );


        /*
         * 例えば30度傾けたら
         * 入力1.0になる
         */
        float inputX =
            Mathf.Clamp(
                tiltX /
                maximumTiltAngle,
                -1f,
                1f
            );


        float inputY =
            Mathf.Clamp(
                tiltY /
                maximumTiltAngle,
                -1f,
                1f
            );


        tiltInput =
            new Vector2(
                inputX,
                inputY
            ) *
            tiltSensitivity;


        tiltInput =
            Vector2.ClampMagnitude(
                tiltInput,
                1f
            );
    }


    private float ApplyTiltDeadZone(
        float angle
    )
    {
        if (Mathf.Abs(angle) <
            tiltDeadZone)
        {
            return 0f;
        }


        /*
         * デッドゾーンを超えた瞬間に
         * 急に動かないようにする
         */
        return Mathf.Sign(angle) *
               (
                   Mathf.Abs(angle) -
                   tiltDeadZone
               );
    }


    // ==================================================
    // マウス
    // ==================================================

    private void ReadMouse()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                return;
            }
        }


        Vector3 mousePosition =
            Input.mousePosition;


        mousePosition.z =
            -mainCamera.transform.position.z;


        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                mousePosition
            );


        mouseTargetPosition =
            new Vector2(
                worldPosition.x,
                worldPosition.y
            );
    }


    // ==================================================
    // GameManager
    // ==================================================

    public void StartMoving()
    {
        canMove = true;

        tiltInput =
            Vector2.zero;

        mouseTargetPosition =
            rb.position;


        /*
         * SPACEを押した瞬間の
         * Joy-Con姿勢を中央にする
         */
        if (controlMode ==
            ControlMode.JoyConTilt)
        {
            if (joycon == null)
            {
                TryGetJoycon();
            }


            if (joycon != null)
            {
                CalibrateJoycon();
            }
        }
    }


    public void StopMoving()
    {
        canMove = false;

        tiltInput =
            Vector2.zero;


        rb.linearVelocity =
            Vector2.zero;
    }


    public void ResetPlayer(
        Vector2 position
    )
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(
                knockbackCoroutine
            );

            knockbackCoroutine = null;
        }


        canMove = false;
        isKnockback = false;

        tiltInput =
            Vector2.zero;


        rb.linearVelocity =
            Vector2.zero;


        rb.position =
            position;


        mouseTargetPosition =
            position;


        if (controlMode ==
            ControlMode.JoyConTilt &&
            joycon != null)
        {
            CalibrateJoycon();
        }
    }


    // ==================================================
    // 操作切り替え
    // ==================================================

    public void SetMouseMode()
    {
        controlMode =
            ControlMode.Mouse;

        tiltInput =
            Vector2.zero;

        mouseTargetPosition =
            rb.position;


        Debug.Log(
            "操作：Mouse"
        );
    }


    public void SetJoyConMode()
    {
        controlMode =
            ControlMode.JoyConTilt;


        if (joycon == null)
        {
            TryGetJoycon();
        }


        if (joycon != null)
        {
            CalibrateJoycon();
        }


        Debug.Log(
            "操作：Joy-Con Tilt"
        );
    }


    // ==================================================
    // ノックバック
    // ==================================================

    public void Knockback(
        Vector2 direction
    )
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(
                knockbackCoroutine
            );
        }


        knockbackCoroutine =
            StartCoroutine(
                KnockbackCoroutine(
                    direction
                )
            );
    }


    private IEnumerator KnockbackCoroutine(
        Vector2 direction
    )
    {
        isKnockback = true;

        tiltInput =
            Vector2.zero;


        rb.linearVelocity =
            Vector2.zero;


        rb.AddForce(
            direction.normalized *
            knockbackPower,
            ForceMode2D.Impulse
        );


        yield return
            new WaitForSeconds(
                knockbackDuration
            );


        rb.linearVelocity =
            Vector2.zero;


        mouseTargetPosition =
            rb.position;


        isKnockback = false;

        knockbackCoroutine = null;
    }


    // ==================================================
    // 振動
    // ==================================================

    public void Rumble()
    {
        if (joycon == null)
        {
            return;
        }


        if (joycon.state <=
            Joycon.state_.ATTACHED)
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
}