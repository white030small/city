using UnityEngine;
using Unity.Cinemachine; // Unity 6 專用命名空間

public class DualWorldManager : MonoBehaviour
{
    [Header("角色參照")]
    [SerializeField] private PlayerController playerTop;             // 現實角色
    [SerializeField] private InvertedPlayerController2D playerBottom; // 靈界角色
    [SerializeField] private PlayerAttack playerAttack;              // 現實主角攻擊腳本

    [Header("同步腳本參照")]
    [SerializeField] private PlayerSync playerSync;                  // 角色同步腳本

    [Header("Cinemachine 相機設定")]
    [SerializeField] private CinemachineCamera vcamTop;              // 現實虛擬相機
    [SerializeField] private CinemachineCamera vcamBottom;           // 靈界虛擬相機

    [Header("音效監聽器 (Audio Listener)")]
    [SerializeField] private AudioListener topAudioListener;        // 現實相機上的 AudioListener
    [SerializeField] private AudioListener bottomAudioListener;     // 靈界相機上的 AudioListener

    [Header("當前世界 (0 = 現實, 1 = 靈界)")]
    public int currentWorld = 0;

    private Rigidbody2D rbTop;
    private Rigidbody2D rbBottom;

    void Awake()
    {
        if (playerTop != null) rbTop = playerTop.GetComponent<Rigidbody2D>();
        if (playerBottom != null) rbBottom = playerBottom.GetComponent<Rigidbody2D>();
        if (playerSync == null) playerSync = GetComponent<PlayerSync>();
    }

    void Start()
    {
        UpdateWorldState();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchWorld();
        }
    }

    public void SwitchWorld()
    {
        currentWorld = (currentWorld == 0) ? 1 : 0;
        UpdateWorldState();
    }

    private void UpdateWorldState()
    {
        bool isTop = (currentWorld == 0);

        // 1. 切換操作權限與殘留速度清理
        if (playerTop != null)
        {
            playerTop.enabled = isTop;
            if (!isTop && rbTop != null)
                rbTop.linearVelocity = new Vector2(0f, rbTop.linearVelocity.y);
        }

        if (playerBottom != null)
        {
            playerBottom.enabled = !isTop;
            if (isTop && rbBottom != null)
                rbBottom.linearVelocity = new Vector2(0f, rbBottom.linearVelocity.y);
        }

        // 2. 切換同步腳本
        if (playerSync != null)
        {
            playerSync.enabled = isTop;
        }

        // 3. 切換攻擊判定
        if (playerAttack != null)
        {
            playerAttack.now_world(currentWorld);
        }

        // 4. 利用 Cinemachine Priority 自動過渡鏡頭
        // 優先權高的虛擬相機會自動取得畫面控制權
        if (vcamTop != null && vcamBottom != null)
        {
            vcamTop.Priority = isTop ? 20 : 10;
            vcamBottom.Priority = isTop ? 10 : 20;
        }

        // 5. 動態切換 AudioListener
        if (topAudioListener != null) topAudioListener.enabled = isTop;
        if (bottomAudioListener != null) bottomAudioListener.enabled = !isTop;
    }
}