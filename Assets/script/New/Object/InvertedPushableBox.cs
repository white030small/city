using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class InvertedPushableBox : MonoBehaviour
{
    [Header("拖移設定")]
    [Tooltip("建議設為 1.6 ~ 1.8，避免貼牆且角色站在外側時超出範圍")]
    [SerializeField] private float grabRange = 1.8f;       // 可抓取半徑
    [SerializeField] private float dragSpeed = 2f;         // 拖移時的主角速度
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("提示 UI (選用)")]
    [SerializeField] private GameObject interactPromptUI;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Transform playerTransform;
    private InvertedPlayerController2D invertedPlayer;

    private bool isPlayerNearby = false;
    private bool isDragging = false;
    private float initialBoxToPlayerOffsetX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        // 平常鎖定 X 軸移動與旋轉
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        CheckPlayerProximity();

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

        // 拖移中防呆：若拉開太遠或角色懸空，自動放開
        if (isDragging)
        {
            if (playerTransform == null || 
                Vector2.Distance(transform.position, playerTransform.position) > grabRange * 1.5f ||
                (invertedPlayer != null && !invertedPlayer.isGrounded))
            {
                StopDragging();
            }
        }
    }

    void FixedUpdate()
    {
        // 拖移中：箱子水平座標跟著靈界主角等距移動
        if (isDragging && playerTransform != null)
        {
            float targetX = playerTransform.position.x + initialBoxToPlayerOffsetX;
            rb.position = new Vector2(targetX, rb.position.y);
        }
    }

    private void CheckPlayerProximity()
    {
        if (isDragging) return;

        // 掃描範圍內所有碰撞體，排除 Tilemap 牆壁，專門尋找靈界角色
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, grabRange);
        InvertedPlayerController2D foundPlayer = null;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<InvertedPlayerController2D>(out var player))
            {
                foundPlayer = player;
                playerTransform = hit.transform;
                break;
            }
        }

        if (foundPlayer != null)
        {
            invertedPlayer = foundPlayer;

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
                invertedPlayer = null;
                if (interactPromptUI != null) interactPromptUI.SetActive(false);
            }
        }
    }

    private bool CanStartDragging()
    {
        if (invertedPlayer == null || playerTransform == null) return false;

        // 1. 限制：靈界主角必須吸附在天花板上 (空中禁止拖移)
        if (!invertedPlayer.isGrounded) return false;

        // 2. 限制：靈界角色是貼天花板（顛倒重力）
        // 只有在靈界主角「頭頂/腳底完全站在箱子貼天花板那一側表面」時才禁止拖移
        // 預留 0.1f 彈性緩衝，避免站在箱子身旁時被微小判定誤差誤擋
        if (boxCollider != null)
        {
            Collider2D playerCol = invertedPlayer.GetComponent<Collider2D>();
            float playerTopY = playerCol != null ? playerCol.bounds.max.y : playerTransform.position.y;
            float boxBottomY = boxCollider.bounds.min.y;

            // 若靈界主角整個處於箱子下方緊貼處，視為站在/踩在箱子上
            if (playerTopY < boxBottomY + 0.1f)
            {
                return false;
            }
        }

        return true;
    }

    private void StartDragging()
    {
        if (invertedPlayer == null) return;

        isDragging = true;
        initialBoxToPlayerOffsetX = transform.position.x - playerTransform.position.x;

        // 解鎖 X 軸以允許拖動
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // 通知靈界主角減速與鎖定跳躍、衝刺
        invertedPlayer.SetDraggingState(true, dragSpeed);

        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }

    public void StopDragging()
    {
        if (!isDragging) return;

        isDragging = false;

        // 重新鎖回箱子 X 軸
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        // 恢復靈界主角正常狀態
        if (invertedPlayer != null)
        {
            invertedPlayer.SetDraggingState(false);
        }

        if (interactPromptUI != null && CanStartDragging())
            interactPromptUI.SetActive(true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, grabRange);
    }
}