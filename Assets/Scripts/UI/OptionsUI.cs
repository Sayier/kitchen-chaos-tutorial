using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance { get; private set; }

    [SerializeField] private Button soundEffectsButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button backButton;

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
    }

    private void Start()
    {
        GameManager.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;

        UpdateVisual();

        Hide();
    }

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

    private void UpdateVisual()
    {
        soundEffectsText.text = "Sound Effects: " + Mathf.Round(SoundManager.Instance.GetVolume() * 10f).ToString();
        musicText.text = "Music: " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f).ToString();
    }
}
