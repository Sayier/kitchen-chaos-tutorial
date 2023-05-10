using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlatePickedUp;

    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    
    private float spawnPlateTimer;
    private const float spawnPlateTimerMax = 4f;
    private int plateSpawnAmount;
    private const int plateSpawnAmountMax = 5;

    private void Update()
    {
        //If the game is not in the Game Playing state then plates should not be spawning
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        //Create a new plate once the spawnPlateTimer has elapsed
        spawnPlateTimer += Time.deltaTime;
        if(spawnPlateTimer > spawnPlateTimerMax)
        {
            spawnPlateTimer = 0f;

            if (plateSpawnAmount < plateSpawnAmountMax)
            {
                plateSpawnAmount++;

                OnPlateSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public override void Interact(Player player)
    {
        //If the player is not holding anything and there are plates on the counter then move one to the player's hands
        if (!player.HasKitchenObject() && plateSpawnAmount > 0)
        {
            KitchenObject.SpawnKitchenObject(kitchenObjectSO, player);

            plateSpawnAmount--;
            OnPlatePickedUp?.Invoke(this, EventArgs.Empty);
        }
    }
}
