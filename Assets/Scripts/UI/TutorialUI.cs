using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moveUpButtonText;
    [SerializeField] private TextMeshProUGUI moveDownButtonText;
    [SerializeField] private TextMeshProUGUI moveLeftButtonText;
    [SerializeField] private TextMeshProUGUI moveRightButtonText;
    [SerializeField] private TextMeshProUGUI keyboardInteractButtonText;
    [SerializeField] private TextMeshProUGUI keyboardInteractAlternateButtonText;
    [SerializeField] private TextMeshProUGUI keyboardPauseButtonText;
    [SerializeField] private TextMeshProUGUI gamepadInteractButtonText;
    [SerializeField] private TextMeshProUGUI gamepadInteractAlternateButtonText;
    [SerializeField] private TextMeshProUGUI gamepadPauseButtonText;

    private void Start()
    {
        GameInput.Instance.OnRebind += GameInput_OnRebind;
        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;

        UpdateVisual();

        Show();
    }

    private void GameManager_OnGameStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsCountdownToStartActive())
        {
            Hide();
        }
    }

    private void GameInput_OnRebind(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        moveUpButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveUp);
        moveDownButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveDown);
        moveLeftButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveLeft);
        moveRightButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveRight);
        keyboardInteractButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Keyboard_Interact);
        keyboardInteractAlternateButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Keyboard_InteractAlternate);
        keyboardPauseButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Keyboard_Pause);
        gamepadInteractButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_Interact);
        gamepadInteractAlternateButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_InteractAlternate);
        gamepadPauseButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_Pause);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
}
