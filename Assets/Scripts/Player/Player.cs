using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyItemPickUp;
    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
        OnAnyItemPickUp = null;
    }

    public static Player LocalInstance { get; private set; }

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private LayerMask collisionsLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private List<Vector3> spawnPositionList;
    [SerializeField] private PlayerVisual playerVisual;

    private bool isWalking = false;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        transform.position = spawnPositionList[MultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];
        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId && HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;

        PlayerData playerData = MultiplayerManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(MultiplayerManager.Instance.GetPlayerColor(playerData.colorId));
    } 

    private void Update()
    {
        //Only let the local player update the Player object
        if (!IsOwner) {
            return;
        }
        HandleMovement();
        HandleInteractions();
    }

    //Listen for Alt. Interact input
    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        //If game is not currently in playing state do not listen for player input
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        //If a counter is selected allow an alternate interaction on button press
        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    //Listen for Interact input
    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        //If game is not currently in playing state do not listen for player input
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        //If there is a counter selected allow an interaction on button press
        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void HandleInteractions()
    {
        float interactDistance = 2f;

        //Check if there is something within interact distance of the player
        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit raycastHit, interactDistance, countersLayerMask))
        {
            //Check if object is a counter and try to return which counter it is
            if(raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                if(baseCounter != selectedCounter)
                {
                    //If not already selected store a reference to interactable counter
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                //Object is not a counter, make sure there is no longer a selected counter
                SetSelectedCounter(null);
            }
        }
        else
        {
            //There is nothing in front of player, make sure there is no longer a selected counter
            SetSelectedCounter(null);
        }
    }

    private void SetSelectedCounter(BaseCounter newSelectedCounter)
    {
        //If there was no selected counter and there is still no selected counter skip attempting to update
        if(selectedCounter == null && newSelectedCounter == null)
        {
            return;
        }
        
        //Store reference of new selected counter and fire event to update visuals
        selectedCounter = newSelectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = this.selectedCounter
        });
    }

    private void HandleMovement()
    {
        float playerRadius = .65f;

        //Pull movement direction from input and calculate how far the player has moved this tick
        Vector3 moveDirection = ConvertInputToNormalizedVector3();
        float moveDistance = moveSpeed * Time.deltaTime;

        //Raycast to check if something is blocking the player
        bool canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirection, Quaternion.identity, moveDistance, collisionsLayerMask);
        if (!canMove)
        {
            //If something is blocking, check if player can still move in the X direction of their movement
            Vector3 moveDirectionX = new Vector3(moveDirection.x, 0, 0).normalized;
            canMove = IsMovingOnXAxis(moveDirection) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirectionX, Quaternion.identity, moveDistance, collisionsLayerMask);

            if (canMove)
            {
                //Player is not blocked in the X direction so set a new movement vector to allow that X movement
                moveDirection = moveDirectionX;
            }
            else
            {
                //Player is blocked forward and is blocked in X, check if player can move in the Z direction
                Vector3 moveDirectionZ = new Vector3(0, 0, moveDirection.z).normalized;
                canMove = IsMovingOnZAxis(moveDirection) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirectionZ, Quaternion.identity, moveDistance, collisionsLayerMask);

                if (canMove)
                {
                    //Player is not blocked in the Z direction so set a new movement vector to allow that Z movement
                    moveDirection = moveDirectionZ;
                }
            }
        }
        if (canMove)
        {
            //Move the player based on what ever movement direction was found above
            transform.position += moveDirection * moveDistance;
        }

        //Check if is walking, walking into an obstacle counts here
        isWalking = moveDirection != Vector3.zero;

        if (isWalking)
        {
            //Smoothly rotate the player to face the direction of movement
            transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotationSpeed);
        }
    }

    //Checks for a Z movement component in the movement vector, accounts for deadzone on controllers
    private static bool IsMovingOnZAxis(Vector3 moveDirection)
    {
        float downMovementDeadzone = -.5f;
        float upMovementDeadzone = .5f;

        return moveDirection.z <= downMovementDeadzone || moveDirection.z >= upMovementDeadzone;
    }

    //Checks for a X movement component in the movement vector, accounts for deadzone on controllers
    private static bool IsMovingOnXAxis(Vector3 moveDirection)
    {
        float leftMovementDeadzone = -.5f;
        float rightMovementDeadzone = .5f;

        return moveDirection.x <= leftMovementDeadzone || moveDirection.x >= rightMovementDeadzone;
    }

    //Return movement direction as a normalized Vector3
    private Vector3 ConvertInputToNormalizedVector3()
    {
        Vector2 inputVectorNormalized = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDirection = new(inputVectorNormalized.x, 0, inputVectorNormalized.y);
        return moveDirection;
    }

    //Returns if player is walking
    public bool IsWalking()
    {
        return isWalking;
    }

    //Returns the transform where the player holds an object
    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    //Returns the kitchen object the player is holding
    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    //Put kitchen object in player's hands and inform Sound system
    public void SetKitchenObject(KitchenObject newKitchenObject)
    {
        this.kitchenObject = newKitchenObject;

        if(kitchenObject != null)
        {
            //OnItemPickUp?.Invoke(this, EventArgs.Empty);
            OnAnyItemPickUp?.Invoke(this, EventArgs.Empty);
        }
    }

    //Remove kitchen object from player
    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    //Return if player is holding a kitchen object
    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
