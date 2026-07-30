using UnityEngine;

public class SimpleChandelier : MonoBehaviour
{
    [Header("時空聯動 (過去的吊燈)")]
    public PastChandelier pastChandelier;

    [Header("外觀切換")]
    public GameObject normalVisual; // 拖入 Normal_Sprite
    public GameObject brokenVisual; // 拖入 Broken_Sprite
    public GameObject triggerZone;  // 拖入 Trigger_Zone

    private Rigidbody2D rb;

    private BoxCollider2D col;
    private bool isFalling = false;
    private bool isBroken = false;

    private Vector3 originalPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        originalPosition = transform.position;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    // 1. 被主角踩到 Trigger Zone 時發動
    public void StartFalling()
    {
        if (isFalling || isBroken) return;

        if (pastChandelier != null && pastChandelier.isFixed)
        {
            Debug.Log("【時空聯動】過去的吊燈已修好，現在的吊燈保持固定！");
            return;
        }

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
    public void RestoreLamp()
    {
        isBroken = false;
        isFalling = false;

        // 1. 飛回原本天花板的位置
        transform.position = originalPosition;

        // 2. 恢復物理狀態為靜止 Kinematic
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 3. 恢復碰撞體為 Trigger（不再擋路）
        if (col != null)
        {
            col.isTrigger = true;
        }

        // 4. 切換外觀：顯示完好外觀，隱藏壞掉殘骸
        if (normalVisual != null) normalVisual.SetActive(true);
        if (brokenVisual != null) brokenVisual.SetActive(false);

        Debug.Log("【時空復原】現在世界的吊燈已重置回天花板！");
    }
}