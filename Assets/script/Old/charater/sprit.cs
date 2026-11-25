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

    [Header("蹲下設定")]
    public float crouchSpeedMultiplier = 0.4f;  // 蹲下走路的速度倍率
    private bool isCrouching = false;            // 是否正在蹲下
    private BoxCollider2D col;
    private Vector2 originalSize;
    private Vector2 originalOffset;
    
    [Header("動畫")]
    public Animator animator;

    void Start()
    {
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
        originalSize = col.size;
        originalOffset = col.offset;
    }

    // Update is called once per frame
    void Update()
    {
        moveInputX = Input.GetAxisRaw("Horizontal");  // A/D 或方向鍵的水平輸入
        bool isRunning = Input.GetKey(KeyCode.LeftShift);   // 按住 Shift = 跑步

        // ---- 蹲下：按住 S 鍵或下方向鍵 ----
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (!isCrouching)
                StartCrouch();
        }
        else
        {
            if (isCrouching)
                StopCrouch();
        }

        if (isCrouching)
        {
            if (moveInputX != 0)
            {
                // 蹲著走
                animator.speed = 1;
            }
            else
            {
                // 蹲著不動
                animator.speed = 0;
            }
        }

        if(moveInputX == 0 )
        {
            animator.SetBool("IDLE" , true);
            animator.SetBool("walk" , false);
            animator.SetBool("run" , false);
        }

        if(moveInputX != 0 )
        {
            animator.SetBool("walk" , true);
            animator.SetBool("IDLE" , false);
            animator.SetBool("run" , false);
        }

        if(moveInputX != 0 &&  isRunning)
        {
            animator.SetBool("walk" , false);
            animator.SetBool("IDLE" , false);
            animator.SetBool("run" , true);
        }
        
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

    /// 開始蹲下：之後可以加縮小碰撞箱、切換蹲下動畫
    void StartCrouch()
    {
        isCrouching = true;
        col.size = new Vector2(originalSize.x, originalSize.y * 0.85f);
        animator.SetBool("Crouch", true);
        animator.speed = 0;
        animator.Play("mainchar_downwalk"); 
        // TODO: 動畫做好後加 animator.SetBool("Crouch", true);
        // TODO: 縮小碰撞箱讓角色可以通過矮通道
    }

    /// 結束蹲下：恢復碰撞箱、切回站立動畫
    void StopCrouch()
    {
        isCrouching = false;
        animator.SetBool("Crouch", false);
        col.size = originalSize;
        col.offset = originalOffset;
        animator.speed = 1;
        // TODO: 動畫做好後加 animator.SetBool("Crouch", false);
        // TODO: 恢復碰撞箱大小
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
