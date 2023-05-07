using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    public static Player Instance { get; private set; }

    public event EventHandler OnItemPickUp;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;

    private float playerHeight = 2f;
    private float playerRadius = .65f;
    private float interactDistance = 2f;
    private bool isWalking = false;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Instance has already been created.");
        }
        Instance = this;
    }

    private void Start()
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    } 

    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void HandleInteractions()
    {
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
        //Pull movement direction from input and calculate how far the player has moved this tick
        Vector3 moveDirection = ConvertInputToNormalizedVector3();
        float moveDistance = moveSpeed * Time.deltaTime;

        //Raycast to check if something is blocking the player
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirection, moveDistance);
        if (!canMove)
        {

            Vector3 moveDirectionX = new Vector3(moveDirection.x, 0, 0).normalized;
            canMove = (moveDirection.x != 0) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirectionX, moveDistance);

            if (canMove)
            {
                moveDirection = moveDirectionX;
            }
            else
            {
                Vector3 moveDirectionZ = new Vector3(0, 0, moveDirection.z).normalized;
                canMove = (moveDirection.z != 0) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirectionZ, moveDistance);

                if (canMove)
                {
                    moveDirection = moveDirectionZ;
                }
            }
        }
        if (canMove)
        {
            transform.position += moveDirection * moveDistance;
        }

        isWalking = moveDirection != Vector3.zero ? true : false;

        if (moveDirection != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotationSpeed);
        }
    }

    //Return movement direction as a normalized Vector3
    private Vector3 ConvertInputToNormalizedVector3()
    {
        Vector2 inputVectorNormalized = gameInput.GetMovementVectorNormalized();

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
            OnItemPickUp?.Invoke(this, EventArgs.Empty);
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
}
