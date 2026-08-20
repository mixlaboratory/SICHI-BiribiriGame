using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("追従対象")]
    [SerializeField]
    private Transform target;

    [Header("横方向の設定")]
    [SerializeField]
    private float horizontalOffset = 2f;

    [SerializeField]
    private float smoothTime = 0.2f;

    [Header("X方向の移動範囲")]
    [SerializeField]
    private bool useLimits = true;

    [SerializeField]
    private float minimumX = 0f;

    [SerializeField]
    private float maximumX = 30f;

    private Vector3 velocity;
    private float fixedY;
    private float fixedZ;

    private void Awake()
    {
        // ゲーム開始時のカメラ位置を保存
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        float targetX =
            target.position.x + horizontalOffset;

        if (useLimits)
        {
            targetX = Mathf.Clamp(
                targetX,
                minimumX,
                maximumX
            );
        }

        Vector3 targetPosition = new Vector3(
            targetX,
            fixedY,
            fixedZ
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        // SmoothDampによる微小なY変化も完全に固定
        transform.position = new Vector3(
            transform.position.x,
            fixedY,
            fixedZ
        );
    }
}