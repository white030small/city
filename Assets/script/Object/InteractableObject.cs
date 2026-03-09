using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private bool isPlayerNearby = false;

    // 當角色進入感應區
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("按 E 鍵進行互動");
        }
    }

    // 當角色離開感應區
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    void Update()
    {
        // 如果玩家在旁邊且按下 E 鍵
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            DoInteract();
        }
    }

    void DoInteract()
    {
        Debug.Log("互動成功！物件啟動了。");
        // 在這裡寫下互動後會發生的事，例如：
        // Destroy(gameObject); // 物件消失（像吃掉金幣）
        // GetComponent<Animator>().SetTrigger("Open"); // 播放開門動畫
    }
}