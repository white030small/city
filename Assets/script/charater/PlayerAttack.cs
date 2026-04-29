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
    private float moveInput ;
    private float finalX = 1 ;

    public bool isCrouching = false;

    void Update()
    {
        // 冷卻倒數
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // 判斷面朝方向
        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0){
            facingRight = true;//面朝右
            finalX = moveInput;//紀錄最後面朝的方向
        } 
        else if (moveInput < 0) {
            facingRight = false;//面朝左
            finalX = moveInput;
        }
        // 按下攻擊鍵
        if (Input.GetMouseButtonDown(0) && cooldownTimer <= 0)
        {
            Attack();//連接到敵人
            cooldownTimer = attackCooldown;
            Debug.Log("攻擊");
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
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
            float direction = Mathf.Sign(enemy.transform.position.x - transform.position.x);//算出左右(左:-1，右:1)

            if(direction < 0 && finalX < 0){//面朝左邊
            
                if(isCrouching == true){
                    if(enemy.CompareTag("Enemy_lay") || enemy.CompareTag("Enemy_stand")){
                        enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
                    }
                }

                if(isCrouching == false){
                    if(enemy.CompareTag("Enemy_stand")){
                        enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
                    }
                }
            }
            if(direction > 0 && finalX > 0){//面朝右邊

                if(isCrouching == true){
                    if(enemy.CompareTag("Enemy_lay") || enemy.CompareTag("Enemy_stand")){
                        enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
                    }
                }

                if(isCrouching == false){
                    if(enemy.CompareTag("Enemy_stand")){
                        enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
                    }
                }
            }
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