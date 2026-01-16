using UnityEngine;
using System.Collections.Generic;

public class CraftingController : MonoBehaviour
{
    // ลิสต์รายการสูตร (ลากใส่ใน Inspector เหมือนเดิม)
    public List<CraftingRecipe> recipes;

    void Update()
    {
        // เปลี่ยนเป็นเช็คปุ่ม C ปุ่มเดียว
        if (Input.GetKeyDown(KeyCode.C))
        {
            TryCraftAll();
        }
    }

    void TryCraftAll()
    {
        // วนลูปเช็ค "ทุกสูตร" ที่มีในลิสต์ recipes
        foreach (var recipe in recipes)
        {
            // ส่งสูตรไปให้ระบบหลักจัดการ
            // (ในระบบหลัก มันมีตัวเช็คของอยู่แล้ว ถ้าของไม่พอ มันจะข้ามไปเอง ไม่ Error ครับ)
            CraftingSystem.instance.Craft(recipe);
        }
    }
}