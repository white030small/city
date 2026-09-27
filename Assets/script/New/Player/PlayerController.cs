using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移動與跳躍參數")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 11f;
    [SerializeField] private float crouchSpeed = 3.5f;
    [SerializeField] private float jumpForce = 14f;

    [Header("地面偵測")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Hitbox 設定 (以 CapsuleCollider2D 為例)")]
    [SerializeField] private CapsuleCollider2D playerCollider;
    [SerializeField] private Vector2 standColliderSize = new Vector2(0.8f, 2.0f);   // 站立時的尺寸
    [SerializeField] private Vector2 standColliderOffset = new Vector2(0f, 0f);      // 站立時的中心偏移
    [SerializeField] private Vector2 crouchColliderSize = new Vector2(0.8f, 1.0f);  // 蹲下時高度減半
    [SerializeField] private Vector2 crouchColliderOffset = new Vector2(0f, -0.5f);  // 蹲下時中心向下移，確保腳底貼地

    // 狀態控制
    public bool canMove = true;
    public bool isHoldingKnife = true;
    public bool isGrounded = false;
    public bool isCrouching = false;
    public bool isRunning = false;

    [Header("拖移狀態")]
    public bool isDraggingObject = false; // 是否正在拖移箱子/物件
    private float originalWalkSpeed;      // 記錄原始走速

    private Rigidbody2D rb;
    private Animator animator;
    private float moveInput;
    private PlayerDash dashComponent;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        dashComponent = GetComponent<PlayerDash>();

        // 自動抓取 Collider
        if (playerCollider == null)
            playerCollider = GetComponent<CapsuleCollider2D>();

        // 初始化記錄站立尺寸 (若 Inspector 沒特別指定)
        if (playerCollider != null && standColliderSize == Vector2.zero)
        {
            standColliderSize = playerCollider.size;
            standColliderOffset = playerCollider.offset;
        }

        // 記錄初始走路速度
        originalWalkSpeed = walkSpeed;
    }

    void Update()
    {
        if (dashComponent != null && dashComponent.isDashing) return;

        // 地面判定
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        }

        // 1. 蹲下判定 (拖移物件時禁止蹲下)
        bool wantCrouch = canMove && !isDraggingObject && isGrounded && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
        if (wantCrouch != isCrouching)
        {
            SetCrouchState(wantCrouch);
        }

        // 2. 加速奔跑判定 (拖移物件時禁止奔跑加速)
        isRunning = canMove && !isDraggingObject && !isCrouching && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        // 3. 跳躍邏輯 (蹲下與拖移物件時皆【禁止跳躍】)
        if (canMove && !isDraggingObject && !isCrouching && isGrounded && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 4. 水平移動面向
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
        if (dashComponent != null && dashComponent.isDashing) return;

        if (canMove)
        {
            float targetSpeed = walkSpeed;

            if (isCrouching)
                targetSpeed = crouchSpeed;
            else if (isRunning)
                targetSpeed = runSpeed;

            rb.linearVelocity = new Vector2(moveInput * targetSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// 切換是否處於拖拉重物狀態 (由 PushableBox 呼叫)
    /// </summary>
    public void SetDraggingState(bool dragging, float slowSpeed = 2f)
    {
        isDraggingObject = dragging;
        if (dragging)
        {
            walkSpeed = slowSpeed;      // 套用拖移減速
            if (isCrouching) SetCrouchState(false); // 若原本蹲著則強制站起
        }
        else
        {
            walkSpeed = originalWalkSpeed; // 放開箱子時恢復正常跑速
        }
    }

    /// <summary>
    /// 切換蹲下狀態並即時調整 Hitbox 尺寸與偏移
    /// </summary>
    private void SetCrouchState(bool crouching)
    {
        isCrouching = crouching;

        if (playerCollider != null)
        {
            if (isCrouching)
            {
                // 蹲下：高度變矮、中心向下偏移，避免腳底懸空
                playerCollider.size = crouchColliderSize;
                playerCollider.offset = crouchColliderOffset;
            }
            else
            {
                // 站起：恢復站立尺寸與偏移
                playerCollider.size = standColliderSize;
                playerCollider.offset = standColliderOffset;
            }
        }
    }

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
        }
    }

    public void change(bool isKnife)
    {
        isHoldingKnife = isKnife;
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
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}