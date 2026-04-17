using UnityEngine;
using System.Collections;

public class EnemyGrass : MonoBehaviour
{
    [Header("Settings")]
    public float trapDuration = 2f;
    public float damagePerSecond = 2f;
    public float digestionPerSecond = 5f;
    public float cooldown = 3f;

    private bool isCooldown = false;
    private Animator anim; // เพิ่มตัวแปร Animator

    void Start()
    {
        // ดึง Component Animator มาเก็บไว้
        anim = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCooldown)
        {
            StartCoroutine(TrapPlayer(other.gameObject));
        }
    }

    IEnumerator TrapPlayer(GameObject player)
    {
        isCooldown = true;
        PlayerStatus status = player.GetComponent<PlayerStatus>();

        if (status != null)
        {
            // --- สั่งเล่น Animation Trap ---
            if (anim != null)
            {
                anim.SetTrigger("isTrapping");
            }

            status.isRooted = true;
            Debug.Log("Player ถูกหญ้าจับไว้!");

            float elapsed = 0f;
            float damageAccumulator = 0f;

            while (elapsed < trapDuration)
            {
                if (DigestionSystem.instance != null)
                    DigestionSystem.instance.IncreaseDigestion(digestionPerSecond * Time.deltaTime);

                if (PlayerController.instance != null)
                {
                    damageAccumulator += damagePerSecond * Time.deltaTime;
                    if (damageAccumulator >= 1f)
                    {
                        int damageToDeal = Mathf.FloorToInt(damageAccumulator);
                        PlayerController.instance.PlayerTakeDamage(damageToDeal);
                        damageAccumulator -= damageToDeal;
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            status.isRooted = false;
            Debug.Log("Player หลุดจากการโดนจับ");
        }

        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
    }
}