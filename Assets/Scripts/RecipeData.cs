using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TriInspector;

public enum RecipeType
{
    Default,
    Forge,
    Cooking,
    Smithing
}
[CreateAssetMenu(menuName = "Crafting/Recipe Database")]
public class RecipeDataSO : ScriptableObject
{
    [ListDrawerSettings(Draggable = true,
        HideAddButton = true,
        HideRemoveButton = true,
        AlwaysExpanded = false,
        ShowElementLabels = true)]
    public List<RecipeData> recipeDatas = new List<RecipeData>();

    [Button(ButtonSizes.Large, "Add New Recipe")]
    private void AddRecipeButton()
    {
        AddNewRecipe();
    }

    public void AddNewRecipe()
    {
        RecipeData newRecipe = new RecipeData();
        if (recipeDatas.Count == 0)
            newRecipe.ID = 1;
        else
            newRecipe.ID = recipeDatas.Max(r => r.ID) + 1;

        recipeDatas.Add(newRecipe);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void RemoveRecipe(RecipeData recipe)
    {
        recipeDatas.Remove(recipe);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    private void OnValidate()
    {
#if UNITY_EDITOR
        // Enforce ingredient limit at the ScriptableObject level
        const int MaxIngredients = 4;

        foreach (var recipe in recipeDatas)
        {
            if (recipe.ingredients.Count > MaxIngredients)
            {
                Debug.LogWarning($"⚠️ {recipe.name} has too many ingredients! Capping to {MaxIngredients}.");
                recipe.ingredients = recipe.ingredients.GetRange(0, MaxIngredients);
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    // ────────────────────────────────────────────────
    [Serializable]
    public class RecipeData
    {
        [SerializeField] public string name = "New Recipe";
        [SerializeField] public int ID;
        [SerializeField] public RecipeType recipeType =  RecipeType.Default;
        [Title("Result")]
        public Item resultItem;
        public int resultCount = 1;

        [Title("Ingredients")]
        [ListDrawerSettings(Draggable = true, AlwaysExpanded = true)]
        public List<Ingredient> ingredients = new List<Ingredient>();
        

        [Button("Delete This Recipe")]
        private void DeleteMe()
        {
#if UNITY_EDITOR
            var so = UnityEditor.AssetDatabase.LoadAssetAtPath<RecipeDataSO>(
                UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject));

            if (so != null)
            {
                so.RemoveRecipe(this);
            }
#endif
        }
    }

    [Serializable]
    public class Ingredient
    {
        public Item item;
        public int count = 1;
    }
    
}
