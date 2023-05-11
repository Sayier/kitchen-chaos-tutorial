using System;
using System.Collections;
using System.Collections.Generic;
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

    private State state;
    private float fryingTimer;
    private float burningTimer;
    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;

    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        //Check if the is a Kitchen Object on the Stove
        if (HasKitchenObject())
        {
            switch (state)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    //Frying timer is initialized when object is placed on Stove, update Events each tick
                    fryingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                    });

                    if (fryingTimer >= fryingRecipeSO.fryingTimerMax)
                    {
                        //Once the timer has reached the cooking time defined in the FryingRecipeSO destroy what is on stove
                        //and replace it with the output defined in the FryingRecipeSO
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                        //Grab the BurningRecipeSO for the object, advance the Stove state, and notify Events
                        burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                        burningTimer = 0f;
                        state = State.Fried;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs()
                        {
                            cookingState = state
                        });

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = burningTimer / burningRecipeSO.burnigTimerMax
                        });
                    }
                    break;
                case State.Fried:
                    //Burning timer was initialized at the end of the previous state, update events each tick
                    burningTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = burningTimer / burningRecipeSO.burnigTimerMax
                    });

                    if (burningTimer >= burningRecipeSO.burnigTimerMax)
                    {
                        //Once the timer has reached the burn time defined in the BurningRecipeSO destroy what is on stove
                        //and replace it with the output defined in the BurningRecipeSO
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                        //Update Stove state and in the OnProgressChanged event reset the progressNormalized now that
                        //the timers are complete
                        state = State.Burned;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs()
                        {
                            cookingState = state
                        });

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = 0f
                        });
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
            player.GetKitchenObject().SetKitchenObjectParent(this);

            fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

            //Initialize the cooking timer, update the Stove state machine, and trigger relevant events
            fryingTimer = 0f;
            state = State.Frying;

            OnStateChanged?.Invoke(this, new OnStateChangedEventArgs()
            {
                cookingState = state
            });

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
            });
        }
        //Player is not holding anything and Stove is occupied
        else if (!player.HasKitchenObject() && this.HasKitchenObject())
        {
            //Transfer object from Stove to Player and null out the FryingRecipeSO
            this.GetKitchenObject().SetKitchenObjectParent(player);
            fryingRecipeSO = null;

            //Update Stove state and trigger relevant events
            state = State.Idle;

            OnStateChanged?.Invoke(this, new OnStateChangedEventArgs()
            {
                cookingState = state
            });

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = 0f
            });
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
                GetKitchenObject().DestroySelf();

                state = State.Idle;

                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs()
                {
                    cookingState = state
                });

                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    progressNormalized = 0f
                });
            }
        }
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
        return state == State.Fried;
    }
}
