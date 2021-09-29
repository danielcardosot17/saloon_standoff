using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum BattleState { COUNTDOWN, PLAYERACTION, RESULT, WIN, LOSE}
public enum BattleMode { DEFAULT }
public enum PlayerState { ALIVE, DEAD }
public enum PlayerActions { IDLE, LOAD, SHOOT, DODGE }
public enum PlayerPosition { INITIAL, MIDLE, FINAL }
public enum CageState { NORMAL, BROKEN }
public class BattleSystem : MonoBehaviour
{
    [SerializeField] private CountdownTimer countDownTimer;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private int maxBulletCount;
    public int MaxBulletCount { get => maxBulletCount; private set => maxBulletCount = value; }
    private List<PlayerController> players;
    public List<PlayerController> Players { get => players; private set => players = value; }
    private BattleState battleState;
    public BattleState BattleState { get => battleState; private set => battleState = value; }
    private BattleMode battleMode = BattleMode.DEFAULT;
    public BattleMode BattleMode { get => battleMode; private set => battleMode = value; }

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
        
    }
}
