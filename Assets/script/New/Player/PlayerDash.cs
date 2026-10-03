using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("衝刺參數")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    public bool isDashing = false;
    private bool canDash = true;

    private Rigidbody2D rb;
    private PlayerController normalController;
    private InvertedPlayerController2D invertedController;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        normalController = GetComponent<PlayerController>();
        invertedController = GetComponent<InvertedPlayerController2D>();
    }

    void Update()
    {
        // 1. 若角色不可行動 (例如處於未啟動的另一個世界 canMove == false)，禁止衝刺
        if (!IsActorActiveAndCanMove()) return;

        // 2. 拖移箱子中禁止衝刺
        if (IsDragging()) return;

        // 3. 滑鼠右鍵衝刺觸發
        if (Input.GetMouseButtonDown(1) && canDash && !isDashing)
        {
            StartCoroutine(ExecuteDash());
        }
    }

    /// <summary>
    /// 檢查當前掛載的主角是否為啟用狀態且可移動
    /// </summary>
    private bool IsActorActiveAndCanMove()
    {
        // 若此 Dash 掛在表界角色身上
        if (normalController != null)
        {
            return normalController.canMove;
        }

        // 若此 Dash 掛在靈界角色身上
        if (invertedController != null)
        {
            return invertedController.canMove;
        }

        return true;
    }

    /// <summary>
    /// 檢查是否正在拖移箱子
    /// </summary>
    private bool IsDragging()
    {
        if (normalController != null && normalController.isDraggingObject)
            return true;

        if (invertedController != null && invertedController.isDraggingObject)
            return true;

        return false;
    }

    private System.Collections.IEnumerator ExecuteDash()
    {
        canDash = false;
        isDashing = true;

        // 判斷面向方向 (根據 transform.localScale.x)
        float dashDirection = transform.localScale.x > 0 ? 1f : -1f;

        // 記錄原始重力
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        // 衝刺結束，恢復重力與速度
        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        // 冷卻時間
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}