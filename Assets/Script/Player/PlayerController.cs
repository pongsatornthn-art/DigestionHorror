using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [Header("References")]
    public Camera cam;
    private Rigidbody2D rb;

    private Animator bodyAnim;
    private Animator legAnim;

    [Header("Body Parts")]
    public Transform bodyTransform;
    public Transform legsTransform;
    public Transform weaponBody;

    [Header("Weapon Visuals")]
    public SpriteRenderer weaponRenderer;

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

        if (bodyTransform != null) bodyAnim = bodyTransform.GetComponent<Animator>();
        if (legsTransform != null) legAnim = legsTransform.GetComponent<Animator>();

        if (cam == null) cam = Camera.main;

        currentHealth = maxHealth;
        currentStamina = maxStamina;

        if (hpSlider) hpSlider.maxValue = maxHealth;
        if (staminaSlider) staminaSlider.maxValue = maxStamina;

        if (currentWeapon != null) EquipWeapon(currentWeapon);
    }

    void Update()
    {
        // 1. Movement Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // 2. Stamina Logic
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

        UpdateUI();

        // 3. Combat Input (แก้ตรงนี้ให้แล้วครับ)
        if (Time.time >= nextAttackTime && currentWeapon != null && currentWeapon.itemType == ItemType.Weapon)
        {
            // คลิกซ้าย (0) -> ตีเบา (Light)
            if (Input.GetMouseButtonDown(0))
            {
                PerformAttack(true);
            }
            // คลิกขวา (1) -> ตีแรง (Heavy)
            else if (Input.GetMouseButtonDown(1))
            {
                PerformAttack(false);
            }
        }

        // 4. Animation Speed
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

        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;

        if (bodyTransform != null)
        {
            bodyTransform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (weaponBody != null)
        {
            weaponBody.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (movement.magnitude > 0.1f && legsTransform != null)
        {
            float angleLegs = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f;
            legsTransform.rotation = Quaternion.Lerp(legsTransform.rotation, Quaternion.Euler(0, 0, angleLegs), 0.2f);
        }
    }

    void UpdateUI()
    {
        if (hpSlider) hpSlider.value = currentHealth;
        if (staminaSlider) staminaSlider.value = currentStamina;
    }

    void PerformAttack(bool isLight)
    {
        float cost = isLight ? currentWeapon.staminaCost : currentWeapon.heavyStaminaCost;
        if (currentStamina < cost) return;

        currentStamina -= cost;
        float delay = isLight ? (1f / attackRate) : (1.5f / attackRate);
        nextAttackTime = Time.time + delay;

        pendingDamage = isLight ? currentWeapon.damage : currentWeapon.heavyDamage;
        pendingKnockback = isLight ? currentWeapon.knockback : currentWeapon.heavyKnockback;

        // สั่งเล่น Animation ร่างปกติ
        if (bodyAnim != null && bodyAnim.gameObject.activeSelf)
        {
            bodyAnim.SetBool("IsLight", isLight); // ⭐ บอกว่าตีเบาหรือแรง
            bodyAnim.SetTrigger("Attack");
        }

        // สั่งเล่น Animation ร่างมีด
        if (weaponBody != null && weaponBody.gameObject.activeSelf)
        {
            Animator weaponAnim = weaponBody.GetComponent<Animator>();
            if (weaponAnim)
            {
                weaponAnim.SetBool("IsLight", isLight); // ⭐ บอกว่าตีเบาหรือแรง
                weaponAnim.SetTrigger("Attack");
            }
        }

        if (audioSource && swingSound) audioSource.PlayOneShot(swingSound);

        DealDamage();
    }

    public void DealDamage()
    {
        if (attackPoint == null) return;

        float zAngle = (weaponBody != null && weaponBody.gameObject.activeSelf) ? weaponBody.eulerAngles.z : bodyTransform.eulerAngles.z;

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackBoxSize, zAngle, enemyLayers);

        bool hitSomething = false;
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(pendingDamage);
                hitSomething = true;
            }

            Debug.Log("ตีโดน: " + enemy.name);
        }
        if (hitSomething && audioSource && hitSound) audioSource.PlayOneShot(hitSound);
    }

    public void EquipWeapon(ItemData newItem)
    {
        currentWeapon = newItem;
        if (newItem != null && newItem.equippedSprite != null && weaponRenderer != null)
        {
            weaponRenderer.sprite = newItem.equippedSprite;
            weaponRenderer.enabled = true;
        }
        else if (weaponRenderer != null)
        {
            weaponRenderer.enabled = false;
        }
    }

    public void PlayerTakeDamage(int dmg)
    {
        currentHealth -= dmg;

        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Debug.Log("Player ตายแล้ว!");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPoint.position, attackBoxSize);
    }
}