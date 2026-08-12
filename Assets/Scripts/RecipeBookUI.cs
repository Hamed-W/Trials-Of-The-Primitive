using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeBookUI : MonoBehaviour
{
    [SerializeField] private CraftingManager craftingManager;

    [Header("Recipe List")]
    [SerializeField] private Transform recipeListContent;
    [SerializeField] private GameObject recipeButtonPrefab;

    [Header("Recipe Display")]
    [SerializeField] private List<Image> ingredientIcons;
    [SerializeField] private List<TMP_Text> ingredientQuantities;

    [SerializeField] private Image resultIcon;
    [SerializeField] private TMP_Text resultName;
    [SerializeField] private TMP_Text resultQuantity;

    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text statModifiersText;

    private void Start()
    {
        CreateRecipeList();

        if (craftingManager.recipes.Count > 0)
        {
            ShowRecipe(craftingManager.recipes[0]);
        }
    }

    private void CreateRecipeList()
    {
        foreach (CraftingRecipe recipe in craftingManager.recipes)
        {
            GameObject buttonObject = Instantiate(recipeButtonPrefab, recipeListContent);
            Button button = buttonObject.GetComponent<Button>();

            TMP_Text text = buttonObject.GetComponentInChildren<TMP_Text>();
            text.text = recipe.resultItem.itemName;

            Image image = buttonObject.GetComponentInChildren<Image>();
            image.sprite = recipe.resultItem.icon;


            CraftingRecipe capturedRecipe = recipe;

            button.onClick.AddListener(() => {ShowRecipe(capturedRecipe);});
        }
    }

    private void ShowRecipe(CraftingRecipe recipe)
    {
        for (int i = 0; i < 9; i++)
        {
            RecipeIngredient ingredient = recipe.ingredients[i];

            if (ingredient != null && ingredient.itemData != null)
            {
                ingredientIcons[i].enabled = true;
                ingredientIcons[i].sprite = ingredient.itemData.icon;
                ingredientQuantities[i].text = ingredient.quantity > 1 ? ingredient.quantity.ToString(): "";
            }
            else
            {
                ingredientIcons[i].enabled = false;
                ingredientQuantities[i].text = "";
            }
        }

        resultIcon.sprite = recipe.resultItem.icon;
        resultIcon.enabled = true;
        resultName.text = recipe.resultItem.itemName;
        resultQuantity.text = recipe.resultQuantity > 1 ? "x" + recipe.resultQuantity : "";

        ItemData item = recipe.resultItem;
        descriptionText.text = item.description;

        statModifiersText.text = "";

        if (item.itemUseType == ItemUseType.Swing) return;
        if (item.statModifiers != null && item.statModifiers.Count > 0)
        {
            foreach (EquipmentStatModifiers modifier in item.statModifiers)
            {
                string text = GetStatName(modifier.statType) + ": " + modifier.amount.ToString() + "\n";
                statModifiersText.text += text;
            }
        }
        else
        {
            statModifiersText.text = "None";
        }
    }

    private string GetStatName(PlayerStatType statType)
    {
        switch (statType)
        {
            case PlayerStatType.MaxHealth:
                return "Max Health";

            case PlayerStatType.MovementSpeed:
                return "Movement Speed";

            case PlayerStatType.AttackSpeed:
                return "Attack Speed";

            case PlayerStatType.Damage:
                return "Damage";

            default:
                return statType.ToString();
        }
    }

}