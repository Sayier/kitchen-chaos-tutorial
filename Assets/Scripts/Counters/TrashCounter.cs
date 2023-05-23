using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrashCounter : BaseCounter
{
    public static event EventHandler OnItemThrownOut;
    new public static void ResetStaticData()
    {
        OnItemThrownOut = null;
    }

    public override void Interact(Player player)
    {   
        //If player is holding an item destory it and inform Sound system 
        if (player.HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(player.GetKitchenObject());

            InteractLogicServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        OnItemThrownOut.Invoke(this, EventArgs.Empty);
    }
}
