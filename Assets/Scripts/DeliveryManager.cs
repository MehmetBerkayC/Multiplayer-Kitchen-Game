using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;

    private List<RecipeSO> _waitingRecipeSOList = new();
    private int _waitingRecipesMax = 4;

    private float _spawnRecipeTimer;
    private float _spawnRecipeTimerMax = 4f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (_waitingRecipeSOList.Count >= _waitingRecipesMax) return; // Order Limit
        _spawnRecipeTimer -= Time.deltaTime;
        if (_spawnRecipeTimer <= 0f)
        {
            _spawnRecipeTimer = _spawnRecipeTimerMax;

            RecipeSO waitingRecipeSO = recipeListSO.RecipeSOList[Random.Range(0, recipeListSO.RecipeSOList.Count)];
            Debug.Log(waitingRecipeSO.RecipeName);
            _waitingRecipeSOList.Add(waitingRecipeSO);
        }
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < _waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = _waitingRecipeSOList[i];

            if (waitingRecipeSO.KitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                // Has the same number of ingredients
                bool plateContentsMatchesRecipe = true;
                foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.KitchenObjectSOList)
                {
                    bool ingredientFound = false;
                    // Cycle through all ingredients in the recipe
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        // Cycle through all ingredients in the plate
                        if (plateKitchenObjectSO == recipeKitchenObjectSO)
                        {
                            // Ingredient Matches!
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound)
                    {
                        // Recipe ingredient wasn't found on the plate
                        plateContentsMatchesRecipe = false;
                    }
                }
                if (plateContentsMatchesRecipe)
                {
                    // Plate delivered the correct recipe!
                    Debug.Log("Recipe Delivered!");
                    _waitingRecipeSOList.RemoveAt(i);
                    return;
                }
            }
        }
        // No matches found!
        // Delivered recipe was wrong!
        Debug.Log("Invalid recipe delivered!");
    }
}
