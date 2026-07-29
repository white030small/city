using UnityEngine;

public class SimpleChandelier : MonoBehaviour
{
    [Header("外觀切換")]
    public GameObject normalVisual; // 拖入 Normal_Sprite
    public GameObject brokenVisual; // 拖入 Broken_Sprite
    public GameObject triggerZone;  // 拖入 Trigger_Zone

    private Rigidbody2D rb;

    private BoxCollider2D col;
    private bool isFalling = false;
    private bool isBroken = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    // 1. 被主角踩到 Trigger Zone 時發動
    public void StartFalling()
    {
        if (isFalling || isBroken) return;

        isFalling = true;
        rb.bodyType = RigidbodyType2D.Dynamic; // 開始受重力掉落

        if (triggerZone != null)
            triggerZone.SetActive(false); // 關閉觸發區
    }

    // 2. 本身 (吊燈) 撞到地板時發動
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只有在下墜狀態下撞到 Ground 才處理
        if (isFalling && other.CompareTag("Ground"))
        {
            OnHitGround();
        }
    }

    private void OnHitGround()
    {
        isBroken = true;
        isFalling = false;

        // 切換外觀
        if (normalVisual != null) normalVisual.SetActive(false);
        if (brokenVisual != null) brokenVisual.SetActive(true);

        // 停止物理運動並固定
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (col != null)
        {
            col.isTrigger = false; 


            col.size = new Vector2(1.5f, 3f); 
        }
    }
}