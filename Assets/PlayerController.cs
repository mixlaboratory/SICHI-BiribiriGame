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
        JoyCon
    }

    [Header("操作モード")]
    [SerializeField]
    private ControlMode controlMode = ControlMode.JoyCon;


    // =====================================
    // マウス
    // =====================================

    [Header("マウス設定")]
    [SerializeField]
    private float mouseFollowSpeed = 15f;


    // =====================================
    // Joy-Con
    // =====================================

    [Header("Joy-Con設定")]
    [Tooltip("使用するJoy-Conの番号。1台なら0")]
    [SerializeField]
    private int joyconIndex = 0;


    [Header("Joy-Con ポインター設定")]
    [SerializeField]
    private float pointerDistance = 10f;

    [SerializeField]
    private float pointerSensitivity = 1.3f;

    [SerializeField]
    private float pointerDeadZone = 0.03f;

    [SerializeField]
    private float maximumPointerOffset = 15f;

    [SerializeField]
    private float joyconFollowSpeed = 40f;


    [Header("Joy-Con 軸調整")]
    [SerializeField]
    private bool swapAxes = false;

    [SerializeField]
    private bool invertX = false;

    [SerializeField]
    private bool invertY = false;


    // =====================================
    // ノックバック
    // =====================================

    [Header("ノックバック")]
    [SerializeField]
    private float knockbackPower = 4f;

    [SerializeField]
    private float knockbackDuration = 0.18f;


    // =====================================
    // 振動
    // =====================================

    [Header("Joy-Con振動")]
    [SerializeField, Range(0f, 1f)]
    private float rumbleStrength = 0.5f;

    [SerializeField]
    private int rumbleMilliseconds = 250;

    [SerializeField]
    private float rumbleLowFrequency = 160f;

    [SerializeField]
    private float rumbleHighFrequency = 320f;


    // =====================================
    // 内部変数
    // =====================================

    private Rigidbody2D rb;
    private Camera mainCamera;

    private Joycon joycon;

    private Quaternion neutralRotation =
        Quaternion.identity;

    private Vector2 pointerOrigin;
    private Vector2 targetPosition;

    private bool pointerCalibrated = false;
    private bool canMove = false;
    private bool isKnockback = false;

    private Coroutine knockbackCoroutine;

    private float nextSearchTime;

    private bool managerWarningLogged = false;
    private bool joyconWarningLogged = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        pointerOrigin = rb.position;
        targetPosition = rb.position;
    }


    private void Start()
    {
        /*
         * Joy-Conモードなら
         * Joy-Conを探す
         */
        if (controlMode == ControlMode.JoyCon)
        {
            TryGetJoycon();
        }
    }


    private void Update()
    {
        if (!canMove || isKnockback)
        {
            return;
        }

        // ================================
        // マウス操作
        // ================================

        if (controlMode == ControlMode.Mouse)
        {
            ReadMouse();

            return;
        }

        // ================================
        // Joy-Con操作
        // ================================

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
         * 接続切れ確認
         */
        if (joycon.state <= Joycon.state_.NO_JOYCONS)
        {
            Debug.LogWarning(
                "Joy-Conとの接続が切れました"
            );

            joycon = null;
            pointerCalibrated = false;

            return;
        }

        /*
         * スティック押し込みで
         * Joy-Con中央を再設定
         */
        if (joycon.GetButtonDown(
            Joycon.Button.STICK))
        {
            CalibratePointer();

            Debug.Log(
                "Joy-Conポインターを再センタリング"
            );
        }

        ReadJoyconPointer();
    }


    private void FixedUpdate()
    {
        if (!canMove || isKnockback)
        {
            return;
        }

        float speed;

        if (controlMode == ControlMode.JoyCon)
        {
            if (joycon == null ||
                !pointerCalibrated)
            {
                return;
            }

            speed = joyconFollowSpeed;
        }
        else
        {
            speed = mouseFollowSpeed;
        }

        Vector2 nextPosition =
            Vector2.MoveTowards(
                rb.position,
                targetPosition,
                speed * Time.fixedDeltaTime
            );

        rb.MovePosition(nextPosition);
    }


    // =====================================
    // マウス操作
    // =====================================

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

        targetPosition =
            new Vector2(
                worldPosition.x,
                worldPosition.y
            );
    }


    // =====================================
    // Joy-Con取得
    // =====================================

    private void TryGetJoycon()
    {
        if (JoyconManager.Instance == null)
        {
            if (!managerWarningLogged)
            {
                Debug.LogWarning(
                    "JoyconManagerが見つかりません"
                );

                managerWarningLogged = true;
            }

            return;
        }

        managerWarningLogged = false;

        List<Joycon> joycons =
            JoyconManager.Instance.j;

        if (joycons == null ||
            joycons.Count == 0)
        {
            if (!joyconWarningLogged)
            {
                Debug.LogWarning(
                    "Joy-Conが認識されていません"
                );

                joyconWarningLogged = true;
            }

            return;
        }

        if (joyconIndex < 0 ||
            joyconIndex >= joycons.Count)
        {
            joyconIndex = 0;
        }

        Joycon candidate =
            joycons[joyconIndex];

        /*
         * IMUが使える状態になるまで待つ
         */
        if (candidate.state !=
            Joycon.state_.IMU_DATA_OK)
        {
            return;
        }

        joycon = candidate;

        joyconWarningLogged = false;

        CalibratePointer();

        Debug.Log(
            "Joy-Con準備完了！"
        );
    }


    // =====================================
    // Joy-Conポインター
    // =====================================

    private void CalibratePointer()
    {
        if (joycon == null)
        {
            return;
        }

        /*
         * 現在のJoy-Conの向きを
         * 中央方向として保存
         */
        neutralRotation =
            joycon.GetVector();

        /*
         * 現在のPlayer位置を
         * ポインターの基準位置にする
         */
        pointerOrigin =
            rb.position;

        targetPosition =
            rb.position;

        pointerCalibrated = true;
    }


    private void ReadJoyconPointer()
    {
        if (!pointerCalibrated)
        {
            return;
        }

        Quaternion currentRotation =
            joycon.GetVector();

        /*
         * 中央姿勢との差分
         */
        Quaternion relativeRotation =
            Quaternion.Inverse(
                neutralRotation
            ) *
            currentRotation;

        /*
         * Joy-Conが指している方向
         */
        Vector3 pointerDirection =
            relativeRotation *
            Vector3.forward;


        /*
         * 仮想平面より後ろを向いていたら
         * 計算しない
         */
        if (pointerDirection.z <= 0.05f)
        {
            return;
        }


        /*
         * 仮想平面との交点
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


        // 軸交換
        if (swapAxes)
        {
            float temp = x;

            x = y;
            y = temp;
        }


        // 左右反転
        if (invertX)
        {
            x = -x;
        }


        // 上下反転
        if (invertY)
        {
            y = -y;
        }


        // 感度
        x *= pointerSensitivity;
        y *= pointerSensitivity;


        // 手ブレ除去
        if (Mathf.Abs(x) <
            pointerDeadZone)
        {
            x = 0f;
        }

        if (Mathf.Abs(y) <
            pointerDeadZone)
        {
            y = 0f;
        }


        Vector2 pointerOffset =
            new Vector2(x, y);

        /*
         * 最大移動範囲
         */
        pointerOffset =
            Vector2.ClampMagnitude(
                pointerOffset,
                maximumPointerOffset
            );


        /*
         * 基準位置 + ポインター位置
         */
        targetPosition =
            pointerOrigin +
            pointerOffset;
    }


    // =====================================
    // ゲーム開始・停止
    // =====================================

    public void StartMoving()
    {
        canMove = true;

        targetPosition =
            rb.position;

        if (controlMode == ControlMode.JoyCon)
        {
            if (joycon == null)
            {
                TryGetJoycon();
            }

            if (joycon != null)
            {
                CalibratePointer();
            }
        }
    }


    public void StopMoving()
    {
        canMove = false;

        rb.linearVelocity =
            Vector2.zero;

        targetPosition =
            rb.position;
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

        rb.linearVelocity =
            Vector2.zero;

        rb.position =
            position;

        pointerOrigin =
            position;

        targetPosition =
            position;

        if (controlMode == ControlMode.JoyCon &&
            joycon != null)
        {
            CalibratePointer();
        }
    }


    // =====================================
    // 操作モード変更
    // =====================================

    public void SetMouseMode()
    {
        controlMode =
            ControlMode.Mouse;

        targetPosition =
            rb.position;

        Debug.Log(
            "操作モード：Mouse"
        );
    }


    public void SetJoyConMode()
    {
        controlMode =
            ControlMode.JoyCon;

        targetPosition =
            rb.position;

        if (joycon == null)
        {
            TryGetJoycon();
        }

        if (joycon != null)
        {
            CalibratePointer();
        }

        Debug.Log(
            "操作モード：Joy-Con"
        );
    }


    // =====================================
    // ノックバック
    // =====================================

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

        /*
         * ノックバック後の位置を
         * Joy-Conの新しい基準位置にする
         */
        pointerOrigin =
            rb.position;

        targetPosition =
            rb.position;

        isKnockback = false;

        knockbackCoroutine = null;
    }


    // =====================================
    // Joy-Con振動
    // =====================================

    public void Rumble()
    {
        /*
         * マウスモードでは振動させない
         */
        if (controlMode != ControlMode.JoyCon)
        {
            return;
        }

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