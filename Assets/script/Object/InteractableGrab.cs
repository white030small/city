using UnityEngine;

public class InteractableGrab : MonoBehaviour
{
    private bool isPlayerNearby = false;
    private bool isBeingDragged = false;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private Collider2D objCollider; // 物件本身的碰撞器
    private mainchar playerScript; // 儲存主角腳本的參考

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        objCollider = GetComponent<Collider2D>();

        // 初始化就鎖死 X 軸與旋轉，確保平常推不動，但能受重力掉落
        rb.bodyType = RigidbodyType2D.Dynamic;
        LockPosition(true);
    }

    void Update()
    {
        if (isBeingDragged && playerScript != null && !playerScript.isGrounded)
        {
            StopDragging();
        }

        // 只有按 E 才會切換狀態
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (!isBeingDragged)
            {
                // 【關鍵檢查】只有主角在地上時，才能開始搬運
                if (playerScript != null && playerScript.isGrounded)
                {
                    StartDragging();
                }
            }
            else
            {
                StopDragging();
            }
        }
    }

    void StartDragging()
    {
        isBeingDragged = true;
        transform.SetParent(playerTransform);

        // 抓取時改為 Kinematic 避免物理抖動
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        // 抓取時暫時忽略與玩家的碰撞，防止跳躍時互相推擠
        Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            Physics2D.IgnoreCollision(objCollider, playerCollider, true);
        }
    }

    void StopDragging()
    {
        isBeingDragged = false;

        // 恢復碰撞偵測
        if (playerTransform != null)
        {
            Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(objCollider, playerCollider, false);
            }
        }

        transform.SetParent(null);
        rb.bodyType = RigidbodyType2D.Dynamic;
        LockPosition(true);
    }

    void LockPosition(bool locked)
    {
        if (locked)
        {
            // 鎖定 X 與 旋轉，保留 Y 軸讓它能下墜
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerTransform = other.transform; // 僅記錄位置，不執行 SetParent
            playerScript = other.GetComponent<mainchar>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if(!isBeingDragged)
            {
                playerTransform = null;
                playerScript = null;
            }
        }
    }
}