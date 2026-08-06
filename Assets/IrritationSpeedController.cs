using UnityEngine;

public class IrritationSpeedController : MonoBehaviour
{
    [Header("イライラ度")]
    [SerializeField]
    private IrritationManager irritationManager;

    [Header("上下移動する障害物")]
    [SerializeField]
    private VerticalMovingObstacle[] movingObstacles;

    [SerializeField]
    private float minimumMoveSpeed = 1f;

    [SerializeField]
    private float maximumMoveSpeed = 6f;

    [Header("回転する障害物")]
    [SerializeField]
    private RotatingObstacle[] rotatingObstacles;

    [Tooltip("イライラ度0の回転速度")]
    [SerializeField]
    private float minimumRotationSpeed = 45f;

    [Tooltip("イライラ度100の回転速度")]
    [SerializeField]
    private float maximumRotationSpeed = 240f;

    [Header("レーザー")]
    [SerializeField]
    private LaserObstacle[] laserObstacles;

    [Tooltip("イライラ度0のレーザー周期倍率")]
    [SerializeField]
    private float minimumLaserSpeed = 0.7f;

    [Tooltip("イライラ度100のレーザー周期倍率")]
    [SerializeField]
    private float maximumLaserSpeed = 2.5f;

    private void Start()
    {
        if (irritationManager == null)
        {
            irritationManager =
                FindFirstObjectByType<IrritationManager>();
        }

        if (irritationManager == null)
        {
            Debug.LogError(
                "IrritationManagerが見つかりません。",
                this
            );

            enabled = false;
            return;
        }

        irritationManager.OnIrritationChanged +=
            ApplySpeed;

        ApplySpeed(
            irritationManager.CurrentIrritation
        );
    }

    private void ApplySpeed(float irritation)
    {
        // 0～100を0～1へ変換
        float ratio = Mathf.InverseLerp(
            0f,
            100f,
            irritation
        );

        UpdateMovingObstacles(ratio);
        UpdateRotatingObstacles(ratio);
        UpdateLasers(ratio);
    }

    private void UpdateMovingObstacles(float ratio)
    {
        float speed = Mathf.Lerp(
            minimumMoveSpeed,
            maximumMoveSpeed,
            ratio
        );

        foreach (
            VerticalMovingObstacle obstacle
            in movingObstacles
        )
        {
            if (obstacle != null)
            {
                obstacle.SetMoveSpeed(speed);
            }
        }
    }

    private void UpdateRotatingObstacles(float ratio)
    {
        float speed = Mathf.Lerp(
            minimumRotationSpeed,
            maximumRotationSpeed,
            ratio
        );

        foreach (
            RotatingObstacle obstacle
            in rotatingObstacles
        )
        {
            if (obstacle != null)
            {
                obstacle.SetRotationSpeed(speed);
            }
        }
    }

    private void UpdateLasers(float ratio)
    {
        float speed = Mathf.Lerp(
            minimumLaserSpeed,
            maximumLaserSpeed,
            ratio
        );

        foreach (
            LaserObstacle laser
            in laserObstacles
        )
        {
            if (laser != null)
            {
                laser.SetCycleSpeed(speed);
            }
        }
    }

    private void OnDestroy()
    {
        if (irritationManager != null)
        {
            irritationManager.OnIrritationChanged -=
                ApplySpeed;
        }
    }
}