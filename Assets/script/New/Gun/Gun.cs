using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("子彈屬性")]
    public float speed = 25f;
    public int gunDamage = 1;
    public float lifeTime = 3f;

    [Header("剛體元件 (留空會自動抓取)")]
    public Rigidbody2D bullet;

    private Vector2 moveDirection = Vector2.zero;
    private bool isFired = false;

    void Awake()
    {
        if (bullet == null)
            bullet = GetComponent<Rigidbody2D>();

        if (bullet != null)
        {
            bullet.gravityScale = 0f;
            // 避免受任何物理拖曳力影響
            bullet.linearDamping = 0f; 
        }
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// 模式 1：直射 (左或右)
    /// </summary>
    public void leftorright(bool isLeft)
    {
        moveDirection = isLeft ? Vector2.left : Vector2.right;
        isFired = true;

        // 若剛體存在，直接賦予物理速度
        if (bullet != null)
        {
            bullet.linearVelocity = moveDirection * speed;
        }
    }

    /// <summary>
    /// 模式 2：滑鼠瞄準方向射擊
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;
        isFired = true;

        if (bullet != null)
        {
            bullet.linearVelocity = moveDirection * speed;
        }
    }

    void Update()
    {
        // 核心防護：即使物理引擎失效，Update 的 Transform 位移也保證子彈往前飛！
        if (isFired)
        {
            transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D hit)
    {
        HandleHit(hit.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D hit)
    {
        HandleHit(hit.gameObject);
    }

    private void HandleHit(GameObject hitObj)
    {
        // 忽略主角與其他子彈
        if (hitObj.CompareTag("Player") || hitObj.GetComponent<gun>() != null) 
            return;

        // 擊中敵人
        if (hitObj.TryGetComponent<EnemyController>(out var enemy))
        {
            enemy.TakeDamage(gunDamage);
            Destroy(gameObject);
            return;
        }

        // 擊中地面或牆壁
        if (hitObj.layer == LayerMask.NameToLayer("Ground") || hitObj.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}