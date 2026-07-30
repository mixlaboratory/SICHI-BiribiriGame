using UnityEngine;

[ExecuteAlways]
public class WallSizeController : MonoBehaviour
{
    [Header("壁全体のサイズ")]
    [SerializeField]
    private Vector2 wallSize = new Vector2(5f, 0.3f);

    [Header("参照")]
    [SerializeField]
    private SpriteRenderer core;

    [SerializeField]
    private SpriteRenderer glow;

    [SerializeField]
    private BoxCollider2D wallCollider;

    [Header("見た目の調整")]
    [Tooltip("Coreの太さ。壁の当たり判定と同じなら0")]
    [SerializeField]
    private float coreExtraThickness = 0f;

    [Tooltip("GlowをCoreよりどれだけ太くするか")]
    [SerializeField]
    private float glowExtraThickness = 0.3f;

    private void OnValidate()
    {
        ApplySize();
    }

    private void Awake()
    {
        ApplySize();
    }

    public void ApplySize()
    {
        wallSize.x = Mathf.Max(0.01f, wallSize.x);
        wallSize.y = Mathf.Max(0.01f, wallSize.y);

        if (core != null)
        {
            core.size = new Vector2(
                wallSize.x,
                wallSize.y + coreExtraThickness
            );
        }

        if (glow != null)
        {
            glow.size = new Vector2(
                wallSize.x,
                wallSize.y + glowExtraThickness
            );
        }

        if (wallCollider != null)
        {
            wallCollider.size = wallSize;
        }
    }
}
