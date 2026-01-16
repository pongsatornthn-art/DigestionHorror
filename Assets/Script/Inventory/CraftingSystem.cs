using UnityEngine;
using System.Collections.Generic;

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem instance;
    void Awake() { instance = this; }

    public void Craft(CraftingRecipe recipe)
    {
        // 1. เช็คของ
        foreach (var item in recipe.ingredients)
        {
            if (!Inventory.instance.HasItem(item)) return; // ของไม่ครบ จบข่าว
        }

        // 2. ลบวัตถุดิบ
        foreach (var item in recipe.ingredients)
        {
            Inventory.instance.RemoveItem(item);
        }

        // 3. ให้ผลลัพธ์
        Inventory.instance.AddItem(recipe.result);
        Debug.Log("คราฟต์เสร็จสิ้น: " + recipe.result.itemName);
    }
}

// สร้างคลาสสำหรับใบสูตรไว้ในไฟล์เดียวกันเลยก็ได้
[System.Serializable]
public class CraftingRecipe
{
    public string recipeName;
    public List<ItemData> ingredients;
    public ItemData result;
}