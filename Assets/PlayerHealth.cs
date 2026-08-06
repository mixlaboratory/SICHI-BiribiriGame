using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP設定")]
    [SerializeField]
    private int maxHP = 3;

    [Header("連続ダメージ防止")]
    [Tooltip("ダメージを受けた後の無敵時間")]
    [SerializeField]
    private float invincibleDuration = 1f;

    [Header("UI")]
    [SerializeField]
    private Text hpText;

    [Header("ゲーム管理")]
    [SerializeField]
    private GameManager gameManager;

    private int currentHP;
    private bool isInvincible;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public bool IsInvincible => isInvincible;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        ResetHealth();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        if (isInvincible)
        {
            return;
        }

        if (gameManager != null && !gameManager.IsPlaying)
        {
            return;
        }

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        UpdateHPText();

        Debug.Log(
            "ダメージ：" + damage +
            "　残りHP：" + currentHP
        );

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibleCoroutine());
    }

    public void ResetHealth()
    {
        StopAllCoroutines();

        currentHP = maxHP;
        isInvincible = false;

        UpdateHPText();
    }

    private void Die()
    {
        isInvincible = false;

        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        yield return new WaitForSeconds(
            invincibleDuration
        );

        isInvincible = false;
    }

    private void UpdateHPText()
    {
        if (hpText != null)
        {
            hpText.text =
                "HP： " +
                currentHP +
                " / " +
                maxHP;
        }
    }
}