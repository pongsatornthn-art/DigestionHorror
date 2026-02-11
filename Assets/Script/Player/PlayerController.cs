using UnityEngine;
using UnityEngine.UI;

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
    // ลาก GameObject หุ่นแต่ละตัวจาก Hierarchy มาใส่ที่นี่ (ต้องลากมาใส่ให้ครบนะ!)
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

    [Header("Sound")]
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
        if (hpSlider) hpSlider.maxValue = maxHealth;
        if (staminaSlider) staminaSlider.maxValue = maxStamina;

        // เริ่มเกมมาเช็คอาวุธทันที
        EquipWeapon(currentWeapon);
    }

    void Update()
    {
        HandleInput();
        UpdateUI();
        HandleCombatInput();
        UpdateAnimationParams();
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

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * activeSpeed * Time.fixedDeltaTime);

        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;

        // หุ่นที่ถืออยู่ต้องหันตามเมาส์
        if (currentActiveHolder != null)
        {
            currentActiveHolder.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (bodyTransform != null) bodyTransform.rotation = Quaternion.Euler(0, 0, angle);

        if (movement.magnitude > 0.1f && legsTransform != null)
        {
            float angleLegs = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f;
            legsTransform.rotation = Quaternion.Lerp(legsTransform.rotation, Quaternion.Euler(0, 0, angleLegs), 0.2f);
        }
    }

    // ⭐ ฟังก์ชันสลับหุ่น: แก้ไขให้เช็คชื่อได้แม่นยำขึ้น
    public void EquipWeapon(ItemData newItem)
    {
        currentWeapon = newItem;

        // 1. ปิดตาทุกหุ่นอาวุธก่อน (ล้างกระดาน)
        if (knifeHolder) knifeHolder.SetActive(false);
        if (axeHolder) axeHolder.SetActive(false);
        if (nailStickHolder) nailStickHolder.SetActive(false);
        currentActiveHolder = null;

        if (newItem == null || newItem.itemType != ItemType.Weapon)
        {
            // กรณี "มือเปล่า": เปิดตาตัวละครหลัก และปิดหุ่นอาวุธ
            if (bodyTransform != null) bodyTransform.gameObject.SetActive(true);
            Debug.Log("โหมดมือเปล่า: เปิดตาตัวละครหลัก");
            return;
        }

        // 2. เช็คชื่ออาวุธเพื่อเลือกหุ่นที่จะเปิด
        if (newItem.itemName == "Knife") currentActiveHolder = knifeHolder;
        else if (newItem.itemName == "Axe") currentActiveHolder = axeHolder;
        else if (newItem.itemName == "NailStick") currentActiveHolder = nailStickHolder;

        // 3. จัดการสลับการมองเห็น (Toggle Visibility)
        if (currentActiveHolder != null)
        {
            // ❌ ปิดตาตัวละครหลัก (bodyTransform) เพื่อไม่ให้ซ้อน
            if (bodyTransform != null) bodyTransform.gameObject.SetActive(false);

            // ✅ เปิดตาหุ่นอาวุธตัวที่เลือกแทน
            currentActiveHolder.SetActive(true);

            Debug.Log("สวมใส่ " + newItem.itemName + ": ปิดตาตัวละครหลัก และใช้หุ่นอาวุธแทน");
        }
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
        if (attackPoint == null || currentActiveHolder == null) return;

        float zAngle = currentActiveHolder.transform.eulerAngles.z;
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackBoxSize, zAngle, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
            if (enemyStats != null) enemyStats.TakeDamage(pendingDamage);
        }
    }

    void UpdateUI()
    {
        if (hpSlider) hpSlider.value = currentHealth;
        if (staminaSlider) staminaSlider.value = currentStamina;
    }

    public void PlayerTakeDamage(int dmg)
    {
        currentHealth = Mathf.Max(0, currentHealth - dmg);
        if (hpSlider) hpSlider.value = currentHealth;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (currentActiveHolder != null) currentActiveHolder.GetComponent<Animator>().SetBool("IsDead", true);
        this.enabled = false;
    }

    void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(attackPoint.position, attackBoxSize);
        }
    }
}