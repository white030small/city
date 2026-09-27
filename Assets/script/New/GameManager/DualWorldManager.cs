using UnityEngine;

public class DualWorldManager : MonoBehaviour
{
    [Header("角色參照")]
    [SerializeField] private PlayerController playerTop;             // 現實角色
    [SerializeField] private InvertedPlayerController2D playerBottom; // 靈界角色
    [SerializeField] private PlayerAttack playerAttack;              // 現實主角攻擊腳本

    [Header("同步腳本參照")]
    [SerializeField] private PlayerSync playerSync;                  // 角色同步腳本

    [Header("攝影機控制器")]
    [SerializeField] private CameraController cameraController;

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
        if (currentWorld == 0)
        {
            // === 現實世界 ===
            if (playerTop != null) playerTop.enabled = true;
            if (playerBottom != null)
            {
                playerBottom.enabled = false;
                if (rbBottom != null) rbBottom.linearVelocity = new Vector2(0f, rbBottom.linearVelocity.y);
            }

            // 開啟同步：靈界角色跟隨現實角色
            if (playerSync != null) playerSync.enabled = true;
        }
        else
        {
            // === 靈界世界 ===
            if (playerTop != null)
            {
                playerTop.enabled = false;
                if (rbTop != null) rbTop.linearVelocity = new Vector2(0f, rbTop.linearVelocity.y);
            }
            if (playerBottom != null) playerBottom.enabled = true;

            // 關閉同步：靈界角色獨立受玩家操作，不受上方拘束
            if (playerSync != null) playerSync.enabled = false;
        }

        if (playerAttack != null)
        {
            playerAttack.now_world(currentWorld);
        }

        if (cameraController != null)
        {
            cameraController.SetWorld(currentWorld);
        }
    }
}