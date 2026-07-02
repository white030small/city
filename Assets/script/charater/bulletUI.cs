using UnityEngine;

public class bulletUI : MonoBehaviour
{
    [Header("子彈UI")]

    public GameObject[] gun_bullet;
    public float showtime = 1.3f;
    public bool isFading = false;
    private float fadeDuration = 0.5f; // 淡出花多久
    private float fadeTimer = 0f;
    private CanvasGroup canvasGroup;
    public int gun_time = 5;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // 外部呼叫這個來顯示 UI
    public void ShowUI()
    {
        canvasGroup.alpha = 1f;    // 完全顯示
        showtime = 2f;            // 顯示 2 秒
        isFading = false;
        fadeTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(showtime > 0){
            showtime -= Time.deltaTime;
        }

        gun_time -= 1 ;
        gun_bullet[gun_time + 1].SetActive(false); // 整個物件開關
        gun_bullet[gun_time].SetActive(true); // 整個物件開關
        if (showtime > 0)
        {
            showtime -= Time.deltaTime;
            if (showtime <= 0)
                isFading = true;   // 2 秒到了，開始淡出
        }
        if(isFading)
        {
            fadeTimer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
            if (fadeTimer >= fadeDuration)
            {
                isFading = false;  // 淡出完成
            }
        }
    }
}
