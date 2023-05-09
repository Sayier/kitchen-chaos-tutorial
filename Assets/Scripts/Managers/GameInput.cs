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
        Keyboard_Interact,
        Keyboard_InteractAlternate,
        Keyboard_Pause,
        Gamepad_Interact,
        Gamepad_InteractAlternate,
        Gamepad_Pause

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

    //For a given key binding return a string that can be used to represent the key
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
            case Binding.Keyboard_Interact:
                bindingText = playerInputActions.Player.Interact.bindings[0].ToDisplayString();
                break;
            case Binding.Keyboard_InteractAlternate:
                bindingText = playerInputActions.Player.InteractAlternate.bindings[0].ToDisplayString();
                break;
            case Binding.Keyboard_Pause:
                bindingText = playerInputActions.Player.Pause.bindings[0].ToDisplayString();
                break;
            case Binding.Gamepad_Interact:
                bindingText = playerInputActions.Player.Interact.bindings[1].ToDisplayString();
                break;
            case Binding.Gamepad_InteractAlternate:
                bindingText = playerInputActions.Player.InteractAlternate.bindings[1].ToDisplayString();
                break;
            case Binding.Gamepad_Pause:
                bindingText = playerInputActions.Player.Pause.bindings[1].ToDisplayString();
                break;
        }

        return bindingText;
    }

    //For a given binding, set a new key and then save that key to Player Prefs
    public void SetKeyBinding(Binding binding, Action onActionRebound)
    {
        //Need to disable all input while keybinding is happening
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
            case Binding.Keyboard_Interact:
                inputAction = playerInputActions.Player.Interact;
                bindingIndex = 0;
                break;
            case Binding.Keyboard_InteractAlternate:
                inputAction = playerInputActions.Player.InteractAlternate;
                bindingIndex = 0;
                break;
            case Binding.Keyboard_Pause:
                inputAction = playerInputActions.Player.Pause;
                bindingIndex = 0;
                break;
            case Binding.Gamepad_Interact:
                inputAction = playerInputActions.Player.Interact;
                bindingIndex = 1;
                break;
            case Binding.Gamepad_InteractAlternate:
                inputAction = playerInputActions.Player.InteractAlternate;
                bindingIndex = 1;
                break;
            case Binding.Gamepad_Pause:
                inputAction = playerInputActions.Player.Pause;
                bindingIndex = 1;
                break;
            default:
                Debug.LogError("Invalid key binding attempting to be set");
                bindingIndex = 0;
                inputAction = null;
                break;
        }

        //Perform the rebinding using the inputs looked up above, once complete remove the rebinding callback, re-enable
        //the controls, save to Player Prefs, and fire off whatever Actions need to happen
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
