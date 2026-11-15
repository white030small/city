using UnityEngine;

public class PlayerSync : MonoBehaviour
{
    [Header("角色目標")]
    [SerializeField] private Transform playerTop;
    [SerializeField] private Transform playerBottom;

    [Header("同步設定")]
    [SerializeField] private bool syncFacingDirection = true;
    [SerializeField] private bool syncJump = true;

    private Rigidbody2D rbTop;
    private Rigidbody2D rbBottom;
    private Animator animTop;
    private Animator animBottom;

    // 快取 Animator 參數 Hash
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

    private bool wasGroundedTop = true;

    void Awake()
    {
        if (playerTop != null)
        {
            rbTop = playerTop.GetComponent<Rigidbody2D>();
            animTop = playerTop.GetComponent<Animator>();
        }

        if (playerBottom != null)
        {
            rbBottom = playerBottom.GetComponent<Rigidbody2D>();
            animBottom = playerBottom.GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (playerTop == null || playerBottom == null) return;

        // 1. 同步面向（左右翻轉）
        if (syncFacingDirection)
        {
            Vector3 bScale = playerBottom.localScale;
            float topDir = Mathf.Sign(playerTop.localScale.x);
            bScale.x = Mathf.Abs(bScale.x) * topDir;
            playerBottom.localScale = bScale;
        }

        // 2. 同步動畫參數
        if (animTop != null && animBottom != null)
        {
            animBottom.SetFloat(SpeedHash, animTop.GetFloat(SpeedHash));
            animBottom.SetBool(IsGroundedHash, animTop.GetBool(IsGroundedHash));
            animBottom.SetBool(IsCrouchingHash, animTop.GetBool(IsCrouchingHash));
            animBottom.SetBool(IsRunningHash, animTop.GetBool(IsRunningHash));
        }
    }

    void FixedUpdate()
    {
        if (playerTop == null || playerBottom == null || rbTop == null || rbBottom == null) return;

        // --- 1. 水平 X 軸貼齊與速度同步 ---
        // 計算水平偏移量，讓下方角色平滑且精確地對齊上方角色的 X 軸
        float deltaX = playerTop.position.x - playerBottom.position.x;
        // 如果有微小偏差，在水平速度上加一點修正力（避免卡牆時暴力穿透）
        float vx = rbTop.linearVelocity.x + (deltaX * 10f);

        // --- 2. 垂直 Y 軸跳躍與重力同步 ---
        float vy = rbBottom.linearVelocity.y;

        if (syncJump)
        {
            // 判斷上方主角是否著地（可透過 Animator 的 IsGrounded 判定）
            bool isGroundedTop = animTop != null ? animTop.GetBool(IsGroundedHash) : (Mathf.Abs(rbTop.linearVelocity.y) < 0.01f);

            // 當上方主角剛離地跳起（上一幀著地，這一幀有正向向上的初速度）
            if (wasGroundedTop && !isGroundedTop && rbTop.linearVelocity.y > 0.1f)
            {
                // 給予完全相同強度的「正向」向上跳躍初速度
                vy = rbTop.linearVelocity.y;
            }

            wasGroundedTop = isGroundedTop;
        }

        // 應用速度到靈界角色的剛體
        rbBottom.linearVelocity = new Vector2(vx, vy);
    }

    void OnDisable()
    {
        // 關閉同步時停止殘留水平速度，保留重力
        if (rbBottom != null)
        {
            rbBottom.linearVelocity = new Vector2(0f, rbBottom.linearVelocity.y);
        }
    }
}