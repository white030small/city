using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public float attackRange = 1f;      // 攻擊距離
    public float attackCooldown = 0.5f; // 冷卻時間
    public float gun_attackCooldown = 2f; // 冷卻時間
    public int attackDamage = 1;        // 傷害
    public Transform attackPoint;       // 攻擊判定的中心點
    public LayerMask enemyLayer;        // 敵人的 Layer
    public float gun_attackRange = 10f;
    public int gunDamage = 10;
    public int gun_time = 5;

    private float cooldownTimer = 0f; //冷卻
    private bool facingRight = true;
    private float moveInput ;
    private float finalX = 1 ;

    public int type_2 = 1;
    public bool isCrouching = false;
    public bool knife = true;
    public bool gun = false;
    public GameObject bulletPrefab;

    [Header("子彈UI")]
    public bulletUI bulletUI;
    public float reloadTimer = 0f;
    public bool isReloading = false;

    public GameObject crosshair; // Inspector 裡拖進去
    public LineRenderer aimLine; // 線的設定

    [Header("血量UI")]
    public GameObject GunUI;
    public GameObject KniUI;

    [Header("角色")]
    public mainchar mainchar;
    public int world = 0;

    [Header("動畫")]
    public Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if(world == 1) {
            return;
        }
        Debug.DrawRay(attackPoint.position, new Vector2(finalX * gun_attackRange, 0), Color.green);
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
        if (Input.GetMouseButtonDown(0) && cooldownTimer <= 0 && knife == true) 
        {
            knife_Attack();//連接到敵人
            cooldownTimer = attackCooldown;
            Debug.Log("攻擊");
        }

        // 按下攻擊鍵
        if (Input.GetMouseButtonDown(0) && cooldownTimer <= 0 && gun == true && type_2 == 1 && gun_time > 0) 
        {
            gun_Attack();//連接到敵人
            gun_time -= 1;
            bulletUI.ShowAttackUI();
            cooldownTimer = gun_attackCooldown;
            Debug.Log("攻擊");
        }

        if (Input.GetKeyDown(KeyCode.R) && gun == true && gun_time < 5 && !isReloading)
        {
            isReloading = true;
            reloadTimer = 2f; // 第一發等 2 秒
            mainchar.walk(false);
        }

        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0)
            {
                gun_time += 1;
                if (gun_time >= 5)
                {
                    bulletUI.ShowChangeUI();
                    isReloading = false; // 補滿了
                    mainchar.walk(true);
                }
                else
                {
                    bulletUI.ShowChangeUI(); // 更新 UI
                    reloadTimer = 2f; // 繼續補下一發
                }
            }
            return;
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            mainchar.change(true);
            knife = true;
            gun = false;
            KniUI.SetActive(true);
            GunUI.SetActive(false);
        }

        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            mainchar.change(false);
            if (knife == true) // 從刀切到槍
            {
                KniUI.SetActive(false);
                GunUI.SetActive(true);
                bulletUI.ShowUI();
                knife = false;
                gun = true;
                type_2 = 1; // 預設簡單模式
                Debug.Log(type_2);
            }
            else // 已經是槍，切換模式
            {
                bulletUI.ShowUI();
                if(type_2 == 1){
                    type_2 = 2;
                    mainchar.walk(false);
                    crosshair.SetActive(true);//瞄準的圖案
                    aimLine.enabled = true;
                }
                else{
                    mainchar.walk(true);
                    crosshair.SetActive(false);
                    aimLine.enabled = false;
                    type_2 = 1 ;
                }
                Debug.Log(type_2);
            }
        }
        if(type_2 == 2 && gun == true)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);//連接到滑鼠現在位置
            mousePos.z = 0;//z軸固定
            aimLine.SetPosition(0, attackPoint.position); // 起點：槍口
            aimLine.SetPosition(1, mousePos);// 終點：滑鼠位置

            if(Input.GetMouseButtonDown(0) && cooldownTimer <= 0 && type_2 == 2 && gun_time > 0)
            {    
                animator.Play("mainchar_shoot");
                // 算出從角色到滑鼠的方向
                Vector2 direction = (mousePos - attackPoint.position).normalized;
                
                GameObject bullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
                bullet.GetComponent<gun>().SetDirection(direction);//把座標傳到Gun
                
                gun_time -= 1;
                bulletUI.ShowAttackUI();
                cooldownTimer = gun_attackCooldown;
            }
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

    public void now_world(int now)
    {
        world = now;
    }

    void knife_Attack()
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
    
    void gun_Attack()
    {
        animator.Play("mainchar_shoot");
        GameObject bullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
        
        if (finalX < 0)
            bullet.GetComponent<gun>().leftorright(true);   // 往左
        else
            bullet.GetComponent<gun>().leftorright(false);  // 往右

    }
    
    // 在 Scene 視窗顯示攻擊範圍（方便調整）
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}