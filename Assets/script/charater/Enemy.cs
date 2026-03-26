using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("敵人設定")]
    public int health = 3;
    //public Sprite deadSprite; // 死後變成的圖（老鼠/蟑螂）

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 切換成老鼠/蟑螂的圖片
        /*if (deadSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = deadSprite;
        }*/

        // 1 秒後消失
        Destroy(gameObject, 1f);
    }
}