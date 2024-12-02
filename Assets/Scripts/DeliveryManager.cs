using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;

    private List<RecipeSO> _waitingRecipeSOList = new();
    private int _waitingRecipesMax = 4;

    private float _spawnRecipeTimer = 4f;
    private float _spawnRecipeTimerMax = 4f;

    private int _successfulRecipesAmount;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer) { return; }

        if (_waitingRecipeSOList.Count >= _waitingRecipesMax) return; // Order Limit
        _spawnRecipeTimer -= Time.deltaTime;
        if (_spawnRecipeTimer <= 0f)
        {
            _spawnRecipeTimer = _spawnRecipeTimerMax;

            if (GameManager.Instance.IsGamePlaying())
            {
                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.RecipeSOList.Count);

                SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);
            }
        }
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex)
    {
        RecipeSO waitingRecipeSO = recipeListSO.RecipeSOList[waitingRecipeSOIndex];
        _waitingRecipeSOList.Add(waitingRecipeSO);

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
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
                    DeliverCorrectRecipeServerRpc(i);

                    return;
                }
            }
        }
        // No matches found!
        // Delivered recipe was wrong!
        DeliverIncorrectRecipeServerRpc();
    }

    // HOST IS ALSO A CLIENT, NOT A DEDICATED SERVER
    #region Correct Recipe
    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSoListIndex) // Code only runs on host(server)
    {
        DeliverCorrectRecipeClientRpc(waitingRecipeSoListIndex);
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSoListIndex) // The code that will exec on all clients + host(server)
    {
        _successfulRecipesAmount++;

        _waitingRecipeSOList.RemoveAt(waitingRecipeSoListIndex);

        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Incorrect Recipe
    [ServerRpc(RequireOwnership = false)]
    private void DeliverIncorrectRecipeServerRpc() // Code only runs on host(server)
    {
        DeliverIncorrectRecipeClientRpc();
    }

    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc() // The code that will exec on all clients + host(server) 
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return _waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount()
    {
        return _successfulRecipesAmount;
    }
}
