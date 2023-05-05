using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;

    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimerMax = 4f;
    private readonly int waitingRecipesMax = 5;

    private void Awake()
    {
        Instance = this;

        waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Update()
    {
        if (waitingRecipeSOList.Count >= waitingRecipesMax)
        {
            return;
        }

        spawnRecipeTimerMax -= Time.deltaTime;
        if (spawnRecipeTimerMax <= 0)
        {
            spawnRecipeTimerMax = 4f;
            RecipeSO newRecipeOrder = recipeListSO.recipeSOList[Random.Range(0, recipeListSO.recipeSOList.Count)];
            waitingRecipeSOList.Add(newRecipeOrder);
        }
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for(int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            //If the waiting recipe has a different count of ingredients then what is on plate then they can not be the same
            if(waitingRecipeSO.kitchenObjectSOList.Count != plateKitchenObject.GetKitchenObjectSOOnPlateList().Count)
            {
                continue;
            }

            bool plateContentsMatchRecipe = true;

            foreach(KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
            {
                bool ingredientFound = false;
                foreach(KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOOnPlateList())
                {
                    if (plateKitchenObjectSO == recipeKitchenObjectSO)
                    {
                        ingredientFound = true;
                        break;
                    }
                }

                if(!ingredientFound)
                {
                    plateContentsMatchRecipe = false;
                    break;
                }
            }

            if (plateContentsMatchRecipe)
            {
                Debug.Log("Player delivered the correct recipe");
                waitingRecipeSOList.RemoveAt(i);
                return;
            }
        }

        Debug.Log("Player delivered the wrong recipe");
    }
}
