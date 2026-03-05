using UnityEngine;
using TMPro; // 這是為了控制 TextMeshPro
using UnityEngine.UI; // 這是為了控制 Slider

public class GameManager : MonoBehaviour
{
    [Header("數值設定")]
    public float maxSan = 100f;
    public float currentSan;
    public float timeRemaining = 300f; // 5 分鐘
    public float sanDecayRate = 2f;    // 每秒扣多少 SAN

    [Header("UI 連結")]
    public Slider sanSlider;
    public TextMeshProUGUI timerText;

    public bool isInSafeZone = false;

    void Start()
    {
        currentSan = maxSan;
        sanSlider.maxValue = maxSan;
    }

    void Update()
    {
        if (!isInSafeZone)
        {
            // 1. 扣除關卡時間
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerUI();
            }

            // 2. 隨時間扣除 SAN 值
            if (currentSan > 0)
            {   
                currentSan -= sanDecayRate * Time.deltaTime;
                sanSlider.value = currentSan;
            }
            else
            {
                GameOver("精神崩潰...");
            }
        }
    }

    void UpdateTimerUI()
    {
        // 將秒數轉換為 分:秒 格式
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void AddTime(float amount)
    {
        timeRemaining += amount;
    }

    void GameOver(string reason)
    {
        Debug.Log("遊戲結束：" + reason);
        // 這裡以後可以寫跳轉到結局畫面的邏輯
    }
}