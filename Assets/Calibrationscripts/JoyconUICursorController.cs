using System.Collections.Generic;
using UnityEngine;

public class JoyconUICursorController : MonoBehaviour
{
    [Header("Joy-Con")]
    [SerializeField]
    private int joyconIndex = 0;

    [Header("カーソル")]
    [SerializeField]
    private RectTransform cursor;

    [SerializeField]
    private RectTransform canvasRect;

    [Header("移動設定")]
    [SerializeField]
    private float moveSpeed = 250f;

    [SerializeField]
    private float tiltDeadZone = 6f;

    [SerializeField]
    private float maximumTiltAngle = 30f;

    [SerializeField]
    private float tiltSensitivity = 1f;

    [Header("軸調整")]
    [SerializeField]
    private bool swapAxes = false;

    [SerializeField]
    private bool invertX = false;

    [SerializeField]
    private bool invertY = false;

    private Joycon joycon;

    private Quaternion neutralRotation =
        Quaternion.identity;

    private bool joyconCalibrated = false;

    private float nextSearchTime;


    private void Start()
    {
        if (cursor == null)
        {
            cursor =
                GetComponent<RectTransform>();
        }

        TryGetJoycon();
    }


    private void Update()
    {
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

        if (joycon.state <=
            Joycon.state_.NO_JOYCONS)
        {
            joycon = null;
            joyconCalibrated = false;

            return;
        }

        if (!joyconCalibrated)
        {
            return;
        }

        MoveCursor();
    }


    private void TryGetJoycon()
    {
        if (JoyconManager.Instance == null)
        {
            return;
        }

        List<Joycon> joycons =
            JoyconManager.Instance.j;

        if (joycons == null ||
            joycons.Count == 0)
        {
            return;
        }

        if (joyconIndex < 0 ||
            joyconIndex >= joycons.Count)
        {
            joyconIndex = 0;
        }

        Joycon candidate =
            joycons[joyconIndex];

        if (candidate.state !=
            Joycon.state_.IMU_DATA_OK)
        {
            return;
        }

        joycon = candidate;

        CalibrateJoycon();

        Debug.Log(
            "Joy-Con UIカーソル準備完了"
        );
    }


    private void CalibrateJoycon()
    {
        if (joycon == null)
        {
            return;
        }

        // 現在のJoy-Con姿勢を
        // カーソル停止位置として登録
        neutralRotation =
            joycon.GetVector();

        joyconCalibrated = true;
    }


    private void MoveCursor()
    {
        Quaternion currentRotation =
            joycon.GetVector();

        Quaternion relativeRotation =
            Quaternion.Inverse(
                neutralRotation
            ) *
            currentRotation;

        Vector3 angles =
            relativeRotation.eulerAngles;


        // PlayerControllerと同じ
        // Z軸 → 左右
        float tiltX =
            Mathf.DeltaAngle(
                0f,
                angles.z
            );

        // X軸 → 上下
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


        tiltX =
            ApplyTiltDeadZone(
                tiltX
            );

        tiltY =
            ApplyTiltDeadZone(
                tiltY
            );


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


        Vector2 input =
            new Vector2(
                inputX,
                inputY
            ) *
            tiltSensitivity;


        input =
            Vector2.ClampMagnitude(
                input,
                1f
            );


        cursor.anchoredPosition +=
            input *
            moveSpeed *
            Time.deltaTime;


        ClampCursor();
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

        // デッドゾーンを超えた瞬間に
        // 急に動かないように差し引く
        return Mathf.Sign(angle) *
               (
                   Mathf.Abs(angle) -
                   tiltDeadZone
               );
    }


    private void ClampCursor()
    {
        if (cursor == null ||
            canvasRect == null)
        {
            return;
        }

        Rect canvasArea =
            canvasRect.rect;

        Vector2 position =
            cursor.anchoredPosition;

        float halfWidth =
            cursor.rect.width * 0.5f;

        float halfHeight =
            cursor.rect.height * 0.5f;


        position.x =
            Mathf.Clamp(
                position.x,
                canvasArea.xMin +
                halfWidth,
                canvasArea.xMax -
                halfWidth
            );

        position.y =
            Mathf.Clamp(
                position.y,
                canvasArea.yMin +
                halfHeight,
                canvasArea.yMax -
                halfHeight
            );


        cursor.anchoredPosition =
            position;
    }
}