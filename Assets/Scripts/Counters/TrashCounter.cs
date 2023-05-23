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
        if (player.HasKitchenObject())
        {
            //Request the destruction of the KitchenObject
            KitchenObject.DestroyKitchenObject(player.GetKitchenObject());

            //Inform the Host/Clients that the Sound system should play relevant Sound clip
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
