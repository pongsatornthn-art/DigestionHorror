using UnityEngine;
using System.Collections.Generic;

public class VisionFOV : MonoBehaviour
{
    [Header("Visual Settings (การวาดแสง)")]
    public float fov = 140f;
    public float viewDistance = 10f;
    public int rayCount = 50;
    public LayerMask obstacleMask;

    [Header("Gameplay Logic (การซ่อนศัตรู)")]
    public LayerMask targetMask;
    [Tooltip("ระยะหน่วง: ศัตรูต้องเดินออกไปไกลกว่าระยะมองเห็นกี่หน่วยถึงจะหายไป (ช่วยแก้กระพริบ)")]
    public float hideThreshold = 1.5f; // ⭐ ตัวช่วยแก้กระพริบ

    public List<Transform> visibleTargets = new List<Transform>();

    private Mesh mesh;
    private MeshFilter meshFilter;

    void Start()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    void LateUpdate()
    {
        DrawFOV();
        FindVisibleTargets(); // ย้ายมาทำทุกเฟรมให้ภาพนิ่งที่สุด
    }

    void FindVisibleTargets()
    {
        List<Transform> currentLoopTargets = new List<Transform>();

        // 1. ค้นหาในระยะที่ "กว้างขึ้นนิดหน่อย" (เผื่อสำหรับคนที่กำลังจะหลุดเฟรม)
        float searchRadius = viewDistance + hideThreshold;
        Collider2D[] targetsInRadius = Physics2D.OverlapCircleAll(transform.position, searchRadius, targetMask);

        foreach (Collider2D targetCol in targetsInRadius)
        {
            Transform target = targetCol.transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float dstToTarget = Vector3.Distance(transform.position, target.position);

            // --- กฎการมองเห็น (Logic แบบมี Buffer) ---
            bool isVisible = false;

            // เช็คว่าเคยเห็นอยู่แล้วหรือเปล่า?
            if (visibleTargets.Contains(target))
            {
                // ถ้า "เคยเห็นอยู่แล้ว" ยอมให้อยู่ไกลได้ถึง (ระยะมอง + ระยะหน่วง)
                if (dstToTarget <= viewDistance + hideThreshold)
                {
                    if (IsTargetInAngleAndSight(dirToTarget, dstToTarget)) isVisible = true;
                }
            }
            else
            {
                // ถ้า "ยังไม่เคยเห็น" ต้องเข้ามาในระยะจริงเท่านั้น (ระยะมองเป๊ะๆ)
                if (dstToTarget <= viewDistance)
                {
                    if (IsTargetInAngleAndSight(dirToTarget, dstToTarget)) isVisible = true;
                }
            }

            if (isVisible)
            {
                currentLoopTargets.Add(target);
            }
        }

        // 2. จัดการ เปิด/ปิด

        // ใครที่เคยเห็น -> แต่รอบนี้ไม่เห็นแล้ว -> ปิด
        foreach (Transform oldTarget in visibleTargets)
        {
            if (oldTarget != null && !currentLoopTargets.Contains(oldTarget))
            {
                var rend = oldTarget.GetComponent<SpriteRenderer>();
                if (rend) rend.enabled = false;
            }
        }

        // ใครที่เห็นในรอบนี้ -> เปิด
        foreach (Transform newTarget in currentLoopTargets)
        {
            if (newTarget != null)
            {
                var rend = newTarget.GetComponent<SpriteRenderer>();
                if (rend) rend.enabled = true;
            }
        }

        // อัปเดตลิสต์
        visibleTargets = new List<Transform>(currentLoopTargets);
    }

    // ฟังก์ชันช่วยเช็ค องศา และ กำแพง
    bool IsTargetInAngleAndSight(Vector3 dir, float dst)
    {
        // เช็คองศา
        if (Vector3.Angle(transform.up, dir) < fov / 2)
        {
            // เช็คกำแพง (Wall)
            if (!Physics2D.Raycast(transform.position, dir, dst, obstacleMask))
            {
                return true;
            }
        }
        return false;
    }

    void DrawFOV()
    {
        // (โค้ดวาดแสงส่วนนี้เหมือนเดิมครับ ไม่ต้องแก้)
        float currentAngle = -fov / 2;
        float angleStep = fov / rayCount;
        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];
        vertices[0] = Vector3.zero;

        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 direction = Quaternion.Euler(0, 0, currentAngle) * transform.up;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, viewDistance, obstacleMask);

            if (hit.collider != null) vertices[i + 1] = transform.InverseTransformPoint(hit.point);
            else vertices[i + 1] = transform.InverseTransformPoint(transform.position + direction * viewDistance);

            if (i < rayCount) { triangles[i * 3] = 0; triangles[i * 3 + 1] = i + 1; triangles[i * 3 + 2] = i + 2; }
            currentAngle += angleStep;
        }
        mesh.Clear(); mesh.vertices = vertices; mesh.triangles = triangles; mesh.RecalculateNormals();
    }
}