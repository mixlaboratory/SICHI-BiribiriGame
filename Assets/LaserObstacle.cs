using UnityEngine;

public class LaserObstacle : MonoBehaviour
{
    private enum LaserState
    {
        Off,
        Warning,
        Active
    }

    [Header("参照")]
    [SerializeField]
    private SpriteRenderer beamRenderer;

    [SerializeField]
    private BoxCollider2D beamCollider;

    [SerializeField]
    private GameManager gameManager;

    [Header("動作時間")]
    [Tooltip("レーザーが出ている時間")]
    [SerializeField]
    private float activeDuration = 1.5f;

    [Tooltip("レーザーが消えている時間")]
    [SerializeField]
    private float inactiveDuration = 1.5f;

    [Tooltip("発射前に点滅する時間")]
    [SerializeField]
    private float warningDuration = 0.6f;

    [Tooltip("警告点滅の間隔")]
    [SerializeField]
    private float blinkInterval = 0.1f;

    [Header("開始設定")]
    [SerializeField]
    private bool startActive = false;

    [Tooltip("ゲーム開始前や終了後も動作させる")]
    [SerializeField]
    private bool runOutsideGame = false;

    [Header("色")]
    [SerializeField]
    private Color activeColor = Color.red;

    [SerializeField]
    private Color warningColor = new Color(1f, 0.5f, 0f, 1f);

    [Header("将来の脳波連携用")]
    [SerializeField]
    private float cycleSpeedMultiplier = 1f;

    private LaserState currentState;
    private float stateTimer;
    private float blinkTimer;
    private bool blinkVisible;
    private bool wasRunning;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        ResetLaser();
    }

    private void Update()
    {
        bool canRun =
            runOutsideGame ||
            gameManager == null ||
            gameManager.IsPlaying;

        if (!canRun)
        {
            if (wasRunning)
            {
                ResetLaser();
            }

            wasRunning = false;
            return;
        }

        if (!wasRunning)
        {
            BeginInitialState();
            wasRunning = true;
        }

        float deltaTime =
            Time.deltaTime * Mathf.Max(0.01f, cycleSpeedMultiplier);

        stateTimer -= deltaTime;

        switch (currentState)
        {
            case LaserState.Off:
                UpdateOff();
                break;

            case LaserState.Warning:
                UpdateWarning(deltaTime);
                break;

            case LaserState.Active:
                UpdateActive();
                break;
        }
    }

    private void BeginInitialState()
    {
        if (startActive)
        {
            BeginActive();
        }
        else
        {
            BeginOff();
        }
    }

    private void BeginOff()
    {
        currentState = LaserState.Off;
        stateTimer = inactiveDuration;

        SetBeamVisible(false);
        SetColliderEnabled(false);
    }

    private void BeginWarning()
    {
        currentState = LaserState.Warning;
        stateTimer = warningDuration;
        blinkTimer = 0f;
        blinkVisible = false;

        SetColliderEnabled(false);
        SetBeamVisible(false);
    }

    private void BeginActive()
    {
        currentState = LaserState.Active;
        stateTimer = activeDuration;

        if (beamRenderer != null)
        {
            beamRenderer.color = activeColor;
        }

        SetBeamVisible(true);
        SetColliderEnabled(true);
    }

    private void UpdateOff()
    {
        if (stateTimer <= 0f)
        {
            BeginWarning();
        }
    }

    private void UpdateWarning(float deltaTime)
    {
        blinkTimer -= deltaTime;

        if (blinkTimer <= 0f)
        {
            blinkTimer = blinkInterval;
            blinkVisible = !blinkVisible;

            if (beamRenderer != null)
            {
                beamRenderer.color = warningColor;
            }

            SetBeamVisible(blinkVisible);
        }

        if (stateTimer <= 0f)
        {
            BeginActive();
        }
    }

    private void UpdateActive()
    {
        if (stateTimer <= 0f)
        {
            BeginOff();
        }
    }

    private void SetBeamVisible(bool visible)
    {
        if (beamRenderer != null)
        {
            beamRenderer.enabled = visible;
        }
    }

    private void SetColliderEnabled(bool enabled)
    {
        if (beamCollider != null)
        {
            beamCollider.enabled = enabled;
        }
    }

    public void ResetLaser()
    {
        currentState = LaserState.Off;
        stateTimer = inactiveDuration;
        blinkTimer = 0f;
        blinkVisible = false;

        SetBeamVisible(false);
        SetColliderEnabled(false);
    }

    public void SetCycleSpeed(float multiplier)
    {
        cycleSpeedMultiplier = Mathf.Max(0.01f, multiplier);
    }

    public void SetActiveDuration(float seconds)
    {
        activeDuration = Mathf.Max(0.05f, seconds);
    }

    public void ForceLaser(bool active)
    {
        if (active)
        {
            BeginActive();
        }
        else
        {
            BeginOff();
        }
    }
}