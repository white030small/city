using UnityEngine;

public class mainchar : MonoBehaviour
{
    [Header("移動與跳躍設定")]
    public float moveSpeed = 5f;
    public float jumpForce = 6f; // 跳躍的力量

    [Header("同步目標")]
    public Rigidbody2D rbReality;   // 現實層的剛體
    public Rigidbody2D rbSpirit;    // 靈界層的剛體

    private float moveInputX;

    void Update()
    {
        // 1. 左右移動輸入 (A/D 或 方向鍵)
        moveInputX = Input.GetAxisRaw("Horizontal");

        // 2. 偵測跳躍按鍵 (按下空白鍵)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformJump();
        }
    }

    void FixedUpdate()
    {
        // 3. 處理左右移動 (使用物理速度)
        ApplyHorizontalMovement(rbReality);
        ApplyHorizontalMovement(rbSpirit);
    }

    void PerformJump()
    {
        // 現實層：往上跳 (+Y)
        if (rbReality != null)
        {
            rbReality.linearVelocity = new Vector2(rbReality.linearVelocity.x, jumpForce);
        }

        // 靈界層：往下跳 (-Y)
        if (rbSpirit != null)
        {
            rbSpirit.linearVelocity = new Vector2(rbSpirit.linearVelocity.x, -jumpForce);
        }
    }

    void ApplyHorizontalMovement(Rigidbody2D rb)
    {
        if (rb == null) return;
        // 保持原本的 Y 軸速度，只改變 X 軸速度
        rb.linearVelocity = new Vector2(moveInputX * moveSpeed, rb.linearVelocity.y);
    }
}