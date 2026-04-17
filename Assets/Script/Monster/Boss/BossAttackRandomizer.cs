using UnityEngine;
using System.Collections;

public class BossAttackRandomizer : MonoBehaviour
{
    private Animator animator;

    // ตั้งชื่อให้ตรงกับ Trigger ที่คุณสร้างใน Animator Window
    [SerializeField] private string[] attackTriggers = { "Attack01", "Attack02", "Attack03" };
    [SerializeField] private float minAttackDelay = 2f;
    [SerializeField] private float maxAttackDelay = 5f;

    void Start()
    {
        animator = GetComponent<Animator>();
        // เริ่มลูปการสุ่มโจมตี
        StartCoroutine(AttackRoutine());
    }


    IEnumerator AttackRoutine()
    {
        while (true)
        {
            // รอเวลาสุ่มก่อนการโจมตีครั้งต่อไป
            float delay = Random.Range(minAttackDelay, maxAttackDelay);
            yield return new WaitForSeconds(delay);

            if (GetComponent<AudioSource>() != null)
                GetComponent<AudioSource>().Play();

            // สุ่มเลือก Index จากรายการ Trigger
            int randomIndex = Random.Range(0, attackTriggers.Length);
            string selectedTrigger = attackTriggers[randomIndex];

            // สั่งให้ Animator เล่นท่าโจมตี
            animator.SetTrigger(selectedTrigger);

            Debug.Log("Boss performs: " + selectedTrigger);
        }
    }
}