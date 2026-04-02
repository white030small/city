using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("敵人設定")]
    public int health = 3;
    public GameObject animalPrefab;

    [Header("偵測設定")]
    public float detectRange = 1f;    // 多遠看到玩家
    public float chaseSpeed = 0.02f;   // 追擊速度

    [Header("待機晃動")]
    public float idleMoveRange = 0.5f;  // 晃動幅度
    public float idleMoveSpeed = 0.3f;  // 晃動速度

    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private bool isDead = false;
    private float idleOriginX;
    private int idleDirection = 1;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        idleOriginX = transform.position.x;
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectRange)
        {
            Chase();
        }
        else
        {
            IdleMove();
        }
    }

    void Chase()
    {
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

        // 面朝玩家
        spriteRenderer.flipX = direction < 0;
    }

    void IdleMove()
    {
        float targetX = idleOriginX + (idleDirection * idleMoveRange);
        float direction = Mathf.Sign(targetX - transform.position.x);

        rb.linearVelocity = new Vector2(direction * idleMoveSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(transform.position.x - targetX) < 0.1f)
        {
            idleDirection *= -1;
        }
    }

    public void TakeDamage(int damage)
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
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        animator.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;
        SpawnAnimal();
    }

    public void SpawnAnimal()
    {
        if (animalPrefab != null)
        {
            Instantiate(animalPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}