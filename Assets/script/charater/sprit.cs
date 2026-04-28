using UnityEngine;

public class sprit : MonoBehaviour
{
    public mainchar mainchar;
    private SpriteRenderer spriteRenderer;//圖片素材
    private float moveInputX;             // 水平輸入值（-1 = 左, 1 = 右, 0 = 不動）

    public bool isGrounded;               // 是否站在地面上

    [Header("後撤設定")]
    public float dashSpeed = 12f;         // 後撤速度
    public float dashDuration = 0.25f;    // 後撤持續時間（秒）
    public float dashCooldown = 1f;       // 後撤冷卻時間（秒）
    private bool isDashing = false;       // 是否正在後撤中
    private float dashTimer = 0f;         // 後撤持續計時器
    private float dashCooldownTimer = 0f; // 後撤冷卻計時器
    private float dashDirection;          // 後撤方向

    private Collider2D playerCollider;    // 玩家的碰撞箱

    void Start()
    {
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        moveInputX = Input.GetAxisRaw("Horizontal");  // A/D 或方向鍵的水平輸入
        if(moveInputX == -1 )//圖片翻轉
        {
            spriteRenderer.flipX = true;
        }
        if(moveInputX == 1 )
        {
            spriteRenderer.flipX = false;
        }

        // ---- 後撤：按下右鍵，往反方向撤退 ----
        if (Input.GetMouseButtonDown(1) && dashCooldownTimer <= 0 && isGrounded)
        {
            StartDash();
            return;
        }

    }

    /// 開始衝刺：記錄方向、開啟無敵、開始計時
    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (moveInputX != 0)
            dashDirection = -Mathf.Sign(moveInputX);
        else
            dashDirection = -Mathf.Sign(transform.localScale.x);

        // 忽略玩家跟敵人 Layer 之間的碰撞
        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
    }

    void OnCollisionEnter2D(Collision2D collision)//碰地板就回歸
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            mainchar.GetSpritJump(0);
            isGrounded = true;
        }
    }
}
