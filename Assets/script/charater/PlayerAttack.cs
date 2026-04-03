using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public float attackRange = 1f;      // 攻擊距離
    public float attackCooldown = 0.5f; // 冷卻時間
    public int attackDamage = 1;        // 傷害
    public Transform attackPoint;       // 攻擊判定的中心點
    public LayerMask enemyLayer;        // 敵人的 Layer

    private float cooldownTimer = 0f; //冷卻
    private bool facingRight = true;

    void Update()
    {
        // 冷卻倒數
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // 判斷面朝方向
        float moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput > 0) facingRight = true;//面朝右
        else if (moveInput < 0) facingRight = false;//面朝左

        // 按下攻擊鍵
        if (Input.GetMouseButtonDown(0) && cooldownTimer <= 0)
        {
            Attack();//連接到敵人
            cooldownTimer = attackCooldown;
            Debug.Log("攻擊");
        }
    }

    void Attack()
    {
        // 在攻擊點周圍找所有敵人
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position, 
            attackRange, 
            enemyLayer
        );

        // 對每個打到的敵人造成傷害
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
        }
    }

    // 在 Scene 視窗顯示攻擊範圍（方便調整）
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}