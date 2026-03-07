using UnityEngine;

public class mainchar : MonoBehaviour
{
    [Header("移動與跳躍設定")]
    public float moveSpeed = 5f;
    public float jumpForce = 6f; // 跳躍的力量
    public float runMultiplier = 1.5f;

    [Header("同步目標")]
    public Rigidbody2D rbReality;   // 現實層的剛體
    public Rigidbody2D rbSpirit;    // 靈界層的剛體

    private float moveInputX;
    public int world = 0 ;//現實:0，靈界:1
    private bool isRunning ;

    void Update()
    {
        // 1. 左右移動輸入 (A/D 或 方向鍵)
        moveInputX = Input.GetAxisRaw("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // 2. 偵測跳躍按鍵 (按下空白鍵)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformJump();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            changeworld();
        }
        
        if(world == 1)
        {
            ApplyHorizontalMovement(rbSpirit);
        }
        else
        {
            ApplyHorizontalMovement(rbSpirit);
            ApplyHorizontalMovement(rbReality);
        }
        
    }
    void changeworld()
    {
        if(world == 0)
        {
            world = 1 ;
        }
        else
        {
            world = 0 ;
        }
    }

    /*void FixedUpdate()
    {
        // 3. 處理左右移動 (使用物理速度)
        ApplyHorizontalMovement(rbReality);
        ApplyHorizontalMovement(rbSpirit);
    }*/

    void PerformJump()
    {
        // 現實層：往上跳 (+Y)
        if (rbReality != null && world == 0)
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
        if (rb != null && isRunning == true)
        {
            // 保持原本的 Y 軸速度，只改變 X 軸速度
            rb.linearVelocity = new Vector2(moveInputX * moveSpeed * runMultiplier, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(moveInputX * moveSpeed , rb.linearVelocity.y);
        }
    }
}