using UnityEngine;
using System.Collections;

public class BossAttackRandomizer : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private string[] attackTriggers = { "Attack01", "Attack02", "Attack03" };
    [SerializeField] private float minAttackDelay = 2f;
    [SerializeField] private float maxAttackDelay = 5f;

    private Coroutine attackRoutine; // ตัวเก็บสถานะลูปโจมตี

    // ⭐ เปลี่ยนจาก Start() เป็น OnEnable() 
    // เพื่อให้มันเริ่มทำงาน "ทุกครั้ง" ที่ถูกเควสปลดล็อค (เปิด SetActive เป็น true)
    void OnEnable()
    {
        animator = GetComponent<Animator>();

        if (animator != null && attackTriggers.Length > 0)
        {
            // สั่งเริ่มลูปโจมตีทันทีที่บอสโผล่มา
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    // ⭐ ถ้าบอสถูกปิด (SetActive เป็น false) ให้หยุดการสุ่มตีด้วย
    void OnDisable()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            float delay = Random.Range(minAttackDelay, maxAttackDelay);
            yield return new WaitForSeconds(delay);

            // 🛡️ เพิ่มบรรทัดนี้เพื่อเช็คก่อนว่าบอสยังไม่ตาย
            if (this == null) yield break;

            // เช็คลำโพงก่อนสั่งเล่นเสียง
            AudioSource source = GetComponent<AudioSource>();
            if (source != null)
            {
                source.Play();
            }

            int randomIndex = Random.Range(0, attackTriggers.Length);
            string selectedTrigger = attackTriggers[randomIndex];

            if (animator != null)
            {
                animator.SetTrigger(selectedTrigger);
            }
        }
    }
}