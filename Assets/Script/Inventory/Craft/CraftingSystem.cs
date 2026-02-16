using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Ingredient { public ItemData item; public int amount = 1; }

[System.Serializable]
public class CraftingRecipe
{
    public string recipeName;
    public List<Ingredient> ingredients;
    public ItemData result;
    internal object resultAmount;
}

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem instance;
    void Awake() => instance = this;

    public void Craft(CraftingRecipe recipe)
    {
        foreach (var ing in recipe.ingredients)
        {
            if (!Inventory.instance.HasItem(ing.item, ing.amount)) return;
        }

        foreach (var ing in recipe.ingredients)
        {
            Inventory.instance.RemoveItem(ing.item, ing.amount);
        }

        Inventory.instance.AddItem(recipe.result);
        Debug.Log("คราฟต์สำเร็จ: " + recipe.result.itemName);
    }
}