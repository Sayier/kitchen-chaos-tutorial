using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStartCountdownUI : MonoBehaviour
{
    private const string NumberPopup = "NumberPopup";

    private TextMeshProUGUI countdownText;
    private Animator animator;
    private int currentCountdownNumber = 0;

    private void Awake()
    {
        countdownText = GetComponentInChildren<TextMeshProUGUI>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;

        Hide();
    }

    private void Update()
    {
        int countdownNumber = Mathf.CeilToInt(GameManager.Instance.GetCountdownToStartTimer());
        countdownText.text = countdownNumber.ToString();

        if (countdownNumber != currentCountdownNumber)
        {
            currentCountdownNumber = countdownNumber;
            animator.SetTrigger(NumberPopup);
            SoundManager.Instance.PlayCountdownSound();
        }
    }

    private void GameManager_OnGameStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsCountdownToStartActive())
        {
            Show();
        }
        else
        {
            Hide();
        }
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
