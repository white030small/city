using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BulletUI : MonoBehaviour
{
    [Header("左輪轉盤圖示 (依序放入 0顆 到 5顆 的轉盤子物件)")]
    public GameObject[] gun_bullet; // 長度請設為 6

    [Header("淡出設定")]
    [SerializeField] private float showDuration = 2.0f;
    [SerializeField] private float fadeDuration = 1.0f;

    [Header("左輪旋轉設定")]
    [SerializeField] private float spinDuration = 0.15f;

    private CanvasGroup canvasGroup;
    private int currentAmmoIndex = 5;
    private bool isSpinning = false;
    private float spinTimer = 0f;
    private float showTimer = 0f;
    private bool isFading = false;
    private float fadeTimer = 0f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        UpdateBulletImage(5);
    }

    public void ShowUI(int currentAmmo = 5)
    {
        if (gun_bullet == null || gun_bullet.Length == 0) return;

        currentAmmoIndex = Mathf.Clamp(currentAmmo, 0, gun_bullet.Length - 1);
        UpdateBulletImage(currentAmmoIndex);

        ResetUIState();
        showTimer = showDuration;
    }

    public void ShowAttackUI(int remainingAmmo)
    {
        if (gun_bullet == null || gun_bullet.Length == 0) return;

        currentAmmoIndex = Mathf.Clamp(remainingAmmo, 0, gun_bullet.Length - 1);
        UpdateBulletImage(currentAmmoIndex);

        ResetUIState();
        isSpinning = true;
        spinTimer = 0f;
    }

    public void ShowChangeUI(int newAmmo)
    {
        if (gun_bullet == null || gun_bullet.Length == 0) return;

        currentAmmoIndex = Mathf.Clamp(newAmmo, 0, gun_bullet.Length - 1);
        UpdateBulletImage(currentAmmoIndex);

        ResetUIState();
        showTimer = showDuration;
    }

    void Update()
    {
        if (isSpinning)
        {
            spinTimer += Time.deltaTime;
            float angle = (spinTimer / spinDuration) * 360f;

            if (gun_bullet[currentAmmoIndex] != null)
            {
                gun_bullet[currentAmmoIndex].transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            if (spinTimer >= spinDuration)
            {
                isSpinning = false;
                if (gun_bullet[currentAmmoIndex] != null)
                {
                    gun_bullet[currentAmmoIndex].transform.rotation = Quaternion.identity;
                }
                showTimer = showDuration;
            }
            return;
        }

        if (showTimer > 0f)
        {
            showTimer -= Time.deltaTime;
            if (showTimer <= 0f)
            {
                isFading = true;
                fadeTimer = 0f;
            }
        }

        if (isFading && canvasGroup != null)
        {
            fadeTimer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
            if (fadeTimer >= fadeDuration)
            {
                isFading = false;
            }
        }
    }

    private void UpdateBulletImage(int targetIndex)
    {
        for (int i = 0; i < gun_bullet.Length; i++)
        {
            if (gun_bullet[i] != null)
            {
                gun_bullet[i].SetActive(i == targetIndex);
            }
        }
    }

    private void ResetUIState()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        isFading = false;
        fadeTimer = 0f;
    }
}