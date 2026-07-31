using UnityEngine;

public class InteractableGrab : MonoBehaviour
{
    private bool isPlayerNearby = false;
    private bool isBeingDragged = false;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private Collider2D objCollider; 
    private mainchar playerScript; 

    private Vector3 dragOffset; // 紀錄固定相對位置

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        objCollider = GetComponent<Collider2D>();

        // 防穿模關鍵：設定為連續碰撞偵測
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        rb.bodyType = RigidbodyType2D.Dynamic;
        LockPosition(true);
    }

    void Update()
    {
        // 玩家離地或跳躍時停止拖移
        if (isBeingDragged && playerScript != null && !playerScript.isGrounded)
        {
            StopDragging();
        }

        // 按 E 鍵觸發拖移/放開
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (!isBeingDragged)
            {
                // 檢查 1：玩家必須在地面上
                // 檢查 2：玩家不能踩在椅子上方（防止站在椅子上推椅子）
                if (playerScript != null && playerScript.isGrounded && !IsPlayerStandingOnTop())
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

    void FixedUpdate()
    {
        // 拖移中：使用 MovePosition 讓相對位置固定，且撞牆硬擋不穿模
        if (isBeingDragged && playerTransform != null)
        {
            Vector2 targetPosition = playerTransform.position + dragOffset;
            rb.MovePosition(targetPosition);
        }
    }

    // 🌟 判斷主角是否踩在椅子上方
    private bool IsPlayerStandingOnTop()
    {
        if (playerTransform == null || objCollider == null) return false;

        // 計算主角與椅子在 X 軸（水平方向）的距離
        float deltaX = Mathf.Abs(playerTransform.position.x - objCollider.bounds.center.x);
        
        // 椅子的半寬度
        float chairHalfWidth = objCollider.bounds.extents.x;

        // 如果主角的 X 座標落在椅子的寬度範圍之內（代表站在椅子的正上或正下）
        // 只有當主角站在椅子兩側（deltaX > chairHalfWidth）時，才算是在旁邊推椅子
        bool isCenterOverlapped = deltaX < (chairHalfWidth * 0.8f);

        return isCenterOverlapped;
    }

    void StartDragging()
    {
        if (playerTransform == null) return;

        isBeingDragged = true;
        
        if (playerScript != null) playerScript.isDraggingObject = true;

        // 紀錄點擊拖移時主角與椅子的相對距離（相對位置固定）
        dragOffset = transform.position - playerTransform.position;

        // 保持 Dynamic，確保 MovePosition 遇到物理牆壁時能被擋住
        rb.bodyType = RigidbodyType2D.Dynamic;
        LockPosition(false);

        // 忽略主角與椅子的碰撞，避免互相排擠
        Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            Physics2D.IgnoreCollision(objCollider, playerCollider, true);
        }
    }

    void StopDragging()
    {
        isBeingDragged = false;

        if (playerScript != null) playerScript.isDraggingObject = false;

        // 恢復主角與椅子的碰撞
        if (playerTransform != null)
        {
            Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(objCollider, playerCollider, false);
            }
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero; // Unity 6 語法，舊版請改為 rb.velocity = Vector2.zero;
        LockPosition(true);
    }

    void LockPosition(bool locked)
    {
        if (locked)
        {
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
            playerTransform = other.transform;
            playerScript = other.GetComponent<mainchar>();
            
            if (playerScript == null)
            {
                playerScript = FindFirstObjectByType<mainchar>();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (!isBeingDragged)
            {
                playerTransform = null;
                playerScript = null;
            }
        }
    }
}