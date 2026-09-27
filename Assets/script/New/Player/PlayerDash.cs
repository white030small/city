using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [Header("後撤步設定")]
    [SerializeField] private float stepBackSpeed = 16f;     // 後撤初速
    [SerializeField] private float stepBackDuration = 0.18f; // 後撤滑行持續時間
    [SerializeField] private float stepBackCooldown = 0.6f;  // 後撤冷卻時間

    public bool isDashing = false;
    private bool canDash = true;

    private Rigidbody2D rb;
    private PlayerController playerController;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        // 若角色處於定格瞄準 (模式2) 或蹲下狀態，禁止後撤
        if (playerController != null && (!playerController.canMove || playerController.isCrouching))
            return;

        // 按下滑鼠右鍵 (MouseButton 1) 且冷卻完畢時觸發後撤
        if (Input.GetMouseButtonDown(1) && canDash && !isDashing)
        {
            StartCoroutine(PerformBackstep());
        }
    }

    private IEnumerator PerformBackstep()
    {
        canDash = false;
        isDashing = true;

        // 取得角色當前面向 (X scale 的正負值)
        float facingDir = Mathf.Sign(transform.localScale.x);

        // 後撤方向為面向的「相反方向」：面向右(1)則往左(-1)，面向左(-1)則往右(1)
        float retreatDir = -facingDir;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f; // 後撤期間保持水平位移，不受重力拉扯
        rb.linearVelocity = new Vector2(retreatDir * stepBackSpeed, 0f);

        yield return new WaitForSeconds(stepBackDuration);

        // 還原重力與重設水平慣性
        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        isDashing = false;

        // 冷卻計時
        yield return new WaitForSeconds(stepBackCooldown);
        canDash = true;
    }
}