using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PushableBox : MonoBehaviour
{
    [Header("拖移設定")]
    [SerializeField] private float grabRange = 1.2f;       // 可抓取半徑
    [SerializeField] private float dragSpeed = 2f;         // 拖移時的主角速度
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("垂直站位限制")]
    [Tooltip("主角 Y 軸若高於箱子中心超過此高度，視為站在物件上，禁止抓取")]
    [SerializeField] private float maxGrabHeightDiff = 0.5f;

    [Header("提示 UI (選用，靠近時顯示 '按 E 拖移')")]
    [SerializeField] private GameObject interactPromptUI;

    private Rigidbody2D rb;
    private Collider2D boxCollider;
    private Transform playerTransform;
    private PlayerController playerController;

    private bool isPlayerNearby = false;
    private bool isDragging = false;
    private float initialBoxToPlayerOffsetX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<Collider2D>();

        // 平常鎖定 X 軸移動，確保主角單純走過去撞不會隨便推動
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        CheckPlayerProximity();

        // 只有符合「在地面」且「非站在箱子上」的有效範圍時，才允許按 E 觸發
        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            if (!isDragging)
            {
                if (CanStartDragging())
                {
                    StartDragging();
                }
            }
            else
            {
                StopDragging();
            }
        }

        // 拖移中的例外脫離檢查 (主角落地狀態失效、被拉開過遠等)
        if (isDragging)
        {
            if (playerTransform == null || 
                Vector2.Distance(transform.position, playerTransform.position) > grabRange * 1.5f ||
                (playerController != null && !playerController.isGrounded))
            {
                StopDragging();
            }
        }
    }

    void FixedUpdate()
    {
        if (isDragging && playerTransform != null)
        {
            // 計算箱子理想的目標 X 座標
            float targetX = playerTransform.position.x + initialBoxToPlayerOffsetX;

            // 計算兩者差距，平滑賦予水平速度（避免瞬間傳送造成物理抖動）
            float diffX = targetX - rb.position.x;
            rb.linearVelocity = new Vector2(diffX * 20f, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// 檢查玩家是否在附近，且是否具備抓取資格
    /// </summary>
    private void CheckPlayerProximity()
    {
        if (isDragging) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, grabRange, LayerMask.GetMask("Default", "Player"));
        if (hit != null && hit.CompareTag("Player"))
        {
            playerTransform = hit.transform;
            playerController = hit.GetComponent<PlayerController>();

            // 必須同時滿足：主角在地面上、且不是站在箱子上方
            if (CanStartDragging())
            {
                isPlayerNearby = true;
                if (interactPromptUI != null) interactPromptUI.SetActive(true);
            }
            else
            {
                isPlayerNearby = false;
                if (interactPromptUI != null) interactPromptUI.SetActive(false);
            }
        }
        else
        {
            if (isPlayerNearby)
            {
                isPlayerNearby = false;
                playerTransform = null;
                playerController = null;
                if (interactPromptUI != null) interactPromptUI.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 核心限制檢查：主角不能在空中、不能站在箱子上方
    /// </summary>
    private bool CanStartDragging()
    {
        if (playerController == null || playerTransform == null) return false;

        // 1. 限制：主角必須在地面上 (空中禁止抓取)
        if (!playerController.isGrounded) return false;

        // 2. 限制：檢查 Y 軸相對位置，防止站在箱子上
        float boxTopY = boxCollider != null ? boxCollider.bounds.max.y : (transform.position.y + maxGrabHeightDiff);
        
        // 若主角中心點明顯高於箱子頂部 (例如主角踩在箱子上)，禁止抓取
        if (playerTransform.position.y > boxTopY)
        {
            return false;
        }

        return true;
    }

    private void StartDragging()
    {
        if (playerController == null) return;

        isDragging = true;
        initialBoxToPlayerOffsetX = transform.position.x - playerTransform.position.x;

        // 解鎖箱子的 X 軸位置鎖定
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // 告知主角控制器進入拖移狀態 (減速、鎖死跳躍)
        playerController.SetDraggingState(true, dragSpeed);

        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }

    public void StopDragging()
    {
        if (!isDragging) return;

        isDragging = false;

        // 重新鎖回箱子 X 軸
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        // 恢復主角原本的狀態與速度
        if (playerController != null)
        {
            playerController.SetDraggingState(false);
        }

        if (interactPromptUI != null && CanStartDragging())
            interactPromptUI.SetActive(true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabRange);

        if (boxCollider != null)
        {
            // 繪製紅線標記箱子頂部高度限制線
            Gizmos.color = Color.red;
            float topY = boxCollider.bounds.max.y;
            Gizmos.DrawLine(new Vector3(transform.position.x - grabRange, topY, 0),
                            new Vector3(transform.position.x + grabRange, topY, 0));
        }
    }
}