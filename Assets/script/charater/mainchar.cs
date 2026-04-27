using UnityEngine;

public class mainchar : MonoBehaviour
{
    // ============================
    // 基本移動
    // ============================
    [Header("移動與跳躍設定")]
    public float moveSpeed = 5f;          // 基本移動速度
    public float jumpForce = 6f;          // 跳躍力道
    public float runMultiplier = 1.5f;    // 按 Shift 跑步時的速度倍率
    private float moveInputX;             // 水平輸入值（-1 = 左, 1 = 右, 0 = 不動）
    private bool isRunning;               // 是否正在跑步

    // ============================
    // 蹲下
    // ============================
    [Header("蹲下設定")]
    public float crouchSpeedMultiplier = 0.4f;  // 蹲下走路的速度倍率
    private bool isCrouching = false;            // 是否正在蹲下

    // ============================
    // 衝刺（滑鏟）
    // ============================
    [Header("後撤設定")]
    public float dashSpeed = 12f;         // 後撤速度
    public float dashDuration = 0.25f;    // 後撤持續時間（秒）
    public float dashCooldown = 1f;       // 後撤冷卻時間（秒）
    private bool isDashing = false;       // 是否正在後撤中
    private float dashTimer = 0f;         // 後撤持續計時器
    private float dashCooldownTimer = 0f; // 後撤冷卻計時器
    private float dashDirection;          // 後撤方向

    private Collider2D playerCollider;    // 玩家的碰撞箱

    // ============================
    // 世界切換
    // ============================
    [Header("同步目標")]
    public Rigidbody2D rbReality;         // 現實層的剛體
    public Rigidbody2D rbSpirit;          // 靈界層的剛體

    [Header("世界")]
    public int world = 0;                 // 目前所在世界（0 = 現實, 1 = 靈界）

    // ============================
    // 跳躍
    // ============================
    [Header("跳躍上限")]
    public int maxjump = 1;               // 最大跳躍次數
    public int jumpCount = 0;             // 目前已跳躍次數

    // ============================
    // 其他設定
    // ============================
    [Header("分界線")]
    public float dividerY = 0f;           // 現實與靈界的分界線 Y 座標

    [Header("地面狀態")]
    public bool isGrounded;               // 是否站在地面上

    [Header("拖移狀態")]
    public bool isDraggingObject = false;  // 是否正在搬東西

    [Header("攝像機")]
    public camermover camermover;          // 攝影機控制器

    void Start()
    {
        playerCollider = GetComponent<Collider2D>();
    }


    void Update()
    {
        // ---- 衝刺冷卻倒數 ----
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        // ---- 衝刺中：不接受其他輸入，只跑衝刺邏輯 ----
        if (isDashing)
        {
            UpdateDash();
            return; // 衝刺中跳過所有其他操作
        }

        // ---- 讀取輸入 ----
        moveInputX = Input.GetAxisRaw("Horizontal");  // A/D 或方向鍵的水平輸入
        isRunning = Input.GetKey(KeyCode.LeftShift);   // 按住 Shift = 跑步

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

        // ---- 後撤：按下右鍵，往反方向撤退 ----
        if (Input.GetMouseButtonDown(1) && dashCooldownTimer <= 0 && isGrounded)
        {
            StartDash();
            return;
        }

        // ---- 跳躍：按下空白鍵 ----
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformJump();
        }

        // ---- 切換世界：按下 Tab ----
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            changeworld();
        }

        // ---- 根據目前世界執行移動 ----
        if (world == 1)
        {
            ApplyHorizontalMovement(rbSpirit);   // 靈界：只移動靈體
        }
        else
        {
            ApplyHorizontalMovement(rbReality);  // 現實：移動現實角色
            spritgoreality();                    // 同時同步靈體位置
        }
    }

    // ============================
    // 蹲下相關
    // ============================


    /// 開始蹲下：之後可以加縮小碰撞箱、切換蹲下動畫
    void StartCrouch()
    {
        isCrouching = true;
        // TODO: 動畫做好後加 animator.SetBool("Crouch", true);
        // TODO: 縮小碰撞箱讓角色可以通過矮通道
    }

    /// 結束蹲下：恢復碰撞箱、切回站立動畫
    void StopCrouch()
    {
        isCrouching = false;
        // TODO: 動畫做好後加 animator.SetBool("Crouch", false);
        // TODO: 恢復碰撞箱大小
    }

    // ============================
    // 衝刺（滑鏟）相關
    // ============================

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

    /// 衝刺中每幀更新：強制移動角色、倒數計時
    void UpdateDash()
    {
        dashTimer -= Time.deltaTime;

        // 取得目前操作的剛體
        Rigidbody2D rb = (world == 1) ? rbSpirit : rbReality;

        // 強制往衝刺方向移動，Y 軸保持不變
        if (rb != null)
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, rb.linearVelocity.y);

        // 現實世界時同步靈體
        if (world == 0)
            spritgoreality();

        // 衝刺時間到就結束
        if (dashTimer <= 0)
            EndDash();
    }

    /// 結束衝刺：關閉無敵、恢復正常控制
    void EndDash()
    {
        isDashing = false;

        // 恢復玩家跟敵人之間的碰撞
        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
    }

    // ============================
    // 碰撞偵測
    // ============================


    /// 碰到地面：重置跳躍次數、標記為在地上
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0;
            isGrounded = true;
        }
    }

    // 持續接觸地面：保持在地上的狀態
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }


    /// 離開地面：進入空中狀態
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    // ============================
    // 跳躍
    // ============================

    /// 接收靈體的跳躍次數（從 sprit.cs 呼叫）
    public void GetSpritJump(int num)
    {
        jumpCount = num;
    }


    /// 執行跳躍：檢查次數限制，根據世界決定跳躍方向
    void PerformJump()
    {
        // 蹲下中或搬東西中不能跳
        if (jumpCount >= maxjump || isDraggingObject || isCrouching) return;

        jumpCount++;

        // 現實層：往上跳（+Y）
        if (rbReality != null && world == 0)
        {
            rbReality.linearVelocity = new Vector2(rbReality.linearVelocity.x, jumpForce);
        }

        // 靈界層：往下跳（-Y，因為靈界是鏡像的）
        if (rbSpirit != null)
        {
            rbSpirit.linearVelocity = new Vector2(rbSpirit.linearVelocity.x, -jumpForce);
        }
    }

    // ============================
    // 世界切換
    // ============================

    /// 切換現實與靈界，同時通知攝影機跟隨目標
    void changeworld()
    {
        if (world == 0)
        {
            world = 1;
            camermover.SwitchTarget(true);   // 攝影機跟隨靈體
        }
        else
        {
            world = 0;
            camermover.SwitchTarget(false);  // 攝影機跟隨現實角色
        }
    }

    // ============================
    // 移動
    // ============================

    /// 現實世界時同步靈體位置：X 軸相同，Y 軸以分界線為中心鏡像翻轉
    void spritgoreality()
    {
        float mirroredY = 2 * dividerY - rbReality.position.y;
        rbSpirit.position = new Vector2(rbReality.position.x, mirroredY);
        rbSpirit.linearVelocity = new Vector2(rbReality.linearVelocity.x, -rbReality.linearVelocity.y);
    }

    /// 水平移動：根據跑步、蹲下、搬東西等狀態計算最終速度
    void ApplyHorizontalMovement(Rigidbody2D rb)
    {
        if (rb == null) return;

        // 基本速度：跑步就乘以跑步倍率
        float currentSpeed = isRunning ? moveSpeed * runMultiplier : moveSpeed;

        // 蹲下時速度變慢
        if (isCrouching)
        {
            currentSpeed *= crouchSpeedMultiplier;
        }

        // 搬東西時速度減半
        if (isDraggingObject)
        {
            currentSpeed *= 0.5f;
        }

        rb.linearVelocity = new Vector2(moveInputX * currentSpeed, rb.linearVelocity.y);
    }

}