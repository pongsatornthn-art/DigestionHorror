using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public List<Ingredient> ingredients;

    [Header("Result")]
    public ItemData result;

    // ⭐ ต้องเป็น int เท่านั้นครับ ห้ามเป็น object
    public int resultAmount = 1;
}

[System.Serializable]
public class Ingredient
{
    public ItemData item;
    public int amount = 1;
}