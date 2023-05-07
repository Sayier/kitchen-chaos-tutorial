using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCounter : BaseCounter
{
    public static event EventHandler OnItemThrownOut;

    public override void Interact(Player player)
    {   
        //If player is holding an item destory it and inform Sound system 
        if (player.HasKitchenObject())
        {
            player.GetKitchenObject().DestroySelf();
            OnItemThrownOut.Invoke(this, EventArgs.Empty);
        }
    }
}
