using UnityEngine;
using System.Collections.Generic;

public class VisionFOV : MonoBehaviour
{
    [Header("Settings")]
    public float fov = 140f;          // องศาการมองเห็น (ตามภาพแนะนำ 120-150)
    public float viewDistance = 10f;  // ระยะการมองเห็น
    public int rayCount = 50;         // ความละเอียดของ Mesh (ยิ่งเยอะยิ่งเนียน)
    public LayerMask obstacleMask;    // Layer ของกำแพง/สิ่งของ

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
    }

    void DrawFOV()
    {
        float currentAngle = -fov / 2;
        float angleStep = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero; // จุดเริ่มต้นคือที่ตัวละคร

        for (int i = 0; i <= rayCount; i++)
        {
            // คำนวณทิศทางของ Ray
            Vector3 direction = Quaternion.Euler(0, 0, currentAngle) * transform.up;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, viewDistance, obstacleMask);

            if (hit.collider != null)
            {
                // ถ้าชนกำแพง ให้จุดของ Mesh อยู่ที่จุดที่ชน
                vertices[i + 1] = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                // ถ้าไม่ชน ให้จุดอยู่ที่ระยะสูงสุด
                vertices[i + 1] = transform.InverseTransformPoint(transform.position + direction * viewDistance);
            }

            if (i < rayCount)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            currentAngle += angleStep;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}