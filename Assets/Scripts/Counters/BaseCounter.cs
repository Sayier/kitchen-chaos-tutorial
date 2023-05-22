using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCounter : NetworkBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnItemDropped;
    public static void ResetStaticData()
    {
        OnItemDropped = null;
    }

    [SerializeField] private Transform counterTopPoint;

    private KitchenObject kitchenObject;

    //Called on Interact button press
    public virtual void Interact(Player player)
    {
        Debug.LogError($"BaseCounter.Interac() should not be accessed directly");
    }

    //Called on Alt. Interact button press
    public virtual void InteractAlternate(Player player)
    {
    }

    //Returns the transform where the counter holds an object
    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }

    //Returns the kitchen object on the counter
    public KitchenObject GetKitchenObject()
    {
        return this.kitchenObject;
    }

    //Put the kitchen object on counter and inform Sound system
    public void SetKitchenObject(KitchenObject newKitchenObject)
    {
        this.kitchenObject = newKitchenObject;

        if (kitchenObject != null)
        {
            OnItemDropped?.Invoke(this, EventArgs.Empty);
        }
    }

    //Remove kitchen object from counter
    public void ClearKitchenObject()
    {
        this.kitchenObject = null;
    }

    //Return if counter is holding a kitchen object
    public bool HasKitchenObject()
    {
        return this.kitchenObject != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
