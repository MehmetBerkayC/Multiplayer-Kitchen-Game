using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }

    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;

    private List<KitchenObjectSO> _kitchenObjectSOList;

    protected override void Awake()
    {
        base.Awake();
        // If the KitchenObject script didn't have a virtual protected Unity method
        // The line below would always get called but,
        // we also need the Awake code in the KitchenObject script to run
        // This is why this virtual Awake setup is required
        _kitchenObjectSOList = new List<KitchenObjectSO>();
        // I know this can be initialized on variable decleration but assume what if this was another code
    }

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO))
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
            OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
            {
                kitchenObjectSO = kitchenObjectSO
            });
            return true;
        }
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return _kitchenObjectSOList;
    }
}
