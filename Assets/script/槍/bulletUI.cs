using UnityEngine;

public class bulletUI : MonoBehaviour
{
    [Header("子彈UI")]

    public GameObject[] gun_bullet;
    public float showtime = 2f;
    public bool isFading = false;
    private float fadeDuration = 4.0f; // 淡出花多久
    private float fadeTimer = 0f;
    private CanvasGroup canvasGroup;
    public int gun_time = 5;

    [Header("旋轉設定")]

    private bool isSpinning = false;
    private float spinDuration = 0.1f;  // 轉一圈花多久
    private float spinTimer = 0f;//基礎時間值

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    public void ShowChangeUI()
    {
        gun_time += 1;
        gun_bullet[gun_time].SetActive(true); // 整個物件開關
        canvasGroup.alpha = 1f;    // 完全顯示
        showtime = 2f;            // 顯示 2 秒
        isFading = false;
        fadeTimer = 0f;
    }

    // 外部呼叫這個來顯示 UI
    public void ShowUI()
    {
        gun_bullet[gun_time].SetActive(true); // 整個物件開關
        canvasGroup.alpha = 1f;    // 完全顯示
        showtime = 2f;            // 顯示 2 秒
        isFading = false;
        fadeTimer = 0f;
    }

    public void ShowAttackUI()
    {
        Debug.Log("gungun");
        gun_time -= 1;
        gun_bullet[gun_time + 1].SetActive(false);
        gun_bullet[gun_time].SetActive(true);
        canvasGroup.alpha = 1f;
        isSpinning = true;
        spinTimer = 0f;
        isFading = false;
        fadeTimer = 0f;
    }

    void Update()
    {
        // 旋轉中
        if (isSpinning)
        {
            spinTimer += Time.deltaTime;
            float angle = (spinTimer / spinDuration) * 360f; //假設0.3秒，0秒時，轉 0/0.3*360 = 0度 ，以此類推到時間結束
            gun_bullet[gun_time].transform.rotation = Quaternion.Euler(0, 0, angle);//轉換角度

            if (spinTimer >= spinDuration)
            {
                // 轉完一圈，開始淡出
                isSpinning = false;
                gun_bullet[gun_time].transform.rotation = Quaternion.identity;
                showtime = 2f;
            }
        }

        // 顯示倒數
        if (!isSpinning && showtime > 0)
        {
            showtime -= Time.deltaTime;
            if (showtime <= 0)
            {
                isFading = true;
            }
        }

        // 淡出
        if (isFading)
        {
            fadeTimer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
            if (fadeTimer >= fadeDuration)
            {
                isFading = false;
                gun_bullet[gun_time].SetActive(false);
            }
        }
    }
}
