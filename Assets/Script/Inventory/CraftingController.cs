using UnityEngine;
using System.Collections.Generic;

public class CraftingController : MonoBehaviour
{
    // สร้างลิสต์รายการสูตร เอาไว้ใส่ใน Inspector
    public List<CraftingRecipe> recipes;

    void Update()
    {
        // กดปุ่มเลข 1, 2, 3 เพื่อสั่งคราฟต์ตามลำดับ
        if (Input.GetKeyDown(KeyCode.Alpha1)) CraftItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) CraftItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) CraftItem(2);
    }

    void CraftItem(int index)
    {
        // เช็คว่ามีสูตรนี้จริงไหม (กัน Error)
        if (index < recipes.Count)
        {
            CraftingSystem.instance.Craft(recipes[index]);
        }
    }
}