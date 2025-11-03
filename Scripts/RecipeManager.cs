using System;
using UnityEngine;
using System.Collections.Generic;
public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance;
    public int currentRecipe;
    public GameObject indicate;
    public GameObject recipeParent;
    public GameObject recipePrefab;
    [Header("Database")]
    public RecipeDataSO recipeDatabase;

    [Header("Inventory Reference")]
    public InventoryManager inventoryManager;

    private RecipeBlock currentBlock;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Update()
    {
        if (currentRecipe == 0)
        {
            indicate.SetActive(false);
        }
        else
        {
            indicate.SetActive(true);
            indicate.transform.position = currentBlock.transform.position;
        }
    }
    public void SelectRecipe(RecipeDataSO.RecipeData recipe, RecipeBlock block)
    {
        currentRecipe = recipe.ID;

        indicate.SetActive(true);
     currentBlock = block;

        // Optionally, make sure it’s on top in hierarchy
        indicate.transform.SetAsLastSibling();
    }
    public bool CanCraft(RecipeDataSO.RecipeData recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            int required = ingredient.count;
            int owned = InventoryManager.Instance.GetTotalItemCount(ingredient.item);

            // Include item being held on cursor
            if (InventoryManager.Instance.itemBeingHeld != null &&
                InventoryManager.Instance.itemBeingHeld.item == ingredient.item)
            {
                owned += InventoryManager.Instance.itemBeingHeld.count;
            }

            if (owned < required)
                return false;
        }
        return true;
    }
    public void OnClickCraft()
    {
        RecipeManager.Instance.CraftCurrent();
    }
    public void CraftCurrent()
    {
        // Find the recipe by currentRecipe ID
        RecipeDataSO.RecipeData recipe = recipeDatabase.recipeDatas
            .Find(r => r.ID == currentRecipe);

        if (recipe == null)
        {
            Debug.LogWarning("❌ No recipe selected to craft!");
            return;
        }

        Craft(recipe); // call your existing Craft method
    }
    public void Craft(RecipeDataSO.RecipeData recipe)
    {
        if (!CanCraft(recipe))
        {
            Debug.LogWarning("❌ Not enough ingredients to craft " + recipe.name);
            return;
        }
        List<InventorySlot> freedSlots = new List<InventorySlot>();
        foreach (var ingredient in recipe.ingredients)
        {
            freedSlots.AddRange(inventoryManager.ConsumeItemAndReturnFreedSlots(ingredient.item, ingredient.count));
        }

        // 2️⃣ Try to add crafted item properly (handles stacking & empty slots)
        int added = inventoryManager.AddItem(recipe.resultItem, recipe.resultCount);
        int leftover = recipe.resultCount - added;

        // 3️⃣ If inventory full — drop into the world instead
        if (leftover > 0)
        {
            Debug.LogWarning($"⚠️ Inventory full — dropping {leftover} {recipe.resultItem.name}!");

            GameObject dropped = Instantiate(
                inventoryManager.worldItemPrefab,
                inventoryManager.dropPoint.position,
                Quaternion.identity
            );
            dropped.GetComponentInChildren<WorldItem>().Initialise(recipe.resultItem, leftover);
        }

        Debug.Log($"✅ Crafted {recipe.resultItem.name} x{recipe.resultCount} (Added {added}, Dropped {leftover})");
    }

    public RecipeDataSO.RecipeData GetRecipeByResult(Item resultItem)
    {
        foreach (var recipe in recipeDatabase.recipeDatas)
        {
            if (recipe.resultItem == resultItem)
                return recipe;
        }
        return null;
    }
    
    public List<RecipeDataSO.RecipeData> GetCraftableRecipes()
    {
        List<RecipeDataSO.RecipeData> craftable = new List<RecipeDataSO.RecipeData>();
        foreach (var recipe in recipeDatabase.recipeDatas)
        {
            if (CanCraft(recipe))
                craftable.Add(recipe);
        }
        return craftable;
    }
    public void GenerateRecipeUI(RecipeType type)
    {
        // Clear old UI
        foreach (Transform child in recipeParent.transform)
            Destroy(child.gameObject);

        // Spawn new recipe prefabs
        foreach (var recipe in recipeDatabase.recipeDatas)
        {
            if (recipe.recipeType != type)
                continue;

            GameObject go = Instantiate(recipePrefab, recipeParent.transform);
            go.name = recipe.ID.ToString();
            // Get the RecipeSlot component (manages result + ingredients + highlight)
            var recipeSlot = go.GetComponent<RecipeBlock>();
            if (recipeSlot != null)
            {
                recipeSlot.InitialiseRecipe(recipe);
            }
        }
    }
}
