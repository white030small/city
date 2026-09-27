using UnityEngine;

public class PlayerSync : MonoBehaviour
{
    [Header("角色目標")]
    [SerializeField] private Transform playerTop;
    [SerializeField] private Transform playerBottom;

    [Header("同步設定")]
    [SerializeField] private bool syncFacingDirection = true;
    [SerializeField] private bool syncJumpVelocity = true;

    private Rigidbody2D rbTop;
    private Rigidbody2D rbBottom;
    private Animator animTop;
    private Animator animBottom;

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

        // 同步面向
        if (syncFacingDirection)
        {
            Vector3 bScale = playerBottom.localScale;
            float topDir = Mathf.Sign(playerTop.localScale.x);
            bScale.x = Mathf.Abs(bScale.x) * topDir;
            playerBottom.localScale = bScale;
        }

        // 同步動畫參數
        if (animTop != null && animBottom != null)
        {
            animBottom.SetFloat("Speed", animTop.GetFloat("Speed"));
            animBottom.SetBool("IsGrounded", animTop.GetBool("IsGrounded"));
            animBottom.SetBool("IsCrouching", animTop.GetBool("IsCrouching"));
            animBottom.SetBool("IsRunning", animTop.GetBool("IsRunning"));
        }
    }

    void FixedUpdate()
    {
        if (playerTop == null || playerBottom == null) return;

        // 水平位置貼齊
        Vector3 targetPos = playerBottom.position;
        targetPos.x = playerTop.position.x;
        playerBottom.position = targetPos;

        // 水平與跳躍速度同步
        if (rbTop != null && rbBottom != null)
        {
            rbBottom.linearVelocity = new Vector2(rbTop.linearVelocity.x, rbBottom.linearVelocity.y);

            if (syncJumpVelocity && rbTop.linearVelocity.y > 0.1f)
            {
                rbBottom.linearVelocity = new Vector2(rbBottom.linearVelocity.x, -Mathf.Abs(rbTop.linearVelocity.y));
            }
        }
    }

    void OnDisable()
    {
        // 關閉同步時停止殘留速度
        if (rbBottom != null)
        {
            rbBottom.linearVelocity = new Vector2(0f, rbBottom.linearVelocity.y);
        }
    }
}