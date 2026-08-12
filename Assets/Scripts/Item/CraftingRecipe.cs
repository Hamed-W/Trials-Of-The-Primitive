using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public RecipeIngredient[] ingredients = new RecipeIngredient[9];

    public ItemData resultItem;
    public int resultQuantity = 1;
}

[System.Serializable]
public class RecipeIngredient
{
    public ItemData itemData;
    public int quantity = 1;
}