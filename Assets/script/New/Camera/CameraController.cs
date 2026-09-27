using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("跟隨目標")]
    [SerializeField] private Transform targetReality; // 現實角色 (PlayerTop)
    [SerializeField] private Transform targetSpirit;  // 靈界角色 (PlayerBottom)

    [Header("上下界固定高度 (Y軸)")]
    [SerializeField] private float realityFixedY = 0f;  // 現實世界的鏡頭固定 Y 座標
    [SerializeField] private float spiritFixedY = -12f; // 靈界世界的鏡頭固定 Y 座標

    [Header("相機平滑設定")]
    [SerializeField] private float smoothTimeX = 0.2f;  // 水平跟隨平滑時間
    [SerializeField] private float smoothTimeY = 0.35f; // 上下切換世界平滑時間

    [Header("靈界鏡頭翻轉效果")]
    [SerializeField] private bool flipCameraInSpirit = false; // 是否在靈界把攝影機上下顛倒 (Z軸轉180度)

    // 當前世界狀態：0 = 現實, 1 = 靈界
    public int currentWorld = 0;

    private float velocityX = 0f;
    private float velocityY = 0f;
    private float cameraZ;

    void Awake()
    {
        cameraZ = transform.position.z; // 固定保留 Main Camera 原本的 Z 軸深度 (通常是 -10)
    }

    void LateUpdate()
    {
        // 依據當前世界取得跟隨目標與目標 Y 高度
        Transform currentTarget = (currentWorld == 0) ? targetReality : targetSpirit;
        float targetY = (currentWorld == 0) ? realityFixedY : spiritFixedY;

        if (currentTarget == null) return;

        // 1. 水平 X 軸平滑跟隨角色
        float newX = Mathf.SmoothDamp(transform.position.x, currentTarget.position.x, ref velocityX, smoothTimeX);

        // 2. 垂直 Y 軸平滑移動至該世界的固定高度
        float newY = Mathf.SmoothDamp(transform.position.y, targetY, ref velocityY, smoothTimeY);

        // 更新攝影機位置
        transform.position = new Vector3(newX, newY, cameraZ);

        // 3. 處理畫面顛倒 (選用)
        if (flipCameraInSpirit)
        {
            float targetAngle = (currentWorld == 1) ? 180f : 0f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
        }
    }

    /// <summary>
    /// 切換世界介面 (可由 DualWorldManager 呼叫)
    /// </summary>
    /// <param name="worldIndex">0 為現實，1 為靈界</param>
    public void SetWorld(int worldIndex)
    {
        currentWorld = worldIndex;
    }
}