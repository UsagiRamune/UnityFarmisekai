using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
public class RecipeBlock : MonoBehaviour
{
    public int ID;
    [Header("Result")] public Image resultImage;
    public TextMeshProUGUI resultCountText;

    [Header("Ingredients")] public Image[] ingredientImages;
    public TextMeshProUGUI[] ingredientCountTexts;

    [Header("Background")] public Image background;

    private RecipeDataSO.RecipeData recipe;

    public void OnClickSelect()
    {
        if (recipe != null)
        {
            RecipeManager.Instance.SelectRecipe(recipe, this);
        }
    }

    public void InitialiseRecipe(RecipeDataSO.RecipeData recipeData)
    {
        recipe = recipeData;
        ID = recipeData.ID;
        // Result
        resultImage.sprite = recipeData.resultItem.sprite;
        resultCountText.text = recipeData.resultCount.ToString();
        if (recipeData.resultCount == 1)
        {
            resultCountText.gameObject.SetActive(false);
        }
        HoverManager.Instance.Register(new HoverableItem { Target = resultImage.rectTransform, ItemName = recipe.resultItem.itemname });
        // Ingredients
        for (int i = 0; i < ingredientImages.Length; i++)
        {
            if (i < recipeData.ingredients.Count)
            {
                var ing = recipeData.ingredients[i];
                ingredientImages[i].sprite = ing.item.sprite;
                ingredientCountTexts[i].text = ing.count.ToString();
                ingredientImages[i].gameObject.SetActive(true);
                ingredientCountTexts[i].gameObject.SetActive(true);
                if (ing.count == 1)
                {
                    ingredientCountTexts[i].gameObject.SetActive(false);
                }
                HoverManager.Instance.Register(new HoverableItem { Target = ingredientImages[i].rectTransform, ItemName = recipe.ingredients[i].item.itemname });
            }
            else
            {
                ingredientImages[i].gameObject.SetActive(false);
                ingredientCountTexts[i].gameObject.SetActive(false);
            }
        }

        UpdateCraftable();
    }
  
    private void OnEnable()
    {
        InventoryManager.Instance.OnInventoryChanged += UpdateCraftable;
    }

    private void OnDisable()
    {
        InventoryManager.Instance.OnInventoryChanged -= UpdateCraftable;
    }

    public void UpdateCraftable()
    {
        bool canCraft = RecipeManager.Instance.CanCraft(recipe);
        Color color = canCraft ? new Color(0, .75f, 0, 0.5f) : new Color(0, 0, 0, 0.5f);

        if (background != null)
            background.color = color;
    }
}