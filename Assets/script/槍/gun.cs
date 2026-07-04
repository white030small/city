using UnityEngine;

public class gun : MonoBehaviour
{
    public bool left = false;
    public Rigidbody2D bullet;
    public float speed = 50f ;
    public int gunDamage = 10;
    private SpriteRenderer spriteRenderer;//圖片素材
    private bool directionSet = false;
    public void leftorright(bool where)
    {
        left = where;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (directionSet) return; // 已經設定過就跳過

        bullet = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if(left){
            spriteRenderer.flipX = true;
            bullet.linearVelocity = new Vector2(-speed, 0);
        }
        else{
            bullet.linearVelocity = new Vector2(speed, 0);
        }
        Destroy(gameObject, 0.2f); // 2秒後自動消失
    }

    public void SetDirection(Vector2 dir)
    {
        bullet = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 讓子彈圖片朝向飛行方向
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;//算出方向的角度，Mathf.Rad2Deg 把弧度轉成角度
        transform.rotation = Quaternion.Euler(0, 0, angle);

        bullet.linearVelocity = dir * speed;//座標 dir[(?,?)]*speed
        directionSet = true; // 標記已經設定過方向
        Destroy(gameObject, 0.3f);
    }

    void OnTriggerEnter2D(Collider2D enemy)
    {
        if(enemy.CompareTag("Enemy_lay") || enemy.CompareTag("Enemy_stand") ||enemy.CompareTag("Enemy_fly")){
            enemy.GetComponent<Enemy>().TakeDamage(gunDamage);
            Destroy(gameObject); // 打到就消失
        }
        if(enemy.CompareTag("Ground")){
            Destroy(gameObject); // 碰到牆壁也消失
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
