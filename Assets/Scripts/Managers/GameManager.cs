using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnGameStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnLocalPlayerReady;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }

    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPausedDictionary;

    private NetworkVariable<State> state = new(State.WaitingToStart);
    private NetworkVariable<float> countdownToStartTimer = new(3f);
    private NetworkVariable<float> gamePlayingTimer = new(0f);
    private NetworkVariable<bool> isGamePaused = new(false);

    private readonly float gamePlayingTimerMax = 300f; 
    private bool isLocalGamePaused = false;
    private bool isLocalPlayerReady;
    private bool autoTestGamePausedState;
    

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("GameManager instance has already been created.");
        }
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPausedDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        //If we try to run TestGamePausedState now the player will still be in the process of disconnecting
        //We are going to set a trigger now and then check this trigger in LateUpdate
        //after the player has disconnected where it will be safe to run TestGamePausedState
        autoTestGamePausedState = true;
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

    private void LateUpdate()
    {
        if (autoTestGamePausedState)
        {
            autoTestGamePausedState = false;
            TestGamePausedState();
        }
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if (isGamePaused.Value == true)
        {
            Time.timeScale = 0f;

            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;

            OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnGameStateChanged?.Invoke(this, EventArgs.Empty);
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
        //Set the player that made this call to true in the ready dictionary
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        playerReadyDictionary[senderClientId] = true;

        //Cycle through each client connected to the session and check if all are true
        bool allClientsReady = true;
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                //If any player does not exist in the ready dictionary or are set to false then break and don't update state
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            //All players are ready, move on to CountdownToStart state
            ChangeState(State.CountdownToStart);
        }
    }

    public void TogglePauseGame()
    {
        isLocalGamePaused = !isLocalGamePaused;

        if (isLocalGamePaused)
        {
            //Tell the server this client has paused and update local paused state
            PauseGameServerRpc();

            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            //Tell the server this client has unpaused and update local paused state
            UnpauseGameServerRpc();

            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        //Set the player that made this call to true in the paused dictionary
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        playerPausedDictionary[senderClientId] = true;

        //Check if the global paused state needs to change
        TestGamePausedState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        //Set the player that made this call to false in the paused dictionary
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        playerPausedDictionary[senderClientId] = false;

        //Check if the global paused state needs to change
        TestGamePausedState();
    }

    private void TestGamePausedState()
    {
        //Cycle through each client that is connected to the session check if they are marked a paused (true) in the playerPausedDictionary
        //if anyone is paused update the global paused state
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPausedDictionary.ContainsKey(clientId) && playerPausedDictionary[clientId])
            {
                isGamePaused.Value = true;
                return;
            }
        }
        //No one is paused, make sure the global paused state reflects this
        isGamePaused.Value = false;
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

    public bool IsLocalGamePaused()
    {
        return isLocalGamePaused;
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
