using UnityEngine;

public class UIFollowTarget : MonoBehaviour
{
    [Header("跟隨目標 (拖入主角 PlayerTop)")]
    public Transform target;

    [Header("右上偏移量 (X往右, Y往上)")]
    public Vector3 worldOffset = new Vector3(1.2f, 1.5f, 0f);

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;
        if (cam == null) cam = Camera.main;

        // 計算主角世界座標偏移後轉為螢幕 UI 座標
        Vector3 screenPos = cam.WorldToScreenPoint(target.position + worldOffset);
        transform.position = screenPos;
    }
}