using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool isGhostMode = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 初始設定：人類模式
        UpdatePhysics();
    }

    void Update()
    {
        // 2D 移動輸入 (左右: A/D, 上下: W/S)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 movement = new Vector2(moveX, moveY).normalized;

        // 設定速度 (Unity 6 建議使用 linearVelocity)
        rb.linearVelocity = movement * moveSpeed;

        // 按空白鍵切換幽靈模式
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isGhostMode = !isGhostMode;
            UpdatePhysics();
        }
    }

    void UpdatePhysics()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int humanLayer = LayerMask.NameToLayer("HumanWorld");

        // 2D 穿牆核心邏輯
        // 當 isGhostMode 為 true 時，忽略 Player 與 HumanWorld 的碰撞
        Physics2D.IgnoreLayerCollision(playerLayer, humanLayer, isGhostMode);

        // 視覺回饋：幽靈模式變半透明藍色
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = isGhostMode ? new Color(0, 1, 1, 0.5f) : Color.white;
    }
}