using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;

    private List<KitchenObjectSO> _kitchenObjectSOList = new List<KitchenObjectSO>();

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO)
        {
            // Not valid ingredient
            return false;
        }

        if (_kitchenObjectSOList.Contains(kitchenObjectSO)) // Duplicate awareness
        {
            return false;
        }
        else
        {
            _kitchenObjectSOList.Add(kitchenObjectSO);
            return true;
        }
    }
}
