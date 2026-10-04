using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PickupItem : MonoBehaviour
{
    public enum ItemType
    {
        Health, // 補包
        Ammo    // 彈藥
    }

    [Header("道具設定")]
    [Tooltip("選擇道具類型")]
    [SerializeField] private ItemType itemType = ItemType.Health;

    [Tooltip("補充的數值量 (例如補 20 滴血或 15 發子彈)")]
    [SerializeField] private int amount = 20;

    [Header("動效設定 (選用)")]
    [Tooltip("拾取物是否上下浮動")]
    [SerializeField] private bool enableFloating = true;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatAmplitude = 0.1f;

    [Header("音效與特效 (選用)")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffectPrefab;

    private Vector3 startPosition;
    private bool isCollected = false; // 防止同一幀多次觸發

    void Awake()
    {
        // 自動強制設定為 Trigger，避免擋住主角
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        startPosition = transform.position;
    }

    void Update()
    {
        // 簡單的 2D 漂浮待機效果
        if (enableFloating && !isCollected)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 只有未被拾取且碰觸者為 Player 時才處理
        if (isCollected || !collision.CompareTag("Player")) return;

        bool success = ApplyPickup(collision.gameObject);

        if (success)
        {
            isCollected = true;
            PlayEffects();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 執行補給邏輯。你可以根據專案內的玩家腳本命名調整裡面的方法。
    /// </summary>
    private bool ApplyPickup(GameObject playerObj)
    {
        switch (itemType)
        {
            case ItemType.Health:
                // 1. 嘗試尋找血量腳本 (請替換為你的血量腳本類別名稱，如 PlayerHealth)

                PlayerHealth healthComp = playerObj.GetComponent<PlayerHealth>();
                if (healthComp != null)
                {
                    // 若血量已滿，可以選擇不讓玩家撿：
                    // if (healthComp.currentHealth >= healthComp.maxHealth) return false;

                    healthComp.Heal(amount);
                    Debug.Log($"[Pickup] 玩家回復生命: +{amount}");
                    return true;
                }


                // 尚未串接血量腳本時的除錯輸出：
                Debug.Log($"[Pickup] 拾取補包: +{amount} HP (請解除腳本內 PlayerHealth 註解以生效)");
                return true;

            case ItemType.Ammo:
                // 2. 嘗試尋找射擊/彈藥腳本 (請替換為你的射擊腳本類別名稱，如 PlayerShooting)

                PlayerAttack shootingComp = playerObj.GetComponent<PlayerAttack>();
                if (shootingComp != null)
                {
                    shootingComp.AddAmmo(amount);
                    Debug.Log($"[Pickup] 玩家獲得子彈: +{amount}");
                    return true;
                }


                // 尚未串接射擊腳本時的除錯輸出：
                Debug.Log($"[Pickup] 拾取子彈: +{amount} Ammo (請解除腳本內 PlayerShooting 註解以生效)");
                return true;
        }

        return false;
    }

    private void PlayEffects()
    {
        // 播放音效 (在道具當前座標生成臨時音頻播放，不受 Destroy 影響)
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // 生成拾取粒子或光效
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}