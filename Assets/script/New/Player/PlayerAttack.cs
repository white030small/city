using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public enum WeaponType { Knife, Gun }

    [Header("相機參照 (重要：分割畫面請指定 Camera_Top)")]
    [SerializeField] private Camera cam;

    [Header("近戰 (刀) 設定")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;

    [Header("蹲下發射點位移 (Y 軸)")]
    [SerializeField] private float crouchFireOffsetY = -0.5f;

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

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (character == null) character = GetComponent<PlayerController>();

        // 若 Inspector 未指定，才自動尋找 MainCamera
        if (cam == null) cam = Camera.main;
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
        // 模式 2 即時更新瞄準器位置與射線
        if (currentWeapon == WeaponType.Gun && gunAimMode == 2)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3 currentFirePos = GetCurrentAttackPoint();
            currentFirePos.z = 0f;
            

            Vector2 aimDirection = (mouseWorldPos - currentFirePos).normalized;
            float maxAimDistance = Vector2.Distance(currentFirePos, mouseWorldPos);

            // 【加入射線檢測】：如果瞄準線碰到了地面或牆壁，就停在撞擊點，不穿牆！
            Vector3 finalTargetPos = mouseWorldPos;
            RaycastHit2D hit = Physics2D.Raycast(currentFirePos, aimDirection, maxAimDistance, LayerMask.GetMask("Ground"));
            
            if (hit.collider != null)
            {
                finalTargetPos = hit.point;
            }

            // 1. 更新 Crosshair 物件位置
            if (crosshair != null)
            {
                crosshair.transform.position = finalTargetPos;
            }

            // 2. 更新瞄準線
            if (aimLine != null)
            {
                aimLine.useWorldSpace = true;
                aimLine.positionCount = 2;
                aimLine.SetPosition(0, currentFirePos);
                aimLine.SetPosition(1, finalTargetPos);
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

        Collider2D[] hits = Physics2D.OverlapCircleAll(currentFirePos, attackRange, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(attackDamage);
            }
        }

        cooldownTimer = attackCooldown;
    }

    private void ExecuteShoot()
    {
        if (bulletPrefab == null) return;

        if (animator != null)
            animator.Play(isCrouching ? "mainchar_down_shoot" : "mainchar_shoot");

        Vector3 firePos = GetCurrentAttackPoint();
        GameObject bullet = Instantiate(bulletPrefab, firePos, Quaternion.identity);

        if (gunAimMode == 1)
        {
            if (bullet.TryGetComponent<Gun>(out var g))
                g.leftorright(facingDirX < 0);
        }
        else
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector2 dir = (mouseWorldPos - firePos).normalized;
            if (bullet.TryGetComponent<Gun>(out var g))
                g.SetDirection(dir);
        }

        currentAmmo--;
        if (bulletUI != null) bulletUI.ShowAttackUI(currentAmmo);
        cooldownTimer = gunAttackCooldown;
    }

    public bool AddAmmo(int amount)
    {
        if (currentAmmo >= maxAmmo) return false;

        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);

        if (bulletUI != null)
        {
            bulletUI.ShowChangeUI(currentAmmo);
        }

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

    /// <summary>
    /// 精確換算滑鼠世界座標 (修復 Z 軸深度遺失與分割相機誤差)
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return Vector3.zero;

        Vector3 mouseViewportPos = cam.ScreenToViewportPoint(Input.mousePosition);

        mouseViewportPos.y = Mathf.Max(0.05f, mouseViewportPos.y);
        mouseViewportPos.z = Mathf.Abs(cam.transform.position.z);

        Vector3 worldPos = cam.ViewportToWorldPoint(mouseViewportPos);
        worldPos.z = 0f;
        return worldPos;
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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position + new Vector3(0f, crouchFireOffsetY, 0f), attackRange);
    }
}