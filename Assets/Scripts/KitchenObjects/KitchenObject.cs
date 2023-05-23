using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    private IKitchenObjectParent kitchenObjectParent;
    private FollowTransform followTransform;

    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    //Inform the server that the KitchObjectParent needs to be updated
    public void SetKitchenObjectParent(IKitchenObjectParent newKitchenObjectParent)
    {
        SetKitchenObjectParentServerRpc(newKitchenObjectParent.GetNetworkObject()); 
    }

    //Broadcast to all the clients that the KitchenObjectParent has been updated
    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectReference);
    }


    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        //Get the which KitchenObjectParent should be associated with the spawn KitchenObject by pulling it from
        //the from the NetworkObject in the passed NetworkObjectReference
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();

        //Clear the current KitchenObjectParent refence from the KitchenObject prior to updating
        if (this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();
        }
        this.kitchenObjectParent = kitchenObjectParent;

        //Should be checked prior to calling SetKitchenObjectParent initially, but
        //throw error if the KitchenObjectParent is already holding something
        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError($"{kitchenObjectParent} already has a kitchen object");
        }

        //Update both the KitchenObjectParent and the transform that the KitchenObject should now follow
        kitchenObjectParent.SetKitchenObject(this);
        followTransform.SetTargetTransform(kitchenObjectParent.GetKitchenObjectFollowTransform());
    }

    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return kitchenObjectParent;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    //Disassociate the KitchenObject with its current KitchenObjectParent
    public void ClearKitchenObjectOnParent()
    {
        kitchenObjectParent.ClearKitchenObject();
    }

    //Returns true if this KitchenObject is a PlateKitchenObject (and return a reference to the PlateKitchenObject)
    //Otherwise returns false if this KitchenObject is anything else (and set the PlateKitchenObject reference to null)
    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if(this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        plateKitchenObject = null;
        return false;
    }

    //Initiate spawning the KitchenObject (processed by the MultiplayerManager for network syncing)
    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        MultiplayerManager.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
    }

    //Initiate destroying the KitchenObject (processed by the MultiplayerManager for network syncing)
    public static void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        MultiplayerManager.Instance.DestroyKitchenObject(kitchenObject);
    }
}
