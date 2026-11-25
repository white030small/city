using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FirstKillEffect : MonoBehaviour
{
    public Volume volume;
    public float riseTime = 0.3f;
    public float holdTime = 2f;
    public float fadeTime = 1.5f;
    public float slowMotionScale = 0.3f;

    [Header("特效強度")]
    public float maxChromatic = 1f;        // 色彩偏移
    public float maxVignette = 0.6f;       // 邊緣變暗
    public float maxDistortion = -0.5f;    // 鏡頭扭曲（負值是膨脹）
    public float maxSaturation = -60f;     // 飽和度降低（畫面變灰）

    private ChromaticAberration chromatic; //色彩偏移
    private Vignette vignette; //邊緣變暗
    private LensDistortion distortion; //鏡頭扭曲
    private ColorAdjustments colorAdjust; //顏色調整
    private bool isPlaying = false; 
    private float timer = 0f;
    private static bool hasTriggered = false;

    void Start()
    {
        volume.profile.TryGet(out chromatic); //嘗試找尋
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out distortion);
        volume.profile.TryGet(out colorAdjust);
        ResetAll(); //重製效果
    }

    void Update()
    {
        if (!isPlaying) return;

        timer += Time.unscaledDeltaTime;

        if (timer < riseTime)//快速提升到最大
        {
            float t = timer / riseTime;
            SetIntensity(t);
        }
        else if (timer < riseTime + holdTime)//維持最大不動
        {
            SetIntensity(1f);
        }
        else if (timer < riseTime + holdTime + fadeTime)//緩降回正常
        {
            float t = 1f - (timer - riseTime - holdTime) / fadeTime;
            SetIntensity(t);
            Time.timeScale = Mathf.Lerp(1f, slowMotionScale, t);
        }
        else
        {
            ResetAll();
            Time.timeScale = 1f;
            isPlaying = false;
        }
    }

    // t = 0 是沒效果，t = 1 是最大強度
    void SetIntensity(float t)
    {
        if (chromatic != null)
            chromatic.intensity.value = Mathf.Lerp(0f, maxChromatic, t);

        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(0f, maxVignette, t);

        if (distortion != null)
            distortion.intensity.value = Mathf.Lerp(0f, maxDistortion, t);

        if (colorAdjust != null)
            colorAdjust.saturation.value = Mathf.Lerp(0f, maxSaturation, t);
    }

    void ResetAll()
    {
        if (chromatic != null) chromatic.intensity.value = 0f;
        if (vignette != null) vignette.intensity.value = 0f;
        if (distortion != null) distortion.intensity.value = 0f;
        if (colorAdjust != null) colorAdjust.saturation.value = 0f;
    }

    public void TriggerEffect()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        isPlaying = true;
        timer = 0f;
        Time.timeScale = slowMotionScale;
    }
}