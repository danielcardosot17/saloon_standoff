using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum BattleState { COUNTDOWN, PLAYERACTION, RESULT, END}
public enum BattleMode { DEFAULT }
public enum CageState { NORMAL, BROKEN }
public class BattleSystem : MonoBehaviour
{
    [SerializeField] private CountdownTimer countDownTimer;
    [SerializeField] private float resultsTime;
    [SerializeField] private int maxBulletCount;
    [SerializeField] private GameObject victoryCanvas;
    [SerializeField] private TMP_Text winnerText;
    public int MaxBulletCount { get => maxBulletCount; private set => maxBulletCount = value; }

    private PlayerController winner;
    private List<PlayerController> players;
    public List<PlayerController> Players { get => players; private set => players = value; }
    private BattleState battleState;
    public BattleState BattleState { get => battleState; private set => battleState = value; }
    private BattleMode battleMode = BattleMode.DEFAULT;
    public BattleMode BattleMode { get => battleMode; private set => battleMode = value; }

    private List<PlayerController> idlePlayers;
    private List<PlayerController> shootPlayers;
    private List<PlayerController> dodgePlayers;
    private List<PlayerController> loadPlayers;
    private List<PlayerController> targetPlayers;

    public static BattleSystem Instance;
    void Awake()
    {
        if(Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        players = new List<PlayerController>(); 
        if(BattleSystem.Instance.BattleMode == BattleMode.DEFAULT)
        {
            MaxBulletCount = 1;
        }
        ResetTurn();
    }

    private void ResetTurn()
    {
        countDownTimer.ResetCountdown();
        BattleState = BattleState.COUNTDOWN;
    }

    private void ChangeStateTo(BattleState newState)
    {
        BattleState = newState;
    }

    void Update()
    {
        print(battleState);
        switch(battleState)
        {
            case BattleState.COUNTDOWN:
            {
                StartCountDown();
                break;
            }
            case BattleState.PLAYERACTION:
            {
                // populate all those action lists
                GetPlayersActions();
                break;
            }
            case BattleState.RESULT:
            {
                // compare all the action lists
                CalculateResults();
                break;
            }
            case BattleState.END:
            {
                EndGame();
                break;
            }
            default: break;
        }
    }

    private void EndGame()
    {
        winnerText.text = winner.NickName;
        victoryCanvas.SetActive(true);
    }

    private void VictoryCondition()
    {
        // throw new NotImplementedException();
    }

    private void CalculateResults()
    {
        // throw new NotImplementedException();
    }

    private void GetPlayersActions()
    {
        // throw new NotImplementedException();
    }

    private void StartCountDown()
    {
        if(!countDownTimer.IsCounting)
        {
            if(!countDownTimer.FinishedCounting)
            {
                countDownTimer.StartCountdown();
            }
            else
            {
                ChangeStateTo(BattleState.PLAYERACTION);
                countDownTimer.ResetCountdown();
            }
        }
    }

    private void ClearAllActionLists()
    {
        idlePlayers.Clear();
        shootPlayers.Clear();
        dodgePlayers.Clear();
        loadPlayers.Clear();
        targetPlayers.Clear();
    }

}
