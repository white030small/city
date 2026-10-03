using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class InvertedPlayerController2D : MonoBehaviour
{
    [Header("移動與跳躍參數")]
    [SerializeField] private float walkSpeed = 6f;       // 正常走路速度
    [SerializeField] private float runSpeed = 11f;       // Shift 加速速度
    [SerializeField] private float crouchSpeed = 3.5f;   // 蹲下移動速度
    [SerializeField] private float jumpForce = 14f;      // 向下推離天花板的跳躍力道

    [Header("天花板偵測 (靈界角色貼天花板處)")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Hitbox 設定 (CapsuleCollider2D)")]
    [SerializeField] private CapsuleCollider2D playerCollider;
    [SerializeField] private Vector2 standColliderSize = new Vector2(0.8f, 2.0f);   // 站立尺寸
    [SerializeField] private Vector2 standColliderOffset = new Vector2(0f, 0f);      // 站立中心偏移
    [SerializeField] private Vector2 crouchColliderSize = new Vector2(0.8f, 1.0f);  // 蹲下尺寸
    [SerializeField] private Vector2 crouchColliderOffset = new Vector2(0f, 0.5f);  // 顛倒蹲下時中心往上移

    [Header("狀態監控")]
    public bool canMove = true;
    public bool isGrounded = false;
    public bool isCrouching = false;
    public bool isRunning = false;
    public bool isDraggingObject = false;

    private float originalWalkSpeed;
    private float moveInput;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerDash dashComponent;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        dashComponent = GetComponent<PlayerDash>();

        originalWalkSpeed = walkSpeed;

        if (playerCollider == null)
            playerCollider = GetComponent<CapsuleCollider2D>();

        if (playerCollider != null && standColliderSize == Vector2.zero)
        {
            standColliderSize = playerCollider.size;
            standColliderOffset = playerCollider.offset;
        }

        // 常態開啟物理模擬，並強制重力為負 (吸附天花板)
        rb.simulated = true;
        rb.gravityScale = -Mathf.Abs(rb.gravityScale == 0 ? 1f : rb.gravityScale);
    }

    void Update()
    {
        // 衝刺中交由 PlayerDash 接管物理
        if (dashComponent != null && dashComponent.isDashing) return;

        // 1. 天花板/地面接觸偵測
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        }

        // 2. 蹲下判定 (不可移動或拖移中禁止蹲下)
        bool wantCrouch = canMove && !isDraggingObject && isGrounded && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
        if (wantCrouch != isCrouching)
        {
            SetCrouchState(wantCrouch);
        }

        // 3. 加速奔跑判定 (不可移動、拖移或蹲下時禁止奔跑)
        isRunning = canMove && !isDraggingObject && !isCrouching && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        // 4. 反向跳躍邏輯 (拖移物件與蹲下時禁止起跳)
        if (canMove && !isDraggingObject && !isCrouching && isGrounded && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -jumpForce);
        }

        // 5. 水平移動輸入與翻轉
        if (canMove)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
            if (moveInput > 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (moveInput < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else
        {
            moveInput = 0f;
        }

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // 衝刺中不覆寫移動速度
        if (dashComponent != null && dashComponent.isDashing) return;

        if (canMove)
        {
            float targetSpeed = walkSpeed;

            if (!isDraggingObject && isRunning)
            {
                targetSpeed = runSpeed;
            }
            else if (isCrouching)
            {
                targetSpeed = crouchSpeed;
            }

            rb.linearVelocity = new Vector2(moveInput * targetSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// 控制主角能否移動 (切換世界時由世界管理器呼叫)
    /// </summary>
    public void walk(bool enable)
    {
        canMove = enable;
        if (!enable)
        {
            moveInput = 0f;
            isRunning = false;

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

            // 若在拖移途中失去控制權 (如切換世界)，自動放手
            if (isDraggingObject)
            {
                SetDraggingState(false);
            }
        }
    }

    /// <summary>
    /// 設置物件拖移狀態 (由 InvertedPushableBox 呼叫)
    /// </summary>
    public void SetDraggingState(bool dragging, float slowSpeed = 2f)
    {
        isDraggingObject = dragging;
        if (dragging)
        {
            isRunning = false;
            walkSpeed = slowSpeed;
            if (isCrouching) SetCrouchState(false);
            if (dashComponent != null) dashComponent.isDashing = false;
        }
        else
        {
            walkSpeed = originalWalkSpeed;
        }
    }

    private void SetCrouchState(bool crouching)
    {
        isCrouching = crouching;

        if (playerCollider != null)
        {
            if (isCrouching)
            {
                playerCollider.size = crouchColliderSize;
                playerCollider.offset = crouchColliderOffset;
            }
            else
            {
                playerCollider.size = standColliderSize;
                playerCollider.offset = standColliderOffset;
            }
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsRunning", isRunning);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}