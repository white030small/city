using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("敵人屬性")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;
    [SerializeField] private GameObject animalPrefab; // 死亡後生成的動物/魂魄

    [Header("偵測與移動")]
    [SerializeField] private float detectRange = 5f;     // 追擊偵測半徑
    [SerializeField] private float chaseSpeed = 3.5f;     // 正常追擊速度
    [SerializeField] private float attackRange = 1.2f;    // 攻擊範圍

    [Header("待機巡邏設定")]
    [SerializeField] private bool enablePatrol = true;    // 是否開啟原地巡邏
    [SerializeField] private float patrolRange = 1.5f;    // 左右巡邏範圍
    [SerializeField] private float patrolSpeed = 1.2f;    // 巡邏移動速度

    [Header("攻擊設定")]
    [SerializeField] private int attackDamage = 1;        // 攻擊傷害
    [SerializeField] private float attackCooldown = 1.5f; // 攻擊冷卻時間
    [SerializeField] private float attackDuration = 0.8f; // 攻擊硬直時間 (防護未設動畫事件時的備案)

    [Header("攻擊時間參數 (純程式控制，不需動畫事件)")]
    [SerializeField] private float attackWindup = 0.3f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private Transform currentTargetPlayer;
    private PlayerHealth targetHealth;

    private float cooldownTimer = 0f;
    private float patrolOriginX;
    private int patrolDirection = 1;
    private bool isAttacking = false;
    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        patrolOriginX = transform.position.x;
        FindActivePlayer();
    }

    void Update()
    {
        if (isDead) return;

        // 冷卻計時
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // 確保目標玩家存在且為當前世界的活躍玩家
        if (currentTargetPlayer == null || !currentTargetPlayer.gameObject.activeInHierarchy)
        {
            FindActivePlayer();
            if (currentTargetPlayer == null)
            {
                Patrol();
                return;
            }
        }

        // 攻擊中保持原地靜止
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, currentTargetPlayer.position);

        if (distanceToPlayer <= attackRange && cooldownTimer <= 0f)
        {
            StartAttack();
        }
        else if (distanceToPlayer <= detectRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    /// <summary>
    /// 動態抓取當前世界活躍的主角
    /// </summary>
    private void FindActivePlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            // 優先鎖定有啟用控制器 (當前被操控) 的玩家
            if (p.activeInHierarchy)
            {
                var pc = p.GetComponent<PlayerController>();
                var ipc = p.GetComponent<InvertedPlayerController2D>();

                if ((pc != null && pc.enabled) || (ipc != null && ipc.enabled))
                {
                    currentTargetPlayer = p.transform;
                    targetHealth = p.GetComponent<PlayerHealth>();
                    return;
                }
            }
        }

        // 若無特別狀態，鎖定第一個標籤為 Player 的物件防呆
        GameObject fallbackPlayer = GameObject.FindGameObjectWithTag("Player");
        if (fallbackPlayer != null)
        {
            currentTargetPlayer = fallbackPlayer.transform;
            targetHealth = fallbackPlayer.GetComponent<PlayerHealth>();
        }
    }

    private void ChasePlayer()
    {
        if (currentTargetPlayer == null) return;

        float direction = Mathf.Sign(currentTargetPlayer.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

        // 轉向面對目標
        FaceDirection(direction);

        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    private void Patrol()
    {
        if (!enablePatrol)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (animator != null) animator.SetFloat("Speed", 0f);
            return;
        }

        float targetX = patrolOriginX + (patrolDirection * patrolRange);
        float direction = Mathf.Sign(targetX - transform.position.x);

        rb.linearVelocity = new Vector2(direction * patrolSpeed, rb.linearVelocity.y);
        FaceDirection(direction);

        if (Mathf.Abs(transform.position.x - targetX) < 0.15f)
        {
            patrolDirection *= -1;
        }

        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    private void FaceDirection(float direction)
    {
        if (direction < 0)
            spriteRenderer.flipX = true;  // 朝左
        else if (direction > 0)
            spriteRenderer.flipX = false; // 朝右
    }

    private void StartAttack()
    {
        isAttacking = true;
        cooldownTimer = attackCooldown;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 如果有動畫就播，沒有也完全不影響扣血
        if (animator != null)
            animator.SetTrigger("Attack");

        // 啟動純程式協程：等前搖時間結束後直接扣血，等整個動作結束後解鎖移動
        StartCoroutine(AttackRoutine());
    }
private System.Collections.IEnumerator AttackRoutine()
    {
        // 1. 等待揮刀前搖 (例如 0.3 秒後刀子砍中)
        yield return new WaitForSeconds(attackWindup);

        // 2. 程式自動執行扣血判定 (不再需要動畫事件！)
        ExecuteDamageCheck();

        // 3. 等待攻擊後搖硬直結束 (剩餘時間)
        float remainingTime = Mathf.Max(0.1f, attackDuration - attackWindup);
        yield return new WaitForSeconds(remainingTime);

        // 4. 攻擊結束，敵人恢復移動追擊
        isAttacking = false;
    }
    private void ExecuteDamageCheck()
    {
        if (isDead) return;

        // 如果沒抓到玩家，現場再抓一次防呆
        if (currentTargetPlayer == null)
            FindActivePlayer();

        if (currentTargetPlayer != null)
        {
            // 只要玩家還在攻擊距離內，就直接扣血
            float distance = Vector2.Distance(transform.position, currentTargetPlayer.position);
            if (distance <= attackRange + 0.3f) // 加 0.3f 寬容度，避免玩家微退造成打不到
            {
                if (targetHealth == null)
                    targetHealth = currentTargetPlayer.GetComponent<PlayerHealth>();

                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(attackDamage);
                    Debug.Log("敵人成功擊中玩家，造成傷害！");
                }
            }
        }
    }

    
    private IEnumerator AttackTimeoutRoutine()
    {
        yield return new WaitForSeconds(attackDuration);
        AttackEnd();
    }

    /// <summary>
    /// 掛載在攻擊動畫擊中幀上的 Animation Event
    /// </summary>
    public void DealDamage()
    {
        if (currentTargetPlayer == null || isDead) return;

        float distance = Vector2.Distance(transform.position, currentTargetPlayer.position);
        if (distance <= attackRange)
        {
            if (targetHealth == null)
                targetHealth = currentTargetPlayer.GetComponent<PlayerHealth>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(attackDamage);
            }
        }
    }

    /// <summary>
    /// 掛載在攻擊動畫結束幀上的 Animation Event
    /// </summary>
    public void AttackEnd()
    {
        isAttacking = false;
    }

    /// <summary>
    /// 受到玩家傷害
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        StartCoroutine(HitFlashRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitFlashRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (col != null)
            col.enabled = false;

        if (animator != null)
            animator.SetTrigger("Die");
        else
            SpawnAnimal(); // 若無死亡動畫則直接觸發生成
    }

    /// <summary>
    /// 掛在死亡動畫最後一幀的 Animation Event，或死亡時觸發
    /// </summary>
    public void SpawnAnimal()
    {
        if (animalPrefab != null)
        {
            Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y - 0.3f);
            Instantiate(animalPrefab, spawnPos, Quaternion.identity);
        }

        // 觸發首殺特效
        FirstKillEffect effect = FindAnyObjectByType<FirstKillEffect>();
        if (effect != null)
        {
            effect.TriggerEffect();
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // 紅色：攻擊範圍
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 黃色：追擊偵測範圍
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // 藍色線條：預覽巡邏範圍
        Gizmos.color = Color.cyan;
        float origin = Application.isPlaying ? patrolOriginX : transform.position.x;
        Gizmos.DrawLine(new Vector3(origin - patrolRange, transform.position.y, 0),
                        new Vector3(origin + patrolRange, transform.position.y, 0));
    }
}