using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public enum WeaponType { Knife, Gun }

    [Header("近戰 (刀) 設定")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;

    [Header("蹲下發射點位移 (Y 軸)")]
    [SerializeField] private float crouchFireOffsetY = -0.5f; // 蹲下時發射點下移量 (請依角色動畫調整)

    [Header("遠程 (槍) 設定")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float gunAttackCooldown = 1f;
    [SerializeField] private int maxAmmo = 5;
    [SerializeField] private float reloadTimePerBullet = 1.5f;
    public int currentAmmo = 5;

    [Header("左上角武器圖示")]
    [SerializeField] private GameObject knifeUI;
    [SerializeField] private GameObject gunUI;

    [Header("右上方子彈轉盤 UI")]
    [SerializeField] private BulletUI bulletUI;

    [Header("模式 2 瞄準器")]
    [SerializeField] private GameObject crosshair;
    [SerializeField] private LineRenderer aimLine;

    [Header("角色控制器參照")]
    [SerializeField] private PlayerController character;

    [Header("狀態顯示 (唯讀檢視)")]
    public WeaponType currentWeapon = WeaponType.Knife;
    public int gunAimMode = 1; // 1 = 水平直射(可移動), 2 = 滑鼠瞄準(定格)
    public int world = 0;      // 0 = 現實, 1 = 靈界

    private float cooldownTimer = 0f;
    private float reloadTimer = 0f;
    private bool isReloading = false;
    private bool isCrouching = false;
    private float facingDirX = 1f;

    private Animator animator;
    private Camera cam;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (character == null) character = GetComponent<PlayerController>();
        cam = Camera.main;
    }

    void Start()
    {
        currentAmmo = maxAmmo;
        SetWeapon(WeaponType.Knife);
    }

    void Update()
    {
        if (world == 1) return;

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        // 優先讀取角色腳本的蹲下狀態，保持與物理碰撞體同步
        if (character != null)
        {
            isCrouching = character.isCrouching;
        }
        else
        {
            isCrouching = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        }

        // 面向判定
        bool isAimMode2 = (currentWeapon == WeaponType.Gun && gunAimMode == 2);
        if (!isAimMode2)
        {
            float h = Input.GetAxisRaw("Horizontal");
            if (h != 0) facingDirX = Mathf.Sign(h);
        }
        else
        {
            Vector3 mousePos = GetMouseWorldPosition();
            if (mousePos.x > transform.position.x)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                facingDirX = 1f;
            }
            else if (mousePos.x < transform.position.x)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                facingDirX = -1f;
            }
        }

        HandleWeaponSwitch();

        if (isReloading)
        {
            HandleReload();
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && currentWeapon == WeaponType.Gun && currentAmmo < maxAmmo)
        {
            StartReload();
            return;
        }

        HandleAttackLogic();
    }

    /// <summary>
    /// 動態取得當前發射/攻擊原點：站立使用原點，蹲下時依 crouchFireOffsetY 向下偏移
    /// </summary>
    private Vector3 GetCurrentAttackPoint()
    {
        if (attackPoint == null) return transform.position;

        Vector3 pos = attackPoint.position;
        if (isCrouching)
        {
            pos.y += crouchFireOffsetY;
        }
        return pos;
    }

    private void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetWeapon(WeaponType.Knife);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (currentWeapon != WeaponType.Gun)
            {
                gunAimMode = 1;
                SetWeapon(WeaponType.Gun);
            }
            else
            {
                gunAimMode = (gunAimMode == 1) ? 2 : 1;
                ApplyGunAimModeSettings();
            }
        }
    }

    private void SetWeapon(WeaponType newWeapon)
    {
        InterruptReload();
        currentWeapon = newWeapon;

        if (currentWeapon == WeaponType.Knife)
        {
            if (knifeUI != null) knifeUI.SetActive(true);
            if (gunUI != null) gunUI.SetActive(false);
            if (bulletUI != null) bulletUI.gameObject.SetActive(false);

            DisableAimVisuals();

            if (character != null)
            {
                character.change(true);
                character.walk(true);
            }
        }
        else
        {
            if (knifeUI != null) knifeUI.SetActive(false);
            if (gunUI != null) gunUI.SetActive(true);

            if (bulletUI != null)
            {
                bulletUI.gameObject.SetActive(true);
                bulletUI.ShowUI(currentAmmo);
            }

            ApplyGunAimModeSettings();
        }
    }

    private void ApplyGunAimModeSettings()
    {
        if (gunAimMode == 1)
        {
            DisableAimVisuals();
            if (character != null)
            {
                character.change(false);
                character.walk(true);
            }
        }
        else
        {
            if (character != null)
            {
                character.change(false);
                character.walk(false);
            }
            if (crosshair != null) crosshair.SetActive(true);
            if (aimLine != null) aimLine.enabled = true;
        }
    }

    private void DisableAimVisuals()
    {
        if (crosshair != null) crosshair.SetActive(false);
        if (aimLine != null) aimLine.enabled = false;
    }

    private void HandleAttackLogic()
    {
        // 模式 2 即時更新瞄準射線起點與終點 (蹲下時起點會自動跟著降下)
        if (currentWeapon == WeaponType.Gun && gunAimMode == 2)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3 currentFirePos = GetCurrentAttackPoint();

            if (aimLine != null)
            {
                aimLine.SetPosition(0, currentFirePos);
                aimLine.SetPosition(1, mouseWorldPos);
            }
        }

        if (Input.GetMouseButtonDown(0) && cooldownTimer <= 0f)
        {
            if (currentWeapon == WeaponType.Knife)
            {
                ExecuteKnife();
            }
            else if (currentWeapon == WeaponType.Gun && currentAmmo > 0)
            {
                ExecuteShoot();
            }
        }
    }

    private void ExecuteKnife()
    {
        if (animator != null)
            animator.Play(isCrouching ? "mainchar_down_attack" : "main_attack");

        Vector3 currentFirePos = GetCurrentAttackPoint();

        // 偵測周圍所有碰撞體
        Collider2D[] hits = Physics2D.OverlapCircleAll(currentFirePos, attackRange, enemyLayer);
        foreach (var hit in hits)
        {
            // 只要身上有 EnemyController 腳本，不管 Tag 是什麼直接扣血！
            if (hit.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(attackDamage);
                Debug.Log("主角砍中敵人！");
            }
        }

        cooldownTimer = attackCooldown;
    }
    private void ExecuteShoot()
    {
        if (bulletPrefab == null) return;

        if (animator != null)
            animator.Play(isCrouching ? "mainchar_down_shoot" : "mainchar_shoot");

        // 使用動態計算後的發射點 (蹲下時已往下偏移)
        Vector3 firePos = GetCurrentAttackPoint();
        GameObject bullet = Instantiate(bulletPrefab, firePos, Quaternion.identity);

        if (gunAimMode == 1)
        {
            if (bullet.TryGetComponent<Gun>(out var g))
                g.leftorright(facingDirX < 0);
        }
        else
        {
            Vector2 dir = (GetMouseWorldPosition() - firePos).normalized;
            if (bullet.TryGetComponent<Gun>(out var g))
                g.SetDirection(dir);
        }

        currentAmmo--;
        if (bulletUI != null) bulletUI.ShowAttackUI(currentAmmo);
        cooldownTimer = gunAttackCooldown;
    }
    public bool AddAmmo(int amount)
    {
        // 彈藥已達上限則不撿取
        if (currentAmmo >= maxAmmo) return false;

        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);

        // 如果目前武器是槍，且 UI 存在，即時更新轉盤 UI
        if (bulletUI != null)
        {
            bulletUI.ShowChangeUI(currentAmmo);
        }

        Debug.Log($"[PlayerAttack] 成功裝填彈藥，目前彈藥: {currentAmmo}/{maxAmmo}");
        return true;
    }

    private void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadTimePerBullet;
        if (character != null) character.walk(false);
    }

    private void HandleReload()
    {
        reloadTimer -= Time.deltaTime;
        if (reloadTimer <= 0f)
        {
            currentAmmo++;
            if (bulletUI != null) bulletUI.ShowChangeUI(currentAmmo);

            if (currentAmmo >= maxAmmo)
            {
                isReloading = false;
                if (character != null) character.walk(gunAimMode == 1);
            }
            else
            {
                reloadTimer = reloadTimePerBullet;
            }
        }
    }

    private void InterruptReload()
    {
        if (!isReloading) return;
        isReloading = false;
        if (character != null) character.walk(currentWeapon == WeaponType.Knife || gunAimMode == 1);
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (cam == null) cam = Camera.main;
        Vector3 p = cam != null ? cam.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
        p.z = 0f;
        return p;
    }

    public void now_world(int w)
    {
        world = w;
        if (world == 1)
        {
            InterruptReload();
            DisableAimVisuals();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        // 綠色球體：站立發射/判定範圍
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        // 藍色球體：蹲下預覽發射/判定範圍
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position + new Vector3(0f, crouchFireOffsetY, 0f), attackRange);
    }
}