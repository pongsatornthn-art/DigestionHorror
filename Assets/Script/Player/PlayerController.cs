using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

    [Header("Combat Settings")]
    public Transform attackPoint;
    public Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    public LayerMask enemyLayers;
    // public float attackRate = 2f; (ไม่ได้ใช้แล้ว เพราะดึงจาก ItemData)
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
    }

    void Update()
    {
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
        if (InventoryUI.instance != null && InventoryUI.instance.inventoryPanel.activeSelf) return;

        rb.MovePosition(rb.position + movement.normalized * activeSpeed * Time.fixedDeltaTime);

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
    public void PlayerDie()
    {
        Debug.Log("Player ตายแล้ว!");

        if (playerDeathBoxPrefab != null && Inventory.Instance != null)
        {
            // 1. สั่งดึงของทุกอย่างออกมา (ยกเว้นมีด ให้เช็คชื่อ "Knife" ให้ตรงกับใน ItemData)
            List<InventoryItem> droppedItems = Inventory.Instance.DropAllItemsExcept("Knife");

            // 2. ถ้ามีของตก (ของที่ไม่ใช่มีด) ให้เสกกล่อง
            if (droppedItems.Count > 0)
            {
                GameObject boxObj = Instantiate(playerDeathBoxPrefab, transform.position, Quaternion.identity);
                LootBox deathBox = boxObj.GetComponent<LootBox>();

                if (deathBox != null)
                {
                    // 3. ยัดของใส่กล่องที่เพิ่งเสกมา
                    deathBox.SetBoxContents(droppedItems);
                }
            }
        }

        // โค้ดส่วนอื่นๆ ตอนตาย เช่น แสดงหน้าจอ Game Over, เกิดใหม่
        // gameObject.SetActive(false); 
    }


    public void EquipWeapon(ItemData newItem)
    {
        currentWeapon = newItem;


        if (knifeHolder) knifeHolder.SetActive(false);
        if (axeHolder) axeHolder.SetActive(false);
        if (nailStickHolder) nailStickHolder.SetActive(false);
        currentActiveHolder = null;

        bool isWeapon = (newItem != null && newItem.itemType == ItemType.Weapon);

        if (durabilityUI != null)
        {
            durabilityUI.SetActive(isWeapon);
        }

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
            if (activeWeapon != null)
            {
                activeWeapon.UpdateUI();
            }
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
        // เช็คคูลดาวน์ก่อนเลย
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
        if (currentWeapon == null) return;

        Weapon activeWeapon = GetActiveWeapon();

        if (activeWeapon != null && activeWeapon.IsBroken())
        {
            Debug.Log("โจมตีไม่ได้! อาวุธพังแล้ว ต้องซ่อมก่อนกด C");

            if (audioSource != null && brokenWeaponSound != null)
            {
                audioSource.PlayOneShot(brokenWeaponSound);
            }

            nextAttackTime = Time.time + 0.3f;

            return;
        }



        float cost = isLight ? currentWeapon.staminaCost : currentWeapon.heavyStaminaCost;
        if (currentStamina < cost)
        {
            Debug.Log("Stamina ไม่พอ!");
            return;
        }

        currentStamina -= cost;
        float cooldown = isLight ? currentWeapon.lightAttackCooldown : currentWeapon.heavyAttackCooldown;
        nextAttackTime = Time.time + cooldown;

        if (activeWeapon != null)
        {
            activeWeapon.UseWeapon(durabilityLossPerHit);
        }

        pendingDamage = isLight ? currentWeapon.damage : currentWeapon.heavyDamage;

        if (currentActiveHolder != null)
        {
            Animator weaponAnim = currentActiveHolder.GetComponent<Animator>();
            if (weaponAnim && weaponAnim.isActiveAndEnabled)
            {
                weaponAnim.SetBool("IsLight", isLight);
                weaponAnim.SetTrigger("Attack");

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
        Debug.Log("💀 Player ตายแล้ว! กำลังเริ่มระบบดรอปกล่อง...");

        // 1. เช็คว่าตั้งค่าครบไหม
        if (playerDeathBoxPrefab == null) Debug.Log("❌ บัค: ลืมใส่ Prefab กล่องใน Inspector!");
        if (Inventory.Instance == null) Debug.Log("❌ บัค: หา Inventory.Instance ไม่เจอ!");

        if (playerDeathBoxPrefab != null && Inventory.Instance != null)
        {
            // 2. ดึงของออกจากกระเป๋า
            List<InventoryItem> droppedItems = Inventory.Instance.DropAllItemsExcept("Knife");
            Debug.Log("📦 จำนวนของที่จะดรอป (ไม่รวมมีด): " + droppedItems.Count + " ชิ้น");

            // 3. ถ้ามีของ ค่อยสร้างกล่อง
            if (droppedItems.Count > 0)
            {
                GameObject boxObj = Instantiate(playerDeathBoxPrefab, transform.position, Quaternion.identity);
                LootBox deathBox = boxObj.GetComponent<LootBox>();

                if (deathBox != null)
                {
                    deathBox.SetBoxContents(droppedItems);
                    Debug.Log("✅ สร้างกล่องและยัดของสำเร็จ!");
                }
                else
                {
                    Debug.Log("❌ บัค: Prefab กล่องที่คุณใส่มา ไม่มีสคริปต์ LootBox แปะอยู่!");
                }
            }
            else
            {
                Debug.Log("⚠️ ไม่สร้างกล่อง เพราะไม่มีของให้ดรอปเลยครับ");
            }
        }

        // --- ระบบ Game Over (คงเดิม) ---
        if (currentActiveHolder != null)
        {
            currentActiveHolder.GetComponent<Animator>().SetBool("IsDead", true);
        }

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

        if (currentActiveHolder != null)
        {
            currentActiveHolder.GetComponent<Animator>().SetBool("IsDead", false);
        }
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }
        this.enabled = true;
    }

    Weapon GetActiveWeapon()
    {
        foreach (Weapon weapon in allWeapons)
        {
            if (weapon != null && weapon.gameObject.activeInHierarchy)
            {
                return weapon;
            }
        }
        return null;
    }
}