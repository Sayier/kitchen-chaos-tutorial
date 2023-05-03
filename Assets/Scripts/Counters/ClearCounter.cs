using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject() && !HasKitchenObject())
        {
            //Player is holding something and the counter is empty

            player.GetKitchenObject().SetKitchenObjectParent(this);
        }
        else if(!player.HasKitchenObject() && HasKitchenObject())
        {
            //Player is not holding anything and counter is occupied

            this.GetKitchenObject().SetKitchenObjectParent(player);
        }
        else if(player.HasKitchenObject() && player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject) && HasKitchenObject() )
        {
            //Player is holding a plate and counter is occupied

            bool hasAddedKitchenObjectToPlate =  plateKitchenObject.TryAddIngrediant(GetKitchenObject().GetKitchenObjectSO());
            if (hasAddedKitchenObjectToPlate)
            {
                GetKitchenObject().DestroySelf();
            }
        }
        else if(player.HasKitchenObject() && GetKitchenObject().TryGetPlate(out plateKitchenObject))
        {
            //Player is holding something and a plate is on the counter

            bool hasAddedKitchenObjectToPlate = plateKitchenObject.TryAddIngrediant(player.GetKitchenObject().GetKitchenObjectSO());
            if (hasAddedKitchenObjectToPlate)
            {
                player.GetKitchenObject().DestroySelf();
            }
        }
    }
}
