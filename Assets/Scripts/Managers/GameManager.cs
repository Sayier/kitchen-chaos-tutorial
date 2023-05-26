using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnGameStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    public event EventHandler OnLocalPlayerReady;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }

    private Dictionary<ulong, bool> playerReadyDictionary;

    private NetworkVariable<State> state = new(State.WaitingToStart);
    private NetworkVariable<float> countdownToStartTimer = new(3f);
    private NetworkVariable<float> gamePlayingTimer = new(0f);
    private readonly float gamePlayingTimerMax = 300f; 
    private bool isGamePaused = false;
    private bool isLocalPlayerReady;
    

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("GameManager instance has already been created.");
        }
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnGameStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        switch (state.Value)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer.Value -= Time.deltaTime;
                if (countdownToStartTimer.Value <= 0f)
                {
                    gamePlayingTimer.Value = gamePlayingTimerMax;
                    ChangeState(State.GamePlaying);
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer.Value -= Time.deltaTime;
                if (gamePlayingTimer.Value <= 0f)
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
        if (state.Value == State.WaitingToStart)
        {
            isLocalPlayerReady = true;

            OnLocalPlayerReady?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        playerReadyDictionary[senderClientId] = true;

        bool allClientsReady = true;
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
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
        if(state.Value == newState)
        {
            return;
        }
        
        state.Value = newState;
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    //Return if the active game play state is active
    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
    }

    //Return if the countdown to start is active
    public bool IsCountdownToStartActive()
    {
        return state.Value == State.CountdownToStart;
    }

    //Return if the waiting to start is active
    public bool IsWaitingToStartActive()
    {
        return state.Value == State.WaitingToStart;
    }

    //Return if the game is over
    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }

    //Return the countdown to start timer
    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer.Value;
    }

    //Return the inverted normalized playtime remaining 
    public float GetGameplayingTimerNormalized()
    {
        return 1 - (gamePlayingTimer.Value / gamePlayingTimerMax); ;
    }

    public bool GetIsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }
}
