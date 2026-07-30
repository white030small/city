using UnityEngine;

public class PastChandelier : MonoBehaviour
{
    [Header("時空狀態")]
    public bool isFixed = false; // 是否已被固定/修好

    [Header("外觀與UI引用")]
    public GameObject fixedVisual; // 被固定時的視覺效果（例如綠色方塊）
    public GameObject interactUI;  // 按鍵提示UI（例如浮在頭上的 "Press E" 方塊/文字）

    [Header("連結現在的吊燈")]
    public SimpleChandelier presentChandelier; // 🌟 用來通知現在的吊燈「我被修好了！」

    private bool isPlayerNearby = false; // 主角是否在附近

    void Start()
    {
        if (interactUI != null) interactUI.SetActive(false);
        if (fixedVisual != null) fixedVisual.SetActive(false);
    }

    void Update()
    {
        // 當主角在附近、還沒被修好、且按下 E 鍵時
        if (isPlayerNearby && !isFixed && Input.GetKeyDown(KeyCode.E))
        {
            FixChandelier();
        }
    }

    public void FixChandelier()
    {
        isFixed = true;

        // 顯示修好的外觀，隱藏提示UI
        if (fixedVisual != null) fixedVisual.SetActive(true);
        if (interactUI != null) interactUI.SetActive(false);

        Debug.Log("【過去世界】吊燈已被修好！");

        // 🌟 核心：通知現在世界的吊燈「復原」！
        if (presentChandelier != null)
        {
            presentChandelier.RestoreLamp();
        }
    }

    // 當主角走進互動範圍
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isFixed)
        {
            isPlayerNearby = true;
            if (interactUI != null) interactUI.SetActive(true); // 顯示 "Press E" 提示
        }
    }

    // 當主角離開互動範圍
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactUI != null) interactUI.SetActive(false); // 隱藏提示
        }
    }
}