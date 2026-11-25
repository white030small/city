using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class AutoDoor : MonoBehaviour
{
    private static readonly Dictionary<string, AutoDoor> doorRegistry = new Dictionary<string, AutoDoor>();

    [Header("門扉 ID 設定")]
    [Tooltip("本門代號 (例如: H1_MainDoor)")]
    public string doorID;
    [Tooltip("目標門代號 (例如: H2_MainDoor)")]
    public string targetDoorID;

    [Header("互動按鍵與提示")]
    [SerializeField] private KeyCode interactKey = KeyCode.W;
    [SerializeField] private GameObject promptUI;

    [Header("雙界落腳點 (留空會自動抓取子物件)")]
    [Tooltip("現實角色落地點 (通常貼緊地板)")]
    [SerializeField] private Transform realitySpawnPoint;
    [Tooltip("靈界角色落地點 (地圖是反的，通常貼緊天花板)")]
    [SerializeField] private Transform spiritSpawnPoint;

    // 全域快取參照
    private static Transform playerTop;
    private static Transform playerBottom;
    private static CinemachineCamera vcamTop;
    private static CinemachineCamera vcamBottom;

    private bool isPlayerNearby = false;
    private Collider2D triggeringPlayerCol = null;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        // 自動抓取子物件的雙界落腳點
        if (realitySpawnPoint == null)
        {
            Transform rSpawn = transform.Find("SpawnPoint_Reality");
            realitySpawnPoint = rSpawn != null ? rSpawn : transform;
        }

        if (spiritSpawnPoint == null)
        {
            Transform sSpawn = transform.Find("SpawnPoint_Spirit");
            if (sSpawn != null) spiritSpawnPoint = sSpawn;
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
            Teleport();
        }
    }

    private void Teleport()
    {
        if (string.IsNullOrEmpty(targetDoorID))
        {
            Debug.LogWarning($"[AutoDoor] {gameObject.name} 沒有設定 targetDoorID！");
            return;
        }

        if (!doorRegistry.TryGetValue(targetDoorID, out AutoDoor targetDoor))
        {
            Debug.LogError($"[AutoDoor] 找不到目標門扉 ID: {targetDoorID}");
            return;
        }

        EnsureReferences();
        if (playerTop == null) return;

        // 1. 若拖著箱子，強制放手
        PushableBox.ReleaseAllBoxes();

        // 2. 計算現實主角傳送目標
        Vector3 targetRealityPos = targetDoor.GetRealitySpawnPosition();
        Vector3 deltaTop = targetRealityPos - playerTop.position;

        // 3. 計算靈界主角傳送目標（考慮地圖顛倒）
        Vector3 targetSpiritPos;
        if (targetDoor.spiritSpawnPoint != null)
        {
            // 如果目標門有手動指定專門貼在天花板的靈界點，直接使用！
            targetSpiritPos = targetDoor.spiritSpawnPoint.position;
        }
        else
        {
            // 如果沒設定，水平 X 軸跟隨位移，垂直 Y 軸保持靈界與現實原有的相對高度差
            targetSpiritPos = playerBottom != null 
                ? new Vector3(targetRealityPos.x, playerBottom.position.y, playerBottom.position.z) 
                : Vector3.zero;
        }

        Vector3 deltaBottom = playerBottom != null ? (targetSpiritPos - playerBottom.position) : Vector3.zero;

        // 4. 移動角色並清空速度
        playerTop.position = targetRealityPos;
        ResetVelocity(playerTop);

        if (playerBottom != null)
        {
            playerBottom.position = targetSpiritPos;
            ResetVelocity(playerBottom);
        }

        // 5. 通知 Cinemachine 瞬移，避免相機拖曳穿幫
        SnapCinemachineCameras(deltaTop, deltaBottom);
    }

    private void SnapCinemachineCameras(Vector3 deltaTop, Vector3 deltaBottom)
    {
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

    public Vector3 GetRealitySpawnPosition()
    {
        return realitySpawnPoint != null ? realitySpawnPoint.position : transform.position;
    }

    private void ResetVelocity(Transform target)
    {
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 現實主角或靈界主角靠近都可以觸發
        if (collision.CompareTag("Player") || collision.GetComponent<InvertedPlayerController2D>() != null)
        {
            isPlayerNearby = true;
            triggeringPlayerCol = collision;
            if (promptUI != null) promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == triggeringPlayerCol || collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            triggeringPlayerCol = null;
            if (promptUI != null) promptUI.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        // 在 Scene 視窗畫出預覽點，方便你對齊地面與天花板
        Gizmos.color = Color.green;
        Vector3 rPos = realitySpawnPoint != null ? realitySpawnPoint.position : transform.position;
        Gizmos.DrawWireCube(rPos, new Vector3(0.8f, 1.8f, 0.1f));

        Gizmos.color = Color.magenta;
        if (spiritSpawnPoint != null)
        {
            Gizmos.DrawWireCube(spiritSpawnPoint.position, new Vector3(0.8f, 1.8f, 0.1f));
        }
    }
}