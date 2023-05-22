using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<OnIngredientAddedToPlateEventArgs> OnIngredientAddedToPlate;
    public class OnIngredientAddedToPlateEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenIngredientSO;
    }

    [SerializeField] List<KitchenObjectSO> validKitchenObjectSOList;

    private List<KitchenObjectSO> kitchenObjectSOOnPlateList;

    protected override void Awake()
    {
        base.Awake();
        kitchenObjectSOOnPlateList = new List<KitchenObjectSO>();
    }

    public bool TryAddIngrediant(KitchenObjectSO kitchenObjectSO)
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO))
        {
            return false;
        }
        if (kitchenObjectSOOnPlateList.Contains(kitchenObjectSO))
        {
            return false;
        }
        else
        {
            kitchenObjectSOOnPlateList.Add(kitchenObjectSO);
            OnIngredientAddedToPlate?.Invoke(this, new OnIngredientAddedToPlateEventArgs
            {
                kitchenIngredientSO = kitchenObjectSO
            });
            return true;
        }
    }

    public List<KitchenObjectSO> GetKitchenObjectSOOnPlateList()
    {
        return kitchenObjectSOOnPlateList;
    }
}
