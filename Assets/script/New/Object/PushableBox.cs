using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PushableBox : MonoBehaviour
{
    [Header("拖移設定")]
    [Tooltip("建議設為 1.6 ~ 1.8，避免箱子貼牆且角色站在外側時超出範圍")]
    [SerializeField] private float grabRange = 1.8f;       
    [SerializeField] private float dragSpeed = 2f;         
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("提示 UI (選用)")]
    [SerializeField] private GameObject interactPromptUI;


    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Transform playerTransform;
    private PlayerController playerController;

    private bool isPlayerNearby = false;
    private bool isDragging = false;
    private float initialBoxToPlayerOffsetX;

    public static event Action OnWorldSwitchRelease;

    public static void ReleaseAllBoxes()
    {
        OnWorldSwitchRelease?.Invoke();
    }

    void OnEnable()
    {
        OnWorldSwitchRelease += ForceRelease;
    }

    void OnDisable()
    {
        OnWorldSwitchRelease -= ForceRelease;
        ForceRelease();
    }

    private void ForceRelease()
    {
        if (isDragging)
        {
            StopDragging();
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 明確抓取主要外框 BoxCollider2D
        boxCollider = GetComponent<BoxCollider2D>();

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

        // 拖移中防呆
        if (isDragging)
        {
            if (playerTransform == null || 
                Vector2.Distance(transform.position, playerTransform.position) > grabRange * 1.5f ||
                (playerController != null && (!playerController.isGrounded || !playerController.canMove)))
            {
                StopDragging();
            }
        }
    }

    void FixedUpdate()
    {
        if (isDragging && playerTransform != null)
        {
            float targetX = playerTransform.position.x + initialBoxToPlayerOffsetX;
            rb.position = new Vector2(targetX, rb.position.y);
        }
    }

    private void CheckPlayerProximity()
    {
        if (isDragging) return;

        // 抓取半徑內的所有碰撞體，過濾掉牆壁(Tilemap)，只找主角
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, grabRange);
        PlayerController foundPlayer = null;

        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                foundPlayer = h.GetComponent<PlayerController>();
                if (foundPlayer != null)
                {
                    playerTransform = h.transform;
                    playerController = foundPlayer;
                    break;
                }
            }
        }

        if (foundPlayer != null && CanStartDragging())
        {
            isPlayerNearby = true;
            if (interactPromptUI != null) interactPromptUI.SetActive(true);
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

    private bool CanStartDragging()
    {
        if (playerController == null || playerTransform == null) return false;

        // 1. 空中禁止
        if (!playerController.isGrounded) return false;

        // 2. 只有在主角「腳底明顯高於箱子表面」時才判定為站在箱子上
        // 預留 0.2f 的彈性緩衝，防止主角站在箱子旁時因為微小垂直誤差被誤擋
        if (boxCollider != null)
        {
            float boxTopY = boxCollider.bounds.max.y;
            // 抓取主角腳底位置 (若有 groundCheck 更好，這裡以 collider 底部為準)
            Collider2D playerCol = playerController.GetComponent<Collider2D>();
            float playerFeetY = playerCol != null ? playerCol.bounds.min.y : playerTransform.position.y;

            if (playerFeetY > boxTopY - 0.1f)
            {
                return false;
            }
        }

        return true;
    }

    private void StartDragging()
    {
        if (playerController == null) return;

        isDragging = true;
        initialBoxToPlayerOffsetX = transform.position.x - playerTransform.position.x;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerController.SetDraggingState(true, dragSpeed);

        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }

    public void StopDragging()
    {
        if (!isDragging) return;

        isDragging = false;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

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
    }
}