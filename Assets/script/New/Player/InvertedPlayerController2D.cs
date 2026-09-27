using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class InvertedPlayerController2D : MonoBehaviour
{
    [Header("移動與跳躍參數")]
    [SerializeField] private float walkSpeed = 6f;       // 正常走路速度
    [SerializeField] private float runSpeed = 11f;       // Shift 加速速度
    [SerializeField] private float crouchSpeed = 3.5f;   // 蹲下移動速度
    [SerializeField] private float jumpForce = 14f;      // 反向跳躍力道 (向下推離天花板)

    [Header("天花板/地面偵測 (放在靈界角色貼天花板處)")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Hitbox 設定 (CapsuleCollider2D)")]
    [SerializeField] private CapsuleCollider2D playerCollider;
    [SerializeField] private Vector2 standColliderSize = new Vector2(0.8f, 2.0f);   // 站立尺寸
    [SerializeField] private Vector2 standColliderOffset = new Vector2(0f, 0f);      // 站立中心偏移
    [SerializeField] private Vector2 crouchColliderSize = new Vector2(0.8f, 1.0f);  // 蹲下尺寸 (高度縮短)
    [SerializeField] private Vector2 crouchColliderOffset = new Vector2(0f, 0.5f);  // 顛倒蹲下時中心往上移，貼緊天花板

    // 狀態控制
    public bool canMove = true;
    public bool isGrounded = false;
    public bool isCrouching = false;
    public bool isRunning = false;

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

        // 確保剛體重力為負值 (向上吸附至天花板)
        rb.gravityScale = -Mathf.Abs(rb.gravityScale);
    }

    void Update()
    {
        // 若有衝刺/後撤中則暫不接管
        if (dashComponent != null && dashComponent.isDashing) return;

        // 判定天花板接觸
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        }

        // 1. 蹲下判定 (按住 S 或 下鍵，且在天花板上)
        bool wantCrouch = canMove && isGrounded && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
        if (wantCrouch != isCrouching)
        {
            SetCrouchState(wantCrouch);
        }

        // 2. 加速奔跑判定 (非蹲下時按住 Shift 生效)
        isRunning = canMove && !isCrouching && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        // 3. 跳躍邏輯 (蹲下時禁止起跳，向上/跳躍鍵觸發向「下」跳離天花板)
        if (canMove && !isCrouching && isGrounded && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -jumpForce);
        }

        // 4. 水平轉向與輸入
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

    private void SetCrouchState(bool crouching)
    {
        isCrouching = crouching;

        if (playerCollider != null)
        {
            if (isCrouching)
            {
                // 靈界角色天花板在上方，蹲下縮小高度時中心要往 +Y 移，使頂部繼續貼牢天花板
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

    public void walk(bool enable)
    {
        canMove = enable;
        if (!enable && rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
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