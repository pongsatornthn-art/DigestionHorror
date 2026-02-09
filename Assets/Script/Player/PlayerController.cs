using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [Header("References")]
    public Camera cam;
    private Rigidbody2D rb;

    // ⭐ แยก Animator เป็น 2 ส่วน
    private Animator bodyAnim;
    private Animator legAnim;

    [Header("Body Parts")]
    public Transform bodyTransform; // ต้องลาก Body_Pivot ใส่ช่องนี้
    public Transform legsTransform; // ต้องลาก Legs_Pivot ใส่ช่องนี้

    [Header("Current Weapon")]
    public ItemData currentWeapon;

    private int pendingDamage;
    private float pendingKnockback;

    [Header("Player Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public Slider hpSlider;

    [Header("Stamina")]
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

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // ⭐ ระบบค้นหา Animator อัตโนมัติ (หัวใจสำคัญ!)
        if (bodyTransform != null) bodyAnim = bodyTransform.GetComponent<Animator>();
        if (legsTransform != null) legAnim = legsTransform.GetComponent<Animator>();

        if (cam == null) cam = Camera.main;

        currentHealth = maxHealth;
        currentStamina = maxStamina;

        if (hpSlider) hpSlider.maxValue = maxHealth;
        if (staminaSlider) staminaSlider.maxValue = maxStamina;
    }

    void Update()
    {
        // 1. รับค่าปุ่มเดิน
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // 2. Stamina
        ManageStamina();
        UpdateUI();

        // 3. โจมตี
        if (Time.time >= nextAttackTime && currentWeapon != null && currentWeapon.itemType == ItemType.Weapon)
        {
            if (Input.GetMouseButtonDown(0)) PerformAttack(true);
            else if (Input.GetMouseButtonDown(1)) PerformAttack(false);
        }

        // 4. ⭐ ส่งค่า Speed ให้ทั้ง "ตัว" และ "ขา" พร้อมกัน
        float currentSpeed = movement.magnitude * activeSpeed;

        if (bodyAnim != null)
        {
            bodyAnim.SetFloat("Speed", currentSpeed);
            bodyAnim.SetBool("IsRunning", isRunning);
        }

        if (legAnim != null)
        {
            legAnim.SetFloat("Speed", currentSpeed);
            legAnim.SetBool("IsRunning", isRunning);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * activeSpeed * Time.fixedDeltaTime);

        // หมุนตัว (Body) ตามเมาส์
        if (bodyTransform != null)
        {
            Vector2 lookDir = mousePos - rb.position;
            float angleBody = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
            bodyTransform.rotation = Quaternion.Euler(0, 0, angleBody);
        }

        // หมุนขา (Legs) ตามทิศเดิน
        if (movement.magnitude > 0.1f && legsTransform != null)
        {
            float angleLegs = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f;
            legsTransform.rotation = Quaternion.Lerp(legsTransform.rotation, Quaternion.Euler(0, 0, angleLegs), 0.2f);
        }
    }

    public void EquipWeapon(ItemData newItem)
    {
        currentWeapon = newItem;
        // ส่งค่า ID อาวุธไปที่ตัว (Body) เพื่อเปลี่ยนท่าถือ
        if (bodyAnim != null)
        {
            if (currentWeapon != null && currentWeapon.itemType == ItemType.Weapon)
                bodyAnim.SetInteger("WeaponID", currentWeapon.weaponID);
            else
                bodyAnim.SetInteger("WeaponID", -1);
        }
    }

    void PerformAttack(bool isLight)
    {
        float cost = isLight ? currentWeapon.staminaCost : currentWeapon.heavyStaminaCost;
        if (currentStamina < cost) return;

        currentStamina -= cost;
        nextAttackTime = Time.time + 1f / attackRate;

        pendingDamage = isLight ? currentWeapon.damage : currentWeapon.heavyDamage;
        pendingKnockback = isLight ? currentWeapon.knockback : currentWeapon.heavyKnockback;

        // สั่งให้ตัว (Body) เล่นท่าโจมตี
        if (bodyAnim != null)
        {
            bodyAnim.SetBool("IsHeavy", !isLight);
            bodyAnim.SetTrigger("Attack");
        }

        if (audioSource && swingSound) audioSource.PlayOneShot(swingSound);
    }

    public void DealDamage()
    {
        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackBoxSize, bodyTransform.eulerAngles.z, enemyLayers);

        bool hitSomething = false;
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(pendingDamage);
                hitSomething = true;
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDir * pendingKnockback, ForceMode2D.Impulse);
                }
            }
        }
        if (hitSomething && audioSource && hitSound) audioSource.PlayOneShot(hitSound);
    }

    public void PlayerTakeDamage(int dmg)
    {
        currentHealth -= dmg;
        Debug.Log("โดนตี! เลือดเหลือ: " + currentHealth);
        if (currentHealth <= 0) Debug.Log("Game Over!");
    }

    void ManageStamina()
    {
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

    void UpdateUI() { if (hpSlider) hpSlider.value = currentHealth; if (staminaSlider) staminaSlider.value = currentStamina; }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        if (bodyTransform != null)
        {
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(attackPoint.position, bodyTransform.rotation, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
        }
        Gizmos.DrawWireCube(Vector3.zero, attackBoxSize);
    }
}