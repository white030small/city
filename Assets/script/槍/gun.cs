using UnityEngine;

public class gun : MonoBehaviour
{
    public bool left = false;
    public Rigidbody2D bullet;
    public float speed = 50f ;
    public int gunDamage = 10;
    private SpriteRenderer spriteRenderer;//圖片素材

    public void leftorright(bool where)
    {
        left = where;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bullet = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(left){
            spriteRenderer.flipX = true;
            bullet.linearVelocity = new Vector2(-speed, 0);
        }
        else{
            bullet.linearVelocity = new Vector2(speed, 0);
        }
        Destroy(gameObject, 0.1f); // 2秒後自動消失
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
