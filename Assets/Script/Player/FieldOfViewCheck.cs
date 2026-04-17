using UnityEngine;
using System.Collections.Generic;

public class FieldOfViewCheck : MonoBehaviour
{
    [Header("Settings")]
    public float viewRadius = 10f;      // ระยะมองเห็น
    [Range(0, 360)] public float viewAngle = 140f; // องศาการมองเห็น

    [Header("Layers")]
    public LayerMask obstacleMask;      // Layer กำแพง (Wall)
    public LayerMask targetMask;        // Layer ศัตรู (Enemy)

    // ลิสต์จำรายชื่อศัตรูที่ "มองเห็นในปัจจุบัน" (เพื่อกันการกระพริบ)
    private List<Transform> visibleTargets = new List<Transform>();

    void LateUpdate()
    {
        FindVisibleTargets();
    }

    void FindVisibleTargets()
    {
        // 1. สร้างลิสต์ชั่วคราว เพื่อจดชื่อศัตรูที่ "เห็นในรอบนี้"
        List<Transform> targetsSeenThisFrame = new List<Transform>();

        // 2. หาศัตรูทั้งหมดในระยะวงกลม
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            // 3. เช็คองศา (อยู่ในกรวยสายตาไหม?)
            if (Vector3.Angle(transform.up, dirToTarget) < viewAngle / 2)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);

                // 4. เช็คกำแพง (มีกำแพงบังไหม?)
                if (!Physics2D.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    // ผ่านทุกเงื่อนไข!
                    targetsSeenThisFrame.Add(target);
                }
            }
        }

        // --- ระบบจัดการเปิด/ปิด (แก้กระพริบ) ---

        // A. ไล่ดูรายชื่อเก่า: ใครที่ "เคยเห็น" แต่ "รอบนี้ไม่เห็นแล้ว" -> สั่งซ่อน (Hide)
        foreach (Transform oldTarget in visibleTargets)
        {
            if (oldTarget != null && !targetsSeenThisFrame.Contains(oldTarget))
            {
                var renderer = oldTarget.GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
        }

        // B. ไล่ดูรายชื่อใหม่: ใครที่ "เห็นในรอบนี้" -> สั่งแสดงตัว (Show) และเช็คผี
        foreach (Transform newTarget in targetsSeenThisFrame)
        {
            if (newTarget != null)
            {
                // 1. แสดงตัวศัตรู
                var renderer = newTarget.GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.enabled = true;

                // 2. ถ้าสิ่งที่เห็นคือ EnemySpirit ให้สั่งให้มันหนี!
                EnemySpirit spirit = newTarget.GetComponent<EnemySpirit>();
                if (spirit != null)
                {
                    spirit.SetScared();
                }
            }
        }

        // C. อัปเดตลิสต์หลักให้เป็นปัจจุบัน
        visibleTargets = new List<Transform>(targetsSeenThisFrame);
    }

    // วาดเส้นใน Scene ให้เห็นระยะตอนปรับค่า (Gizmos)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}