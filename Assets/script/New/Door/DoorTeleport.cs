using System.Collections;
using UnityEngine;
using Unity.Cinemachine; // Unity 6 專用命名空間

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

    [Header("Cinemachine 虛擬相機 (手動拖入或自動尋找)")]
    [SerializeField] private CinemachineCamera vcamTop;
    [SerializeField] private CinemachineCamera vcamBottom;

    [Header("互動按鍵與提示")]
    [SerializeField] private KeyCode interactKey = KeyCode.W;
    [SerializeField] private GameObject interactPromptUI;

    private bool isPlayerNearby = false;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (interactPromptUI != null) interactPromptUI.SetActive(false);

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

        // 1. 傳送前若正拖著箱子，強制放手
        PushableBox.ReleaseAllBoxes();

        // 2. 計算兩者位移量
        Vector3 deltaReality = realityDestination.position - realityPlayer.position;
        Vector3 spiritTargetPos = (spiritDestination != null) 
            ? spiritDestination.position 
            : (spiritPlayer != null ? spiritPlayer.position + deltaReality : Vector3.zero);

        Vector3 deltaSpirit = spiritPlayer != null ? (spiritTargetPos - spiritPlayer.position) : Vector3.zero;

        // 3. 傳送角色並歸零速度
        realityPlayer.position = realityDestination.position;
        ResetVelocity(realityPlayer);

        if (spiritPlayer != null)
        {
            spiritPlayer.position = spiritTargetPos;
            ResetVelocity(spiritPlayer);
        }

        // 4. 【核心修復】通知 Cinemachine 目標瞬移，消除飛躍平滑延遲
        SnapCinemachineCameras(deltaReality, deltaSpirit);
    }

    private void SnapCinemachineCameras(Vector3 deltaReality, Vector3 deltaSpirit)
    {
        // 告知 Vcam_Top 目標產生了瞬間跳躍 (Warp)，忽略 Damping
        if (vcamTop != null)
        {
            vcamTop.OnTargetObjectWarped(realityPlayer, deltaReality);
            vcamTop.ForceCameraPosition(realityPlayer.position, vcamTop.transform.rotation);
        }

        // 告知 Vcam_Bottom 靈界相機瞬間對齊
        if (vcamBottom != null && spiritPlayer != null)
        {
            vcamBottom.OnTargetObjectWarped(spiritPlayer, deltaSpirit);
            vcamBottom.ForceCameraPosition(spiritPlayer.position, vcamBottom.transform.rotation);
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