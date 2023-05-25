using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public class OnStateChangedEventArgs : EventArgs
    {
        public State cookingState;
    }

    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned
    }

    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private NetworkVariable<State> state = new(State.Idle);
    private NetworkVariable<float> fryingTimer = new(0f);
    private NetworkVariable<float> burningTimer = new(0f);
    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;

    private readonly float defaultFryingTimerMax = 1f;
    private readonly float defaultBurningTimerMax = 1f;

    public override void OnNetworkSpawn()
    {
        fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    }

    private void FryingTimer_OnValueChanged(float previousValue, float newValue)
    {
        //In case there is latency and the FryingRecipeSO has not yet been set, default the maximum timer if needed
        float fryingTimerMax = fryingRecipeSO != null ? fryingRecipeSO.fryingTimerMax : defaultFryingTimerMax;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = fryingTimer.Value / fryingTimerMax
        });
    }

    private void BurningTimer_OnValueChanged(float previousValue, float newValue)
    {
        //In case there is latency and the BurningRecipeSO has not yet been set, default the maximum timer if needed
        float burningTimerMax = burningRecipeSO != null ? burningRecipeSO.burnigTimerMax : defaultBurningTimerMax;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = burningTimer.Value / burningTimerMax
        });
    }

    private void State_OnValueChanged(State previousState, State newState)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs()
        {
            cookingState = newState
        });

        if (state.Value == State.Burned || state.Value == State.Idle)
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = 0f
            });
        }
    }

    private void Update()
    {
        //Update logic will all be handled by the server
        if (!IsServer)
        {
            return;
        }

        //Check if the is a Kitchen Object on the Stove
        if (HasKitchenObject())
        {
            switch (state.Value)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer.Value += Time.deltaTime;

                    if (fryingTimer.Value >= fryingRecipeSO.fryingTimerMax)
                    {
                        //Once the timer has reached the cooking time defined in the FryingRecipeSO destroy what is on stove
                        //and replace it with the output defined in the FryingRecipeSO
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                        //Grab the BurningRecipeSO for the object, initialize burning timer, advance the Stove state to Fried
                        SetBurningRecipeSOClientRpc();
                        burningTimer.Value = 0f;
                        state.Value = State.Fried;
                    }
                    break;
                case State.Fried:
                    //Burning timer was initialized at the end of the previous state
                    burningTimer.Value += Time.deltaTime;

                    if (burningTimer.Value >= burningRecipeSO.burnigTimerMax)
                    {
                        //Once the timer has reached the burn time defined in the BurningRecipeSO destroy what is on stove
                        //and replace it with the output defined in the BurningRecipeSO
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                        //Update Stove state to Burned
                        state.Value = State.Burned;
                    }
                    break;
                case State.Burned:
                    break;
            }
        }
    }

    public override void Interact(Player player)
    {
        //Player is holding something and it is fryable and Stove is empty
        if (player.HasKitchenObject() && HasRecipeForInput(player.GetKitchenObject().GetKitchenObjectSO()) && !HasKitchenObject())
        {
            //Place the object on the Stove and retrieve the associated FryingRecipeSO
            KitchenObject kitchenObject = player.GetKitchenObject();
            kitchenObject.SetKitchenObjectParent(this);

            //Request to server to retrieve the cooking recipe, reset cooking timer, and update state to Frying
            InteractLogicPlaceOnCounterServerRpc();
        }
        //Player is not holding anything and Stove is occupied
        else if (!player.HasKitchenObject() && this.HasKitchenObject())
        {
            //Transfer object from Stove to Player and null out the FryingRecipeSO
            this.GetKitchenObject().SetKitchenObjectParent(player);
            fryingRecipeSO = null;

            //Request the server to update the cooking state to Idle
            SetStateIdleServerRpc();
        }
        //Player is holding a plate and counter has a Kitchen Object on it
        else if (player.HasKitchenObject() && player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject) && HasKitchenObject())
        {
            //Check if the object on the Stove can be placed on a Plate
            bool hasAddedKitchenObjectToPlate = plateKitchenObject.TryAddIngrediant(GetKitchenObject().GetKitchenObjectSO());
            if (hasAddedKitchenObjectToPlate)
            {
                //KitchenObject can be added to plate, at this point it has already been added to the plate by the TryAddIngrediant call
                //so we just need to destroy the version of the object on the Stove, set Stove state to Idle, and update events
                KitchenObject.DestroyKitchenObject(GetKitchenObject());

                SetStateIdleServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        state.Value = State.Idle;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceOnCounterServerRpc()
    {
        //Initialize the frying timer to 0, update state to Frying
        //and broadcast that all clients need to update the FryingRecipeSO
        fryingTimer.Value = 0f;
        state.Value = State.Frying;

        SetFryingRecipeSOClientRpc();
    }

    [ClientRpc]
    private void SetFryingRecipeSOClientRpc()
    {
        fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
    }
    
    [ClientRpc]
    private void SetBurningRecipeSOClientRpc()
    {
        burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);

        if (fryingRecipeSO != null)
        {
            return fryingRecipeSO.output;
        }

        return null;
    }

    private bool HasRecipeForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);

        return fryingRecipeSO != null;
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray)
        {
            if (fryingRecipeSO.input == inputKitchenObjectSO)
            {
                return fryingRecipeSO;
            }
        }

        return null;
    }

    //Return what the BurningRecipeSO based on a provided input KitchenObjectSO
    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == inputKitchenObjectSO)
            {
                return burningRecipeSO;
            }
        }

        return null;
    }

    //Check is the object on the stove is currently in a Fried state
    public bool IsFried()
    {
        return state.Value == State.Fried;
    }
}
