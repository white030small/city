using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class MirrorDoor : MonoBehaviour
{
    private static readonly Dictionary<string, MirrorDoor> doorRegistry = new Dictionary<string, MirrorDoor>();

    [Header("門扉 ID 設定")]
    [Tooltip("本門代號 (例如: H1_DoorA)")]
    public string doorID;
    [Tooltip("目標門代號 (例如: H2_DoorA)")]
    public string targetDoorID;

    [Header("地圖對稱中心高度 (Y 軸)")]
    [Tooltip("上下兩塊地圖正中央的對稱中心線 Y 座標")]
    [SerializeField] private float mirrorCenterY = -6f; // 填入第一步算出來的數值

    [Header("傳送落點與互動")]
    [SerializeField] private Transform realitySpawnPoint;
    [SerializeField] private KeyCode interactKey = KeyCode.W;
    [SerializeField] private GameObject promptUI;

    // 快取參照
    private static Transform playerTop;
    private static Transform playerBottom;
    private static CinemachineCamera vcamTop;
    private static CinemachineCamera vcamBottom;

    private bool isPlayerNearby = false;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (realitySpawnPoint == null)
        {
            Transform sp = transform.Find("SpawnPoint");
            realitySpawnPoint = sp != null ? sp : transform;
        }

        if (promptUI != null) promptUI.SetActive(false);

        if (!string.IsNullOrEmpty(doorID))
        {
            doorRegistry[doorID] = this;
        }
    }

    void OnDestroy()
    {
        if (!string.IsNullOrEmpty(doorID) && doorRegistry.ContainsKey(doorID))
        {
            doorRegistry.Remove(doorID);
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            TeleportBothWorlds();
        }
    }

    private void TeleportBothWorlds()
    {
        if (string.IsNullOrEmpty(targetDoorID))
        {
            Debug.LogWarning($"[MirrorDoor] {gameObject.name} 尚未設定 targetDoorID！");
            return;
        }

        if (!doorRegistry.TryGetValue(targetDoorID, out MirrorDoor targetDoor))
        {
            Debug.LogError($"[MirrorDoor] 找不到目標門扉 ID: {targetDoorID}，請確認場景中是否有這扇門！");
            return;
        }

        EnsureReferences();
        if (playerTop == null) return;

        // 1. 若正拉著箱子，強制放手
        PushableBox.ReleaseAllBoxes();

        // 2. 計算現實主角目標座標與位移
        Vector3 newRealityPos = targetDoor.GetSpawnPosition();
        Vector3 deltaTop = newRealityPos - playerTop.position;

        // 3. 【數學鏡像翻轉】：以 mirrorCenterY 為軸，自動換算靈界 Y 座標
        float distFromCenter = newRealityPos.y - mirrorCenterY;
        Vector3 newSpiritPos = new Vector3(newRealityPos.x, mirrorCenterY - distFromCenter, newRealityPos.z);
        Vector3 deltaBottom = playerBottom != null ? (newSpiritPos - playerBottom.position) : Vector3.zero;

        // 4. 移動角色並歸零速度
        playerTop.position = newRealityPos;
        ResetVelocity(playerTop);

        if (playerBottom != null)
        {
            playerBottom.position = newSpiritPos;
            ResetVelocity(playerBottom);
        }

        // 5. 通知 Cinemachine 瞬移 (第 0 幀對齊，防止鏡頭拉扯飛越地圖)
        if (vcamTop != null)
        {
            vcamTop.OnTargetObjectWarped(playerTop, deltaTop);
            vcamTop.ForceCameraPosition(playerTop.position, vcamTop.transform.rotation);
        }
        if (vcamBottom != null && playerBottom != null)
        {
            vcamBottom.OnTargetObjectWarped(playerBottom, deltaBottom);
            vcamBottom.ForceCameraPosition(playerBottom.position, vcamBottom.transform.rotation);
        }
    }

    public Vector3 GetSpawnPosition()
    {
        return realitySpawnPoint != null ? realitySpawnPoint.position : transform.position;
    }

    private void ResetVelocity(Transform target)
    {
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void EnsureReferences()
    {
        if (playerTop == null)
        {
            var p = FindFirstObjectByType<PlayerController>();
            if (p != null) playerTop = p.transform;
        }
        if (playerBottom == null)
        {
            var sp = FindFirstObjectByType<InvertedPlayerController2D>();
            if (sp != null) playerBottom = sp.transform;
        }
        if (vcamTop == null)
        {
            GameObject obj = GameObject.Find("Vcam_Top");
            if (obj != null) vcamTop = obj.GetComponent<CinemachineCamera>();
        }
        if (vcamBottom == null)
        {
            GameObject obj = GameObject.Find("Vcam_Bottom");
            if (obj != null) vcamBottom = obj.GetComponent<CinemachineCamera>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (promptUI != null) promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (promptUI != null) promptUI.SetActive(false);
        }
    }
}