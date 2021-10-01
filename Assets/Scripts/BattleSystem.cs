using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum BattleState { START, COUNTDOWN, PLAYERACTION, RESULT, END}
public enum BattleMode { DEFAULT }
public enum CageState { NORMAL, BROKEN }
public class BattleSystem : MonoBehaviour
{
    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private float resultsTime;
    [SerializeField] private int maxBulletCount;
    [SerializeField] private int maxMoveFoward;
    [SerializeField] private GameObject endgameCanvas;
    [SerializeField] private TMP_Text endgameText;
    [SerializeField] private string everybodyIsDeadText;
    public int MaxBulletCount { get => maxBulletCount; private set => maxBulletCount = value; }
    public int MaxMoveFoward { get => maxMoveFoward; private set => maxMoveFoward = value; }

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
    private List<GameObject> targetObjects;
    private CageState cageState;
    public CageState CageState { get => cageState; private set => cageState = value; }

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
        cageState = CageState.NORMAL;
        players = new List<PlayerController>();
        idlePlayers = new List<PlayerController>();
        loadPlayers = new List<PlayerController>();
        shootPlayers = new List<PlayerController>();
        dodgePlayers = new List<PlayerController>();
        targetObjects = new List<GameObject>();
        if(BattleSystem.Instance.BattleMode == BattleMode.DEFAULT)
        {
            MaxBulletCount = 1;
        }
        ResetTurn();
    }

    private void ResetTurn()
    {
        countdownTimer.ResetCountdown();
        BattleState = BattleState.COUNTDOWN;
    }

    private void ChangeStateTo(BattleState newState)
    {
        BattleState = newState;
    }

    void Update()
    {
        switch(battleState)
        {
            case BattleState.START:
            {
                print("START");
                ResetPlayersActions();
                ClearAllActionLists();
                ChangeStateTo(BattleState.COUNTDOWN);
                break;
            }
            case BattleState.COUNTDOWN:
            {
                StartCountdown();
                break;
            }
            case BattleState.PLAYERACTION:
            {
                print("PLAYERACTION");
                // populate all those action lists
                GetPlayersActions();
                ChangeStateTo(BattleState.RESULT);
                break;
            }
            case BattleState.RESULT:
            {
                print("RESULT");
                // compare all the action lists
                // DoActions();
                CalculateResults();
                if(EndgameCondition())
                {
                    ChangeStateTo(BattleState.END);
                }
                else{
                    ChangeStateTo(BattleState.START); // GAME LOOOOP!
                }
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

    // private void DoActions()
    // {
    //     foreach(PlayerController player in players)
    //     {
    //         player.DoAction();
    //     }
    // }

    private void CalculateResults()
    {
        // Who shoots first?
        ShootFirst();

        // Who DIES??
        GetShotAndDie(); // Death scream audio
        
        // Who Loads??
        LoadGun();

        // Who Dodges (Moves forward)??
        DodgeBullet();
    }

    private void ShootFirst()
    {
        foreach(PlayerController player in shootPlayers)
        {
            if(player.BulletCount > 0){
                player.Shoot();
            }
            else
            {
                targetObjects.Remove(player.Target); //doesnt have bullets to shoot. lose the turn!
                player.DryShoot();
            }
        }
    }

    private void DodgeBullet()
    {
        foreach(PlayerController player in dodgePlayers)
        {
            player.Dodge();
            if(targetObjects.Contains(player.gameObject))
            {
                player.PlayRandomDodgeAudio();
            }
        }
    }

    private void LoadGun()
    {
        foreach(PlayerController player in loadPlayers)
        {
            if(!player.IsDead){
                player.Load();
            }
        }
    }

    private void GetShotAndDie()
    {
        foreach(GameObject target in targetObjects)
        {   
            if(target.name.Equals("Cage")) // Someone shot the cage!!
            {
                cageState = CageState.BROKEN;
            }
            else
            {
                var targetPlayer = target.GetComponent<PlayerController>();
                if(idlePlayers.Contains(targetPlayer) || loadPlayers.Contains(targetPlayer) || shootPlayers.Contains(targetPlayer))
                {
                    targetPlayer.Die();
                }
            }
        }
    }

    private void EndGame()
    {
        if(IsEverybodyDead())
        {
            endgameText.text = everybodyIsDeadText;
        }
        else if(OnlyOneAlive())
        {
            foreach(PlayerController player in players)
            {
                if(!player.IsDead)
                {
                    winner = player;
                }
            }
            endgameText.text = winner.NickName + " Victory!";
        }
        else if(SomeoneGotCocktail())
        {
            var endgameString = "";
            List<string> whoGotTheCocktail = new List<string>();
            WhoGotTheCocktail(whoGotTheCocktail);
            if(whoGotTheCocktail.Count == 1)
            {
                endgameString += whoGotTheCocktail.First() + " Got the Drink!";
            }
            else
            {
                foreach(string name in whoGotTheCocktail)
                {
                    endgameString += name + " ";
                }
                endgameString += "Shared the Drink!";
            }
            endgameText.text = endgameString;
        }
        endgameCanvas.SetActive(true);
    }

    private void WhoGotTheCocktail(List<string> whoGotTheCocktail)
    {
        foreach(PlayerController player in players)
        {
            if(player.GotTheCocktail)
            {
                whoGotTheCocktail.Add(player.NickName);
            } 
        }
    }

    private bool EndgameCondition()
    {
        return IsEverybodyDead() || OnlyOneAlive() || SomeoneGotCocktail();
    }

    private bool SomeoneGotCocktail()
    {
        foreach(PlayerController player in players)
        {
            if(player.GotTheCocktail)
            {
                return true;
            } 
        }
        return false;
    }

    private bool OnlyOneAlive()
    {
        var playersAlive = 0;
        foreach(PlayerController player in players)
        {
            if(!player.IsDead)
            {
                playersAlive++;
            }
        }
        return (playersAlive == 1);
    }

    private bool IsEverybodyDead()
    {
        var isEverybodyDead = true;
        foreach(PlayerController player in players)
        {
            if(!player.IsDead)
            {
                isEverybodyDead = false;
                return isEverybodyDead;
            }
        }
        return isEverybodyDead;
    }

    private void GetPlayersActions()
    {
        foreach(PlayerController player in players)
        {
            switch(player.Action)
            {
                case PlayerActions.IDLE:
                {
                    idlePlayers.Add(player);
                    break;
                }
                case PlayerActions.LOAD:
                {
                    loadPlayers.Add(player);
                    break;
                }
                case PlayerActions.DODGE:
                {
                    dodgePlayers.Add(player);
                    break;
                }
                case PlayerActions.SHOOT:
                {
                    shootPlayers.Add(player);
                    targetObjects.Add(player.Target);
                    break;
                }
                default: break;
            }
        }
    }

    private void StartCountdown()
    {
        if(!countdownTimer.IsCounting)
        {
            if(!countdownTimer.FinishedCounting)
            {
                countdownTimer.StartCountdown();
            }
            else
            {
                ChangeStateTo(BattleState.PLAYERACTION);
                countdownTimer.ResetCountdown();
            }
        }
    }

    private void ClearAllActionLists()
    {
        idlePlayers.Clear();
        shootPlayers.Clear();
        dodgePlayers.Clear();
        loadPlayers.Clear();
        targetObjects.Clear();
    }

    private void ResetPlayersActions()
    {
        foreach(PlayerController player in players)
        {
            player.ResetAction();
        }
    }

}
