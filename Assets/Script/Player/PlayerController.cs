using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [Header("References")]
    public Camera cam;
    private Rigidbody2D rb;
    public Transform bodyTransform;
    public Transform legsTransform;
    private Animator bodyAnim;
    private Animator legAnim;

    [Header("Separate Weapon System")]
    public GameObject knifeHolder;
    public GameObject axeHolder;
    public GameObject nailStickHolder;
    private GameObject currentActiveHolder;

    [Header("Current Weapon Data")]
    public ItemData currentWeapon;
    private int pendingDamage;

    [Header("Player Stats & UI")]
    public int maxHealth = 100;
    public int currentHealth;
    public Slider hpSlider;
    public float maxStamina = 100f;
    public float currentStamina;
    public Slider staminaSlider;
    public float staminaRegen = 7f;
    public float runStaminaCost = 15f;
    private bool isRunning;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    private float activeSpeed;
    private Vector2 movement;
    private Vector2 mousePos;

    [Header("Combat Settings")]
    public Transform attackPoint;
    public Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    public LayerMask enemyLayers;
    public float attackRate = 2f;
    private float nextAttackTime = 0f;

    [Header("Game Over & Sound")]
    public GameObject gameOverPanel;
    public AudioSource audioSource;
    public AudioClip swingSound;
    public AudioClip hitSound;

    void Awake() => instance = this;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (bodyTransform != null) bodyAnim = bodyTransform.GetComponent<Animator>();
        if (legsTransform != null) legAnim = legsTransform.GetComponent<Animator>();

        currentHealth = maxHealth;
        currentStamina = maxStamina;

        UpdateUI();
        Time.timeScale = 1f;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        EquipWeapon(currentWeapon);

        // เริ่มเกมมาให้เมาส์เลื่อนได้ปกติแต่ซ่อนไว้ (สำหรับเกม Top-down)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        // ✅ 1. ถ้าเปิด UI อยู่ ให้หยุดรับคำสั่งควบคุมทั้งหมดเพื่อให้เมาส์เป็นอิสระ
        if (InventoryUI.instance != null && InventoryUI.instance.inventoryPanel.activeSelf)
        {
            movement = Vector2.zero;
            return;
        }

        HandleInput();
        UpdateUI();
        HandleCombatInput();
        UpdateAnimationParams();
    }

    void FixedUpdate()
    {
        // ✅ 2. รวมฟังก์ชัน FixedUpdate ไว้ที่เดียว และเช็ค UI
        if (InventoryUI.instance != null && InventoryUI.instance.inventoryPanel.activeSelf) return;

        // โค้ดขยับตัว
        rb.MovePosition(rb.position + movement.normalized * activeSpeed * Time.fixedDeltaTime);

        // โค้ดหันหน้าตามเมาส์
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;

        if (currentActiveHolder != null)
            currentActiveHolder.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (bodyTransform != null) bodyTransform.rotation = Quaternion.Euler(0, 0, angle);

        if (movement.magnitude > 0.1f && legsTransform != null)
        {
            float angleLegs = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f;
            legsTransform.rotation = Quaternion.Lerp(legsTransform.rotation, Quaternion.Euler(0, 0, angleLegs), 0.2f);
        }
    }

    // --- ส่วนฟังก์ชันอื่นๆ (EquipWeapon, PerformAttack, etc.) ของคุณพงศธรคงเดิม ---
    public void EquipWeapon(ItemData newItem)
    {
        currentWeapon = newItem;
        if (knifeHolder) knifeHolder.SetActive(false);
        if (axeHolder) axeHolder.SetActive(false);
        if (nailStickHolder) nailStickHolder.SetActive(false);
        currentActiveHolder = null;

        if (newItem == null || newItem.itemType != ItemType.Weapon)
        {
            if (bodyTransform != null) bodyTransform.gameObject.SetActive(true);
            return;
        }

        if (newItem.itemName == "Knife") currentActiveHolder = knifeHolder;
        else if (newItem.itemName == "Axe") currentActiveHolder = axeHolder;
        else if (newItem.itemName == "NailStick") currentActiveHolder = nailStickHolder;

        if (currentActiveHolder != null)
        {
            if (bodyTransform != null) bodyTransform.gameObject.SetActive(false);
            currentActiveHolder.SetActive(true);
        }
    }

    void HandleInput()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && movement.magnitude > 0)
        {
            activeSpeed = runSpeed;
            isRunning = true;
            currentStamina -= runStaminaCost * Time.deltaTime;
        }
        else
        {
            activeSpeed = walkSpeed;
            isRunning = false;
            if (currentStamina < maxStamina) currentStamina += staminaRegen * Time.deltaTime;
        }
    }

    void HandleCombatInput()
    {
        if (Time.time >= nextAttackTime && currentWeapon != null && currentWeapon.itemType == ItemType.Weapon)
        {
            if (Input.GetMouseButtonDown(0)) PerformAttack(true);
            else if (Input.GetMouseButtonDown(1)) PerformAttack(false);
        }
    }

    void UpdateAnimationParams()
    {
        float currentSpeed = movement.magnitude * activeSpeed;
        if (bodyAnim) { bodyAnim.SetFloat("Speed", currentSpeed); bodyAnim.SetBool("IsRunning", isRunning); }
        if (legAnim) { legAnim.SetFloat("Speed", currentSpeed); legAnim.SetBool("IsRunning", isRunning); }
    }

    void PerformAttack(bool isLight)
    {
        float cost = isLight ? currentWeapon.staminaCost : currentWeapon.heavyStaminaCost;
        if (currentStamina < cost) return;
        currentStamina -= cost;
        float delay = isLight ? (1f / attackRate) : (1.5f / attackRate);
        nextAttackTime = Time.time + delay;
        pendingDamage = isLight ? currentWeapon.damage : currentWeapon.heavyDamage;
        if (currentActiveHolder != null)
        {
            Animator weaponAnim = currentActiveHolder.GetComponent<Animator>();
            if (weaponAnim && weaponAnim.isActiveAndEnabled)
            {
                weaponAnim.SetBool("IsLight", isLight);
                weaponAnim.SetTrigger("Attack");
            }
        }
        if (audioSource && swingSound) audioSource.PlayOneShot(swingSound);
        DealDamage();
    }

    public void DealDamage()
    {
        if (attackPoint == null || currentActiveHolder == null || currentWeapon == null) return;
        float zAngle = currentActiveHolder.transform.eulerAngles.z;
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackBoxSize, zAngle, enemyLayers);
        float currentKnockback = (pendingDamage == currentWeapon.damage) ? currentWeapon.knockback : currentWeapon.heavyKnockback;
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
            if (enemyStats != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemyStats.TakeDamage(pendingDamage, currentKnockback, knockbackDir);
            }
        }
    }

    void UpdateUI()
    {
        if (hpSlider != null) hpSlider.value = (maxHealth > 0) ? (float)currentHealth / maxHealth : 0;
        if (staminaSlider != null) staminaSlider.value = (maxStamina > 0) ? currentStamina / maxStamina : 0;
    }

    public void PlayerTakeDamage(int dmg)
    {
        currentHealth = Mathf.Max(0, currentHealth - dmg);
        UpdateUI();
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (currentActiveHolder != null) currentActiveHolder.GetComponent<Animator>().SetBool("IsDead", true);
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        this.enabled = false;
    }

    public void RestartGame() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }

    void OnDrawGizmos()
    {
        if (attackPoint != null) { Gizmos.color = Color.yellow; Gizmos.DrawWireCube(attackPoint.position, attackBoxSize); }
    }
}