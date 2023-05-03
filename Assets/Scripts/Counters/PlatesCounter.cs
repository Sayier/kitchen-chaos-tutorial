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
        if (!player.HasKitchenObject() && plateSpawnAmount > 0)
        {
            KitchenObject.SpawnKitchenObject(kitchenObjectSO, player);

            plateSpawnAmount--;
            OnPlatePickedUp?.Invoke(this, EventArgs.Empty);
        }
    }
}
