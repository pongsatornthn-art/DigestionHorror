using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    private PlayerStatus status;

    [Header("References")]
    public Camera cam;
    private Rigidbody2D rb;
    public Transform bodyTransform;
    public Transform legsTransform;
    private Animator bodyAnim;
    private Animator legAnim;

    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float iFrameDuration = 0.5f;
    public float dashStaminaCost = 20f;
    public AudioClip dashSound;
    private bool isDashing = false;
    private bool canDash = true;
    private bool isInvulnerable = false;

    [Header("Weapon Durability System")]
    public Weapon[] allWeapons;
    public float durabilityLossPerHit = 10f;
    public GameObject durabilityUI;

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

    [Header("Footstep Sounds")]
    public AudioClip[] footstepSounds;
    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.3f;
    private float stepTimer = 0f;

    [Header("Combat Settings")]
    public Transform attackPoint;
    public Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    public LayerMask enemyLayers;
    private float nextAttackTime = 0f;

    [Header("Game Over & Sound")]
    public GameObject gameOverPanel;
    public AudioSource audioSource;
    public AudioClip swingSound;
    public AudioClip hitSound;
    public AudioClip brokenWeaponSound;

    [Header("Death Settings")]
    public GameObject playerDeathBoxPrefab;
    public Transform respawnPoint;

    [Header("Knockback Settings")]
    public float knockbackDuration = 0.2f;
    private bool isKnockbacked = false;

    [Header("Economy")]
    public int currentMoney = 500;
    public TextMeshProUGUI moneyText;

    // ⭐ [เพิ่มใหม่] ตัวแปรเช็คว่ากำลังคราฟต์ของอยู่ไหม
    private bool isCrafting = false;

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

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        status = GetComponent<PlayerStatus>();
    }

    void Update()
    {
        // ⭐ [เพิ่ม] ถ้ากำลังคราฟต์ของอยู่ ห้ามทำอะไรเลย
        if (isCrafting) return;

        if (isDashing) return;

        if ((InventoryUI.instance != null && InventoryUI.instance.inventoryPanel.activeSelf) || isKnockbacked)
        {
            movement = Vector2.zero;
            return;
        }

        if (status != null && status.isRooted)
        {
            movement = Vector2.zero;
            return;
        }

        HandleInput();

        if (Input.GetKeyDown(KeyCode.Space) && canDash)
        {
            if (currentStamina >= dashStaminaCost)
            {
                StartCoroutine(DashRoutine());
            }
        }

        UpdateUI();
        HandleCombatInput();
        UpdateAnimationParams();
        HandleFootsteps();
    }

    void FixedUpdate()
    {
        if (isDashing || isKnockbacked || isCrafting || (InventoryUI.instance != null && InventoryUI.instance.inventoryPanel.activeSelf)) return;

        rb.MovePosition(rb.position + movement.normalized * activeSpeed * Time.fixedDeltaTime);

        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;

        if (currentActiveHolder != null)
            currentActiveHolder.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (bodyTransform != null) bodyTransform.rotation = Quaternion.Euler(0, 0, angle);

        if (legsTransform != null)
        {
            if (movement.magnitude > 0.1f)
            {
                float angleLegs = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f;
                legsTransform.rotation = Quaternion.Lerp(legsTransform.rotation, Quaternion.Euler(0, 0, angleLegs), 0.2f);
            }
            else
            {
                legsTransform.rotation = Quaternion.Lerp(legsTransform.rotation, Quaternion.Euler(0, 0, angle), 0.1f);
            }
        }
    }

    // ⭐ [เพิ่มใหม่] ฟังก์ชันสำหรับให้ CraftingManager เรียกใช้ตอนเริ่ม/หยุดคราฟต์
    public void SetCraftingState(bool state)
    {
        isCrafting = state;
        if (state)
        {
            movement = Vector2.zero; // หยุดเดินทันที
            // ถ้ามีอนิเมชั่นก้มคราฟต์ ก็ให้ใส่ตรงนี้ได้เลย (เช่น bodyAnim.SetTrigger("Craft");)
        }
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;
        isInvulnerable = true;

        currentStamina -= dashStaminaCost;
        UpdateUI();

        if (audioSource != null && dashSound != null) audioSource.PlayOneShot(dashSound);

        if (bodyAnim != null) bodyAnim.SetTrigger("Dash");
        if (legAnim != null) legAnim.SetTrigger("Dash");

        Vector2 dashDir = movement.magnitude > 0.1f ? movement.normalized : (mousePos - rb.position).normalized;
        rb.linearVelocity = dashDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        rb.linearVelocity = Vector2.zero;

        float remainingIFrame = Mathf.Max(0, iFrameDuration - dashDuration);
        if (remainingIFrame > 0)
        {
            yield return new WaitForSeconds(remainingIFrame);
        }

        isInvulnerable = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void HandleFootsteps()
    {
        if (movement.magnitude <= 0.1f || isKnockbacked || isDashing)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstepSound();
            stepTimer = isRunning ? runStepInterval : walkStepInterval;
        }
    }

    void PlayFootstepSound()
    {
        if (audioSource != null && footstepSounds != null && footstepSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, footstepSounds.Length);
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(footstepSounds[randomIndex]);
        }
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (isKnockbacked || isDashing) return;

        StopAllCoroutines();
        StartCoroutine(KnockbackRoutine(force));
    }

    private System.Collections.IEnumerator KnockbackRoutine(Vector2 force)
    {
        isKnockbacked = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockbacked = false;
        rb.linearVelocity = Vector2.zero;
    }

    public void PlayerTakeDamage(int dmg)
    {
        if (isInvulnerable) return;

        currentHealth = Mathf.Max(0, currentHealth - dmg);
        UpdateUI();
        if (currentHealth <= 0) Die();
    }

    public void PlayerDie() { }

    void Die()
    {
        if (playerDeathBoxPrefab != null && Inventory.Instance != null)
        {
            List<InventoryItem> droppedItems = Inventory.Instance.DropAllItemsExcept("Knife");

            if (droppedItems.Count > 0)
            {
                GameObject boxObj = Instantiate(playerDeathBoxPrefab, transform.position, Quaternion.identity);
                LootBox deathBox = boxObj.GetComponent<LootBox>();
                if (deathBox != null) deathBox.SetBoxContents(droppedItems);
            }
        }

        if (currentActiveHolder != null)
            currentActiveHolder.GetComponent<Animator>().SetBool("IsDead", true);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        this.enabled = false;
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        UpdateUI();
        if (currentActiveHolder != null) currentActiveHolder.GetComponent<Animator>().SetBool("IsDead", false);
        if (respawnPoint != null) transform.position = respawnPoint.position;
        this.enabled = true;
        isKnockbacked = false;
        isDashing = false;
        isInvulnerable = false;
    }

    public void EquipWeapon(ItemData newItem)
    {
        currentWeapon = newItem;
        if (knifeHolder) knifeHolder.SetActive(false);
        if (axeHolder) axeHolder.SetActive(false);
        if (nailStickHolder) nailStickHolder.SetActive(false);
        currentActiveHolder = null;

        bool isWeapon = (newItem != null && newItem.itemType == ItemType.Weapon);
        if (durabilityUI != null) durabilityUI.SetActive(isWeapon);

        if (!isWeapon)
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
            Weapon activeWeapon = currentActiveHolder.GetComponent<Weapon>();
            if (activeWeapon != null) activeWeapon.UpdateUI();
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
        if (currentWeapon == null) return;

        if (currentWeapon.itemType == ItemType.Weapon)
        {
            if (Time.time >= nextAttackTime)
            {
                if (Input.GetMouseButtonDown(0)) PerformAttack(true);
                else if (Input.GetMouseButtonDown(1)) PerformAttack(false);
            }
        }
        else if (currentWeapon.itemType == ItemType.Consumable)
        {
            if (Input.GetMouseButtonDown(0)) ConsumeItem();
        }
        else if (currentWeapon.itemType == ItemType.Totem)
        {
            if (Input.GetMouseButtonDown(0)) UseTotem();
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
        if (currentWeapon == null) return;
        Weapon activeWeapon = GetActiveWeapon();

        if (activeWeapon != null && activeWeapon.IsBroken())
        {
            if (audioSource != null && brokenWeaponSound != null) audioSource.PlayOneShot(brokenWeaponSound);
            nextAttackTime = Time.time + 0.3f;
            return;
        }

        float cost = isLight ? currentWeapon.staminaCost : currentWeapon.heavyStaminaCost;
        if (currentStamina < cost) return;

        currentStamina -= cost;
        float cooldown = isLight ? currentWeapon.lightAttackCooldown : currentWeapon.heavyAttackCooldown;
        nextAttackTime = Time.time + cooldown;

        if (activeWeapon != null) activeWeapon.UseWeapon(durabilityLossPerHit);
        pendingDamage = isLight ? currentWeapon.damage : currentWeapon.heavyDamage;

        if (currentActiveHolder != null)
        {
            Animator weaponAnim = currentActiveHolder.GetComponent<Animator>();
            if (weaponAnim && weaponAnim.isActiveAndEnabled)
            {
                weaponAnim.SetBool("IsLight", isLight);
                weaponAnim.SetTrigger("Attack");
                if (audioSource) audioSource.pitch = 1f;
                if (audioSource && swingSound) audioSource.PlayOneShot(swingSound);
                Invoke("DealDamage", 0.2f);
            }
        }
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

    public void UpdateUI()
    {
        if (hpSlider != null) hpSlider.value = (maxHealth > 0) ? (float)currentHealth / maxHealth : 0;
        if (staminaSlider != null) staminaSlider.value = (maxStamina > 0) ? currentStamina / maxStamina : 0;
        if (moneyText != null) moneyText.text = currentMoney.ToString();
    }

    Weapon GetActiveWeapon()
    {
        foreach (Weapon weapon in allWeapons)
            if (weapon != null && weapon.gameObject.activeInHierarchy) return weapon;
        return null;
    }

    void ConsumeItem()
    {
        if (currentWeapon == null || currentWeapon.itemType != ItemType.Consumable) return;
        if (DigestionSystem.instance != null) DigestionSystem.instance.DecreaseDigestion(currentWeapon.digestionReduceAmount);
        if (Inventory.instance != null) Inventory.instance.RemoveItem(currentWeapon);
    }

    void UseTotem()
    {
        if (currentWeapon == null || currentWeapon.itemType != ItemType.Totem) return;
        if (DigestionSystem.instance != null) DigestionSystem.instance.ApplyTotemBuff(currentWeapon.digestionSlowMultiplier, currentWeapon.totemEffectDuration);
        if (Inventory.instance != null) Inventory.instance.RemoveItem(currentWeapon);
    }
}