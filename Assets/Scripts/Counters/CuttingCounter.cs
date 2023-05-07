using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress
{
    public static event EventHandler OnAnyCut;

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<EventArgs> OnCut;

    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    private int cuttingProgress;

    public override void Interact(Player player)
    {
        //Check if player is holding something that is able to be cut and if the cutting counter is empty
        if (player.HasKitchenObject() && !HasKitchenObject() && HasRecipeForInput(player.GetKitchenObject().GetKitchenObjectSO()))
        {
            //Put object on counter and reset cutting UI visualizer to 0
            player.GetKitchenObject().SetKitchenObjectParent(this);
            cuttingProgress = 0;

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = cuttingProgress
            });
        }
        //Check if players hands are empty and if the counter has an object to pick up
        else if (!player.HasKitchenObject() && HasKitchenObject())
        {
            //Pick up object from counter and reset cutting visualizer to 0
            GetKitchenObject().SetKitchenObjectParent(player);

            cuttingProgress = 0;

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = cuttingProgress
            });
        }
        //Check if counter has an object that can be put on a plate and whether or not the player is holding a plate
        else if (player.HasKitchenObject() && player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject) && HasKitchenObject())
        {
            //Put cut item on plate
            bool hasAddedKitchenObjectToPlate = plateKitchenObject.TryAddIngrediant(GetKitchenObject().GetKitchenObjectSO());
            if (hasAddedKitchenObjectToPlate)
            {
                GetKitchenObject().DestroySelf();
            }
        }
    }

    public override void InteractAlternate(Player player)
    {
        //Check if players hands are empty and if item on counter is cutable
        if(!player.HasKitchenObject() && HasKitchenObject() && HasRecipeForInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            //Update cutting progress and send out events to UI and Sound systems
            cuttingProgress++;
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = (float) this.cuttingProgress / cuttingRecipeSO.cuttingProgressMax
            });

            OnCut?.Invoke(this, EventArgs.Empty);
            OnAnyCut?.Invoke(this, EventArgs.Empty);

            if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
            {
                //If cutting progress is complete convert to the chopped version of item
                KitchenObjectSO slicedKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());

                GetKitchenObject().DestroySelf();

                KitchenObject.SpawnKitchenObject(slicedKitchenObjectSO, this);
            }
        }
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        //Return cut item if cutable or return null
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);

        if (cuttingRecipeSO != null)
        {
            return cuttingRecipeSO.output;
        }

        return null;
    }

    private bool HasRecipeForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        //Return if item is cutable
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        
        return cuttingRecipeSO != null;
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        //Finds the relevant cutting recipe SO for a given cutable input or returns null
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray)
        {
            if (cuttingRecipeSO.input == inputKitchenObjectSO)
            {
                return cuttingRecipeSO;
            }
        }

        return null;
    }
}
