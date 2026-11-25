using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("敵人設定")]
    public int health = 3;//血量上限
    public GameObject animalPrefab;//死掉的動物(還需要修)

    [Header("偵測設定")]
    public float detectRange = 1f;    // 多遠看到玩家
    public float chaseSpeed = 0.02f;   // 追擊速度

    [Header("待機晃動")]
    public float idleMoveRange = 0.5f;  // 晃動幅度
    public float idleMoveSpeed = 0.3f;  // 晃動速度

    private Animator animator;//敵人動畫
    private Rigidbody2D rb;//敵人的碰撞箱
    private SpriteRenderer spriteRenderer;
    private Transform player;//主角的位置
    private bool isDead = false;
    private float idleOriginX;
    private int idleDirection = 1;
    private bool isAttacking = false;

    [Header("攻擊")]
    public float Attackrange = 1f;
    public blood blood;
    public float attackCooldown = 1f; // 冷卻時間
    private float cooldownTimer = 0f; //冷卻


    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");//找到是玩家tag的角色
        if (playerObj != null)
            player = playerObj.transform;//找到角色的位置

        idleOriginX = transform.position.x;
    }

    void Update()
    {
        // 冷卻倒數
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (isDead || isAttacking) return;//死亡或攻擊中就停

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);//敵人跟主角的距離


        if (distanceToPlayer <= Attackrange && cooldownTimer <= 0)
        {
            cooldownTimer = attackCooldown;
            Attack();//攻擊（最近才打）
        }
        else if (distanceToPlayer <= detectRange)//小於設定值就追擊
        {
            Chase();//追擊
        }
    }

    void Chase()
    {
        float direction = Mathf.Sign(player.position.x - transform.position.x);//算出左右(左:-1，右:1)
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);//往角色方向追擊

        // 面朝玩家
        if (direction < 0)
            spriteRenderer.flipX = true;   // 面朝左，翻轉圖片
        else
            spriteRenderer.flipX = false;  // 面朝右，不翻轉
    }

    void IdleMove()//(可能後續切成動畫)
    {
        float targetX = idleOriginX + (idleDirection * idleMoveRange);
        float direction = Mathf.Sign(targetX - transform.position.x);

        rb.linearVelocity = new Vector2(direction * idleMoveSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(transform.position.x - targetX) < 0.1f)
        {
            idleDirection *= -1;
        }
    }

    void Attack()
    {
        //Debug.Log("Attack 被呼叫了");
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;//攻擊時停下來
        animator.SetTrigger("Attack");
        Invoke("AttackEnd", 1f); // 1秒後結束攻擊狀態，根據你動畫長度調整
    }

    // 掛在攻擊動畫打中的那一幀，用 Animation Event 呼叫
    public void DealDamage()
    {
        // 先確認玩家還在攻擊範圍內才扣血
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= Attackrange)
        {
            blood.damage(1);
        }
    }

    // 掛在攻擊動畫最後一幀，用 Animation Event 呼叫
    public void AttackEnd()
    {
        isAttacking = false;
    }

    public void TakeDamage(int damage)//傷害計算
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;//停止移動
        rb.bodyType = RigidbodyType2D.Kinematic;//把角色的重力停止
        animator.SetTrigger("Die");//切到死亡動畫
        GetComponent<Collider2D>().enabled = false;//碰撞箱關閉
    }

    public void SpawnAnimal()
    {
        if (animalPrefab != null)
        {
            Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y - 0.5f);
            Instantiate(animalPrefab, spawnPos, Quaternion.identity);
        }
        
        // 觸發第一次殺敵特效
        FirstKillEffect effect = FindFirstObjectByType<FirstKillEffect>();
        if (effect != null)
        {
            effect.TriggerEffect();
        }
        Destroy(gameObject);
    }
}