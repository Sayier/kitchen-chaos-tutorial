using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
            AddIngredientToPlateServerRpc(MultiplayerManager.Instance.GetKitchenObjectSOIndex(kitchenObjectSO));
            return true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientToPlateServerRpc(int kitchenObjectSOIndex)
    {
        AddIngredientToPlateClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void AddIngredientToPlateClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = MultiplayerManager.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);

        kitchenObjectSOOnPlateList.Add(kitchenObjectSO);
        OnIngredientAddedToPlate?.Invoke(this, new OnIngredientAddedToPlateEventArgs
        {
            kitchenIngredientSO = kitchenObjectSO
        });
    }

    public List<KitchenObjectSO> GetKitchenObjectSOOnPlateList()
    {
        return kitchenObjectSOOnPlateList;
    }
}
