using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;

    private const string PlayerPrefsBindings = "InputBindings";

    public enum Binding
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Interact,
        InteractAlternate,
        Pause
    }

    private PlayerInputActions playerInputActions;    

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("GameInput instance has already been created.");
        }
        Instance = this;

        playerInputActions = new PlayerInputActions();

        //Load key rebinds if they have previously been set
        if (PlayerPrefs.HasKey(PlayerPrefsBindings))
        {
            playerInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PlayerPrefsBindings));
        }

        playerInputActions.Player.Enable();

        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.InteractAlternate.performed += InteractAlternate_performed;
        playerInputActions.Player.Pause.performed += Pause_performed;
    }

    //Clean up the game inputs when leaving scene
    private void OnDestroy()
    {
        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.InteractAlternate.performed -= InteractAlternate_performed;
        playerInputActions.Player.Pause.performed -= Pause_performed;

        playerInputActions.Dispose();
    }

    private void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlternate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    //Return the normalized vector2 of the movement input
    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }

    public string GetBindingText(Binding binding)
    {
        string bindingText = "";

        switch (binding)
        {
            case Binding.MoveUp:
                bindingText = playerInputActions.Player.Move.bindings[1].ToDisplayString();
                break;
            case Binding.MoveDown:
                bindingText = playerInputActions.Player.Move.bindings[2].ToDisplayString();
                break;
            case Binding.MoveLeft:
                bindingText = playerInputActions.Player.Move.bindings[3].ToDisplayString();
                break;
            case Binding.MoveRight:
                bindingText = playerInputActions.Player.Move.bindings[4].ToDisplayString();
                break;
            case Binding.Interact:
                bindingText = playerInputActions.Player.Interact.bindings[0].ToDisplayString();
                break;
            case Binding.InteractAlternate:
                bindingText = playerInputActions.Player.InteractAlternate.bindings[0].ToDisplayString();
                break;
            case Binding.Pause:
                bindingText = playerInputActions.Player.Pause.bindings[0].ToDisplayString();
                break;
        }

        return bindingText;
    }

    public void SetKeyBinding(Binding binding, Action onActionRebound)
    {
        playerInputActions.Player.Disable();

        InputAction inputAction;
        int bindingIndex;

        switch (binding)
        {
            case Binding.MoveUp:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 1;
                break;
            case Binding.MoveDown:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 2;
                break;
            case Binding.MoveLeft:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 3;
                break;
            case Binding.MoveRight:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 4;
                break;
            case Binding.Interact:
                inputAction = playerInputActions.Player.Interact;
                bindingIndex = 0;
                break;
            case Binding.InteractAlternate:
                inputAction = playerInputActions.Player.InteractAlternate;
                bindingIndex = 0;
                break;
            case Binding.Pause:
                inputAction = playerInputActions.Player.Pause;
                bindingIndex = 0;
                break;
            default:
                Debug.LogError("Invalid key binding attempting to be set");
                bindingIndex = 0;
                inputAction = null;
                break;
        }

        inputAction.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(callback =>
            {
                callback.Dispose();
                playerInputActions.Player.Enable();
                onActionRebound();

                PlayerPrefs.SetString(PlayerPrefsBindings, playerInputActions.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();
            })
            .Start();
    }
}
