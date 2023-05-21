using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;

    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer;
    private readonly float spawnRecipeTimerMax = 4f;
    private readonly int waitingRecipesMax = 5;
    private int successfulRecipeCount;

    private void Awake()
    {
        Instance = this;

        waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Start()
    {
        //Initialize spawn recipe timer
        spawnRecipeTimer = spawnRecipeTimerMax;
    }

    private void Update()
    {
        //Only the server should generate recipes and then share them out to the clients
        if (!IsServer)
        {
            return;
        }

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
        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;
            //Pick a random recipe index from the list of all possible RecipeSOs and pass that index to the relevant ClientRpc

            int newRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);

            SpawnNewWaitingRecipeClientRpc(newRecipeSOIndex);
            
        }
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int newWaitingRecipeSOIndex)
    {
        //Using the passedc index pull a specific Recipe, add it to the waitingRecipeSO list, and update all systems and clients
        RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[newWaitingRecipeSOIndex];

        waitingRecipeSOList.Add(waitingRecipeSO);

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
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
                DeliverCorrectRecipeServerRpc(i);                

                return;
            }
        }
        //Player delivered the wrong recipe
        DeliverIncorrectRecipeServerRpc();
    }

    //Called to inform the server that the wrong recipe has been delivered
    [ServerRpc(RequireOwnership = false)]
    private void DeliverIncorrectRecipeServerRpc()
    {
        //Inform all the clients
        DeliverIncorrectRecipeClientRpc();
    }

    //Update all the systems on each client that the delivered order failed
    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    //Called to inform the server that the correct recipe has been delivered and what index in the waitRecipeList it is at
    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int correctRecipeSOListIndex)
    {
        //Inform all the clients that the recipe at the passed index was successfully deliverd
        DeliverCorrectRecipeClientRpc(correctRecipeSOListIndex);         
    }

    //Update all the systems on each client that the delivered order was successful
    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int correctRecipeSOListIndex)
    {
        successfulRecipeCount++;
        waitingRecipeSOList.RemoveAt(correctRecipeSOListIndex);

        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
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
