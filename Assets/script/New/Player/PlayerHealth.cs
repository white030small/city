using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("血量基礎設定")]
    [SerializeField] private int maxHealth = 4;
    [SerializeField] private int currentHealth;

    [Header("受傷無敵時間 (i-Frames)")]
    [SerializeField] private float invincibilityDuration = 1.0f; // 受傷後的無敵秒數
    [SerializeField] private float flashInterval = 0.1f;        // 受傷閃爍間隔
    private bool isInvincible = false;

    [Header("UI 血量物件 (films)")]
    [Tooltip("請依序拖入血量圖示 (Element 0 為第 1 滴血)")]
    public GameObject[] films;

    [Header("受傷與死亡事件 (選用)")]
    public UnityEvent onDamaged;
    public UnityEvent onDeath;

    private SpriteRenderer spriteRenderer;
    private Animator playerAnimator;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        // 確保初始血量不超過 UI 陣列上限
        if (films != null && films.Length > 0 && maxHealth > films.Length)
        {
            maxHealth = films.Length;
        }

        currentHealth = maxHealth;
        InitializeUI();
    }

    /// <summary>
    /// 受到傷害
    /// </summary>
    /// <param name="amount">扣除血量</param>
    public void TakeDamage(int amount)
    {
        if (isInvincible || currentHealth <= 0 || amount <= 0) return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - amount);

        // 播放扣除對應血塊的燃燒/消失動畫
        PlayDamageUIAnimation(currentHealth, previousHealth);

        onDamaged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 觸發無敵時間與角色閃爍
            StartCoroutine(InvincibilityRoutine());
        }
    }

    /// <summary>
    /// 回復血量
    /// </summary>
    public void Heal(int amount)
    {
        if (currentHealth <= 0 || amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUIImmediate();
    }

    /// <summary>
    /// 擴充血量上限
    /// </summary>
    public void IncreaseMaxHealth(int amount)
    {
        if (amount <= 0) return;

        int allowedMax = (films != null) ? films.Length : 10;
        maxHealth = Mathf.Min(allowedMax, maxHealth + amount);
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUIImmediate();
    }

    private void InitializeUI()
    {
        UpdateUIImmediate();
    }

    /// <summary>
    /// 即時同步血量圖示狀態 (補血或初始化時使用)
    /// </summary>
    private void UpdateUIImmediate()
    {
        if (films == null) return;

        for (int i = 0; i < films.Length; i++)
        {
            if (films[i] != null)
            {
                films[i].SetActive(i < currentHealth);
            }
        }
    }

    /// <summary>
    /// 只對本次「失去的那幾滴血」播放燃燒動畫，避免重複觸發
    /// </summary>
    private void PlayDamageUIAnimation(int newHealth, int oldHealth)
    {
        if (films == null) return;

        for (int i = newHealth; i < oldHealth; i++)
        {
            if (i >= 0 && i < films.Length && films[i] != null)
            {
                if (films[i].activeSelf)
                {
                    if (films[i].TryGetComponent<Animator>(out var anim))
                    {
                        anim.SetTrigger("Burn");
                    }
                    else
                    {
                        // 若無 Animator 則直接隱藏防呆
                        films[i].SetActive(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 受傷後的無敵協程與精靈閃爍反饋
    /// </summary>
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                // 透明度在 0.2 與 1.0 之間交替切換閃爍
                Color c = spriteRenderer.color;
                c.a = (c.a == 1f) ? 0.2f : 1f;
                spriteRenderer.color = c;
            }

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // 結束無敵並還原透明度
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        isInvincible = false;
    }

    /// <summary>
    /// 角色死亡處理
    /// </summary>
    private void Die()
    {
        // 確保角色控制關閉，避免死後還能移動
        if (TryGetComponent<PlayerController>(out var pc)) pc.enabled = false;
        if (TryGetComponent<PlayerAttack>(out var pa)) pa.enabled = false;

        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Die");
        }

        onDeath?.Invoke();

        // 延遲一點點時間再暫停遊戲或開啟 GameOver 畫面，避免瞬間黑畫面/凍結
        StartCoroutine(HandleDeathSequence());
    }

    private IEnumerator HandleDeathSequence()
    {
        // 使用真實時間等待 0.8 秒讓死亡動作播放
        yield return new WaitForSecondsRealtime(0.8f);
        Time.timeScale = 0f;
    }

    // 取得當前血量狀態 (供外部系統讀取)
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}