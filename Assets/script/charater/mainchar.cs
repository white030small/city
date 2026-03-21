using UnityEngine;

public class mainchar : MonoBehaviour
{
    [Header("移動與跳躍設定")]
    public float moveSpeed = 5f;
    public float jumpForce = 6f; // 跳躍的力量
    public float runMultiplier = 1.5f;
    private float moveInputX;
    private bool isRunning ;

    [Header("同步目標")]
    public Rigidbody2D rbReality;   // 現實層的剛體
    public Rigidbody2D rbSpirit;    // 靈界層的剛體

    [Header("世界")]
    public int world = 0 ;//現實:0，靈界:1

    [Header("跳躍上限")]
    public int maxjump = 1;
    public int jumpCount = 0;
    
    [Header("分界線")]
    public float dividerY = 0f;  // 白線的 Y 座標

    [Header("地面狀態")]
    public bool isGrounded;

    [Header("拖移狀態")]
    public bool isDraggingObject = false;

    [Header("攝像機")]
    public camermover camermover;

    void Update()
    {
        // 1. 左右移動輸入 (A/D 或 方向鍵)
        moveInputX = Input.GetAxisRaw("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // 地面偵測(待更改)

        // 落地時重置跳躍次數


        // 2. 偵測跳躍按鍵 (按下空白鍵)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformJump();
        }

        //切換世界
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            changeworld();
        }
        

        if(world == 1)
        {
            ApplyHorizontalMovement(rbSpirit);//靈魂單獨行動
        }
        else
        {
            ApplyHorizontalMovement(rbReality);
            spritgoreality();//同步行動
        }
        
    }

    void OnCollisionEnter2D(Collision2D collision)//碰地板就回歸
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0;
            isGrounded = true;
        }
    }
    
    //在地上
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    // 空中
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false; // 進入「在空中」狀態
        }
    }

    public void GetSpritJump(int num)
    {
        jumpCount = num ;//連接sprit.cs
    }

    void changeworld()
    {
        if(world == 0)
        {
            world = 1 ;
            camermover.SwitchTarget(true);//連接攝像機
        }
        else
        {
            world = 0 ;
            camermover.SwitchTarget(false);//連接攝像機
        }
    }


    /*void FixedUpdate()
    {
        // 3. 處理左右移動 (使用物理速度)
        ApplyHorizontalMovement(rbReality);
        ApplyHorizontalMovement(rbSpirit);
    }*/

    void spritgoreality()
    {
        // 鏡像 Y：如果現實在線上方 +1，靈體就在線下方 -1
        float mirroredY = 2 * dividerY - rbReality.position.y;
        
        rbSpirit.position = new Vector2(rbReality.position.x, mirroredY);
        rbSpirit.linearVelocity = new Vector2(rbReality.linearVelocity.x, -rbReality.linearVelocity.y);
    }

    void PerformJump()
    {
        if(jumpCount >= maxjump || isDraggingObject ) return;

        jumpCount++;

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

    /*void ApplyHorizontalMovement(Rigidbody2D rb)
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
    */
    void ApplyHorizontalMovement(Rigidbody2D rb)
    {
        if (rb == null) return;

        float currentSpeed = isRunning ? moveSpeed * runMultiplier : moveSpeed;

        // 【新增】如果正在搬東西，速度減半 (可以自己調整 0.5f 這個數值)
        if (isDraggingObject)
        {
            currentSpeed *= 0.5f;
        }

        rb.linearVelocity = new Vector2(moveInputX * currentSpeed, rb.linearVelocity.y);
    }
}