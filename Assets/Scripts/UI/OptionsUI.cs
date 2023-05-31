using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance { get; private set; }

    public event EventHandler<OnSliderChangedEventArgs> OnMusicSliderChanged;
    public event EventHandler<OnSliderChangedEventArgs> OnSoundEffectsSliderChanged;
    public class OnSliderChangedEventArgs : EventArgs
    {
        public float sliderValue;
    }

    [SerializeField] private Button backButton;
    
    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button keyboardInteractButton;
    [SerializeField] private Button keyboardInteractAlternateButton;
    [SerializeField] private Button keyboardPauseButton;
    [SerializeField] private Button gamepadInteractButton;
    [SerializeField] private Button gamepadInteractAlternateButton;
    [SerializeField] private Button gamepadPauseButton;

    [SerializeField] private Transform PressToRebindKeyTransform;

    [SerializeField] private Slider soundEffectsVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    private TextMeshProUGUI soundEffectsText;
    private TextMeshProUGUI musicText;

    private TextMeshProUGUI moveUpButtonText;
    private TextMeshProUGUI moveDownButtonText;
    private TextMeshProUGUI moveLeftButtonText;
    private TextMeshProUGUI moveRightButtonText;
    private TextMeshProUGUI keyboardInteractButtonText;
    private TextMeshProUGUI keyboardInteractAlternateButtonText;
    private TextMeshProUGUI keyboardPauseButtonText;
    private TextMeshProUGUI gamepadInteractButtonText;
    private TextMeshProUGUI gamepadInteractAlternateButtonText;
    private TextMeshProUGUI gamepadPauseButtonText;

    private Action onCloseButtonAction;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Options UI instance has already been created.");
        }
        Instance = this;

        soundEffectsText = soundEffectsVolumeSlider.GetComponentInChildren<TextMeshProUGUI>();
        musicText = musicVolumeSlider.GetComponentInChildren<TextMeshProUGUI>();

        moveUpButtonText = moveUpButton.GetComponentInChildren<TextMeshProUGUI>();
        moveDownButtonText = moveDownButton.GetComponentInChildren<TextMeshProUGUI>();
        moveLeftButtonText = moveLeftButton.GetComponentInChildren<TextMeshProUGUI>();
        moveRightButtonText = moveRightButton.GetComponentInChildren<TextMeshProUGUI>();
        keyboardInteractButtonText = keyboardInteractButton.GetComponentInChildren<TextMeshProUGUI>();
        keyboardInteractAlternateButtonText = keyboardInteractAlternateButton.GetComponentInChildren<TextMeshProUGUI>();
        keyboardPauseButtonText = keyboardPauseButton.GetComponentInChildren<TextMeshProUGUI>();
        gamepadInteractButtonText = gamepadInteractButton.GetComponentInChildren<TextMeshProUGUI>();
        gamepadInteractAlternateButtonText = gamepadInteractAlternateButton.GetComponentInChildren<TextMeshProUGUI>();
        gamepadPauseButtonText = gamepadPauseButton.GetComponentInChildren<TextMeshProUGUI>();

        backButton.onClick.AddListener(() => 
        { 
            Hide();
            onCloseButtonAction();
        });

        moveUpButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.MoveUp); });
        moveDownButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.MoveDown); });
        moveLeftButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.MoveLeft); });
        moveRightButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.MoveRight); });
        keyboardInteractButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Keyboard_Interact); });
        keyboardInteractAlternateButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Keyboard_InteractAlternate); });
        keyboardPauseButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Keyboard_Pause); });
        gamepadInteractButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Gamepad_Interact); });
        gamepadInteractAlternateButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Gamepad_InteractAlternate); });
        gamepadPauseButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Gamepad_Pause); });

        musicVolumeSlider.onValueChanged.AddListener((float sliderValue) => 
        {
            UpdateMusicVolume(sliderValue);
            UpdateVisual();
        });

        soundEffectsVolumeSlider.onValueChanged.AddListener((float sliderValue) => 
        {
            UpdateSoundEffectsVolume(sliderValue);
            UpdateVisual();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnLocalGameUnpaused += GameManager_OnGameUnpaused;

        UpdateVisual();

        Hide();
    }

    //If the game is unpaused while options is open then the options menu should be closed
    private void GameManager_OnGameUnpaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show(Action onCloseButtonAction)
    {
        this.onCloseButtonAction = onCloseButtonAction;

        gameObject.SetActive(true);

        //Default the selected button to resume on pause
        moveUpButton.Select();
    }

    //Update the visual for all the options to reflect current settings
    private void UpdateVisual()
    {
        soundEffectsText.text = "Sound Effects: " + Mathf.Round(SoundManager.Instance.GetVolume() * SoundManager.SoundEffectsVolumeLevelMax).ToString();
        soundEffectsVolumeSlider.value = SoundManager.Instance.GetVolume() * SoundManager.SoundEffectsVolumeLevelMax;

        musicText.text = "Music: " + Mathf.Round(MusicManager.Instance.GetVolume() * MusicManager.MusicVolumeLevelMax).ToString();
        musicVolumeSlider.value = MusicManager.Instance.GetVolume() * MusicManager.MusicVolumeLevelMax;

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

    private void HidePressToRebindKey()
    {
        PressToRebindKeyTransform.gameObject.SetActive(false);
    }

    public void ShowPressToRebindKey()
    {
        PressToRebindKeyTransform.gameObject.SetActive(true);
    }

    private void RebindBinding(GameInput.Binding binding)
    {
        ShowPressToRebindKey();
        GameInput.Instance.SetKeyBinding(binding, () =>
        {
            HidePressToRebindKey();
            UpdateVisual();
        });
    }

    private void UpdateMusicVolume(float newSliderValue)
    {
        OnMusicSliderChanged?.Invoke(this, new OnSliderChangedEventArgs
        {
            sliderValue = newSliderValue
        });
    }
    
    private void UpdateSoundEffectsVolume(float newSliderValue)
    {
        OnSoundEffectsSliderChanged?.Invoke(this, new OnSliderChangedEventArgs
        {
            sliderValue = newSliderValue
        });
    }
}
