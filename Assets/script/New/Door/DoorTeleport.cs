using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DoorTeleport : MonoBehaviour
{
    [Header("傳送目標點")]
    [Tooltip("表界主角落地點")]
    [SerializeField] private Transform realityDestination;
    [Tooltip("靈界主角出生點 (若留空，則自動以相對位移平移)")]
    [SerializeField] private Transform spiritDestination;

    [Header("兩界角色參照 (若未指派會自動尋找)")]
    [SerializeField] private Transform realityPlayer;
    [SerializeField] private Transform spiritPlayer;

    [Header("攝影機設定")]
    [Tooltip("你的 Main Camera，若留空會自動抓取 Camera.main")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("攝影機身上掛的跟隨腳本 (例如 CameraFollow)，請直接拖進來")]
    [SerializeField] private MonoBehaviour cameraFollowScript;

    [Header("互動按鍵與提示")]
    [SerializeField] private KeyCode interactKey = KeyCode.W;
    [SerializeField] private GameObject interactPromptUI;

    private bool isPlayerNearby = false;

    void Awake()
    {
        // 確保門是穿透的觸發器
        GetComponent<Collider2D>().isTrigger = true;
        if (interactPromptUI != null) interactPromptUI.SetActive(false);

        // 自動取得相機
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 自動尋找兩界角色
        if (realityPlayer == null)
        {
            var p = FindFirstObjectByType<PlayerController>();
            if (p != null) realityPlayer = p.transform;
        }
        if (spiritPlayer == null)
        {
            var sp = FindFirstObjectByType<InvertedPlayerController2D>();
            if (sp != null) spiritPlayer = sp.transform;
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            TeleportBothPlayers();
        }
    }

    private void TeleportBothPlayers()
    {
        if (realityDestination == null || realityPlayer == null) return;

        // 計算位移向量 (Delta)
        Vector3 delta = realityDestination.position - realityPlayer.position;

        // 1. 傳送前若正拖著箱子，強制放手
        PushableBox.ReleaseAllBoxes();

        // 2. 傳送表界主角
        realityPlayer.position = realityDestination.position;
        ResetVelocity(realityPlayer);

        // 3. 傳送靈界主角
        if (spiritPlayer != null)
        {
            if (spiritDestination != null)
            {
                spiritPlayer.position = spiritDestination.position;
            }
            else
            {
                spiritPlayer.position += delta;
            }
            ResetVelocity(spiritPlayer);
        }

        // 4. 執行攝影機 0 幀硬切瞬移
        SnapCameraInstant();
    }

    private void SnapCameraInstant()
    {
        if (mainCamera == null) return;

        // 先把相機平滑跟隨腳本暫停，避免它在傳送幀做 Lerp/SmoothDamp 飛行運算
        if (cameraFollowScript != null)
        {
            cameraFollowScript.enabled = false;
        }

        // 直接將攝影機 X, Y 座標改到目的地，保持原本的 Z 深度 (通常為 -10)
        mainCamera.transform.position = new Vector3(
            realityDestination.position.x,
            realityDestination.position.y,
            mainCamera.transform.position.z
        );

        // 啟動協程：等待一幀後再開啟跟隨腳本
        StartCoroutine(ReenableCameraFollow());
    }

    private IEnumerator ReenableCameraFollow()
    {
        // yield return null 會等待至下一影格開始
        yield return null;

        // 下一幀角色與相機都已在新位置，此時開啟就不會再有飛躍效果
        if (cameraFollowScript != null)
        {
            cameraFollowScript.enabled = true;
        }
    }

    private void ResetVelocity(Transform target)
    {
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactPromptUI != null) interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactPromptUI != null) interactPromptUI.SetActive(false);
        }
    }
}