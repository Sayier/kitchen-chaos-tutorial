using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance { get; private set; }

    [SerializeField] private Button backButton;
    [SerializeField] private Button soundEffectsButton;
    [SerializeField] private Button musicButton;
    
    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button interactButton;
    [SerializeField] private Button interactAlternateButton;
    [SerializeField] private Button pauseButton;

    [SerializeField] private Transform PressToRebindKeyTransform;

    private TextMeshProUGUI moveUpButtonText;
    private TextMeshProUGUI moveDownButtonText;
    private TextMeshProUGUI moveLeftButtonText;
    private TextMeshProUGUI moveRightButtonText;
    private TextMeshProUGUI interactButtonText;
    private TextMeshProUGUI interactAlternateButtonText;
    private TextMeshProUGUI pauseButtonText;

    private TextMeshProUGUI soundEffectsText;
    private TextMeshProUGUI musicText;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Options UI instance has already been created.");
        }
        Instance = this;

        soundEffectsText = soundEffectsButton.GetComponentInChildren<TextMeshProUGUI>();
        musicText = musicButton.GetComponentInChildren<TextMeshProUGUI>();

        moveUpButtonText = moveUpButton.GetComponentInChildren<TextMeshProUGUI>();
        moveDownButtonText = moveDownButton.GetComponentInChildren<TextMeshProUGUI>();
        moveLeftButtonText = moveLeftButton.GetComponentInChildren<TextMeshProUGUI>();
        moveRightButtonText = moveRightButton.GetComponentInChildren<TextMeshProUGUI>();
        interactButtonText = interactButton.GetComponentInChildren<TextMeshProUGUI>();
        interactAlternateButtonText = interactAlternateButton.GetComponentInChildren<TextMeshProUGUI>();
        pauseButtonText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();

        soundEffectsButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.ChangeVolume();
            UpdateVisual();
        });

        musicButton.onClick.AddListener(() => 
        {
            MusicManager.Instance.ChangeVolume();
            UpdateVisual();
        });

        backButton.onClick.AddListener(() => 
        {
            Hide();
        });

        moveUpButton.onClick.AddListener(() => 
        {
            RebindBinding(GameInput.Binding.MoveUp);
        });

        moveDownButton.onClick.AddListener(() => 
        {
            RebindBinding(GameInput.Binding.MoveDown);
        });

        moveLeftButton.onClick.AddListener(() => 
        {
            RebindBinding(GameInput.Binding.MoveLeft);
        });

        moveRightButton.onClick.AddListener(() => 
        {
            RebindBinding(GameInput.Binding.MoveRight);
        });

        interactButton.onClick.AddListener(() => 
        {
            RebindBinding(GameInput.Binding.Interact);
        });

        interactAlternateButton.onClick.AddListener(() => 
        {
            RebindBinding(GameInput.Binding.InteractAlternate);
        });

        pauseButton.onClick.AddListener(() => 
        {
            RebindBinding(GameInput.Binding.Pause);
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;

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

    public void Show()
    {
        gameObject.SetActive(true);
    }

    //Update the visual for all the options to reflect current settings
    private void UpdateVisual()
    {
        soundEffectsText.text = "Sound Effects: " + Mathf.Round(SoundManager.Instance.GetVolume() * 10f).ToString();
        musicText.text = "Music: " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f).ToString();

        moveUpButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveUp);
        moveDownButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveDown);
        moveLeftButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveLeft);
        moveRightButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveRight);
        interactButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
        interactAlternateButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlternate);
        pauseButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause);
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
}
