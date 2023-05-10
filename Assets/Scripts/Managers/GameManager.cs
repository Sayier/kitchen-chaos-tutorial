using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnGameStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }

    private float countdownToStartTimer = 3f;
    private float gamePlayingTimer;
    private readonly float gamePlayingTimerMax = 30f; 
    private bool isGamePaused = false;
    private State state;
    

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("GameManager instance has already been created.");
        }
        Instance = this;

        state = State.WaitingToStart;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer <= 0f)
                {
                    gamePlayingTimer = gamePlayingTimerMax;
                    ChangeState(State.GamePlaying);
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                if (gamePlayingTimer <= 0f)
                {
                    ChangeState(State.GameOver);
                }
                break;
            case State.GameOver:
                break;
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state == State.WaitingToStart)
        {
            ChangeState(State.CountdownToStart);
        }
    }

    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
        
    }

    //Handles switching state and informing other systems of the state change
    private void ChangeState(State newState)
    {
        if(state == newState)
        {
            return;
        }
        
        state = newState;
        OnGameStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    //Return if the active game play state is active
    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }

    //Return if the countdown to start is active
    public bool IsCountdownToStartActive()
    {
        return state == State.CountdownToStart;
    }

    //Return if the game is over
    public bool IsGameOver()
    {
        return state == State.GameOver;
    }

    //Return the countdown to start timer
    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }

    //Return the inverted normalized playtime remaining 
    public float GetGameplayingTimerNormalized()
    {
        return 1 - (gamePlayingTimer / gamePlayingTimerMax);
    }
}
