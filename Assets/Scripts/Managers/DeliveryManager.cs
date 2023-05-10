using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;

    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimerMax = 4f;
    private readonly int waitingRecipesMax = 5;
    private int successfulRecipeCount;

    private void Awake()
    {
        Instance = this;

        waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Update()
    {
        //If the game is not in the Game Playing state then orders should not be generating
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        //Check if the maximum amount of orders have been created, if so stop spawning
        if (waitingRecipeSOList.Count >= waitingRecipesMax)
        {
            return;
        }

        //Count down spawn timer and then check if it is time to spawn the next order
        spawnRecipeTimerMax -= Time.deltaTime;
        if (spawnRecipeTimerMax <= 0)
        {
            spawnRecipeTimerMax = 4f;
            RecipeSO newRecipeOrder = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];
            waitingRecipeSOList.Add(newRecipeOrder);

            OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
        }
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        //Loop through each recipe current ordered
        for(int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            //If the waiting recipe has a different count of ingredients then what is on plate then they can not be the same
            if(waitingRecipeSO.kitchenObjectSOList.Count != plateKitchenObject.GetKitchenObjectSOOnPlateList().Count)
            {
                continue;
            }

            bool plateContentsMatchRecipe = true;

            //Loop through each ingredient in the order and compare it to each ingredient on the plate
            foreach(KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
            {
                //Loop through each item on the plate and see if it
                bool ingredientFound = false;
                foreach(KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOOnPlateList())
                {
                    //Matching ingredient found on plate
                    if (plateKitchenObjectSO == recipeKitchenObjectSO)
                    {
                        //If match found no need to keep searching
                        ingredientFound = true;
                        break;
                    }
                }

                //Matching ingredient not found on plate
                if (!ingredientFound)
                {
                    //If match not found no need to check remaining ingredients
                    plateContentsMatchRecipe = false;
                    break;
                }
            }

            //Player delivered the correct recipe
            if (plateContentsMatchRecipe)
            {
                successfulRecipeCount++;
                waitingRecipeSOList.RemoveAt(i);

                OnRecipeCompleted?.Invoke(this, EventArgs.Empty);

                return;
            }
        }
        //Player delivered the wrong recipe
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    //Return a list of every ordered recipe
    public List<RecipeSO> GetWaitingRecipeSOList()
    {    
        return waitingRecipeSOList;
    }

    //Return a count of how many orders were completed successfully
    public int GetSuccessfulRecipeCount()
    {
        return successfulRecipeCount;
    }
}
