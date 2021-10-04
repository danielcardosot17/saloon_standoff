using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BattleState { START, COUNTDOWN, PLAYERACTION, RESULT, IDLE, END}
public enum BattleMode { DEFAULT }
public enum CageState { NORMAL, BROKEN }
public class BattleSystem : MonoBehaviourPunCallbacks
{
    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private int maxBulletCount;
    [SerializeField] private GameObject endgameCanvas;
    [SerializeField] private TMP_Text endgameText;
    [SerializeField] private string soloVictoryText;
    [SerializeField] private string[] everybodyIsDeadText;
    [SerializeField] private string sharedDrinkText;
    [SerializeField] private string gotTheDrinkText;
    [SerializeField] private AchievementManager achievementManager;
    [SerializeField] private TMP_Text achievementsText;
    [SerializeField] private float idleTime;
    [SerializeField] private GameObject cage;
    [SerializeField] private string cageBreakAudio;
    [SerializeField] private TMP_Text artistText;
    [SerializeField] private TMP_Text musicText;
    public int MaxBulletCount { get => maxBulletCount; private set => maxBulletCount = value; }
    private PlayerController soloWinner;
    private PlayerController localPlayer;
    public PlayerController LocalPlayer { get => localPlayer; set => localPlayer = value; }
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
    private bool isEnd = false;
    private List<string> playersWhoGotTheCocktail;
    public List<string> PlayersWhoGotTheCocktail { get => playersWhoGotTheCocktail; set => playersWhoGotTheCocktail = value; }

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
        isEnd = false;
        cageState = CageState.NORMAL;
        cage.SetActive(true);
        players = new List<PlayerController>();
        idlePlayers = new List<PlayerController>();
        loadPlayers = new List<PlayerController>();
        shootPlayers = new List<PlayerController>();
        dodgePlayers = new List<PlayerController>();
        targetObjects = new List<GameObject>();
        playersWhoGotTheCocktail = new List<string>();
        if(BattleSystem.Instance.BattleMode == BattleMode.DEFAULT)
        {
            MaxBulletCount = 1;
        }
        
        endgameCanvas.SetActive(false);
        countdownTimer.ResetCountdown();
        AudioManager.Instance.StopAllExcept();
        PlayRandomBackgroundMusic();
    }

    private void ChangeStateTo(BattleState newState)
    {
        BattleState = newState;
    }

    void Update()
    {
        if(!isEnd)
        {
            switch(battleState)
            {
                case BattleState.START:
                {
                    print("START");
                    countdownTimer.ResetCountdown();
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

                    // need to give time between Results and next phase.
                    CalculateResults();
                    ChangeStateTo(BattleState.IDLE);
                    StartCoroutine(EnterIdleStateForThisTimeThenCheckEndgame(idleTime));
                    break;
                }
                case BattleState.END:
                {
                    print("END");
                    EndGame();
                    break;
                }
                case BattleState.IDLE:
                {
                    print("IDLE");
                    break;
                }
                default: break;
            }
        }
    }

    IEnumerator EnterIdleStateForThisTimeThenCheckEndgame(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if(EndgameCondition())
        {
            ChangeStateTo(BattleState.END);
        }
        else{
            ChangeStateTo(BattleState.START); // GAME LOOOOP!
        }
    }

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

        // Get Kill Count
        GetPlayerKillCount();
    }

    private void GetPlayerKillCount()
    {
        foreach(PlayerController player in players)
        {
            player.GetKillCount();
        }
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
                var randomDelay = Random.Range(player.AudioMaxDelay/2, player.AudioMaxDelay);
                player.PlayRandomDodgeAudio(randomDelay);
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
                StartCoroutine(PlayCageBreakAudio());
            }
            else
            {
                var targetPlayer = target.GetComponent<PlayerController>();
                if(idlePlayers.Contains(targetPlayer) || loadPlayers.Contains(targetPlayer) || shootPlayers.Contains(targetPlayer))
                {
                    //RPC
                    targetPlayer.photonView.RPC("Die", RpcTarget.AllBuffered);
                    // targetPlayer.Die();
                }
            }
        }
    }

    IEnumerator  PlayCageBreakAudio()
    {
        // just to make cage disappear at the same time of the sound
        AudioManager.Instance.PlayDelayed(cageBreakAudio, localPlayer.AudioMaxDelay);
        yield return new WaitForSeconds(localPlayer.AudioMaxDelay);
        cage.SetActive(false);
    }

    private void EndGame()
    {
        isEnd = true;
        countdownTimer.DeactivateTimerCanvas();
        DisableAllPlayers();
        ShowEndgameText();
        ShowAchievements();
        endgameCanvas.SetActive(true);
    }

    private void DisableAllPlayers()
    {
        foreach(PlayerController player in players)
        {
            player.DisableForEndgame();
        }
    }

    // Achievements will be shown individually if player wins
    private void ShowAchievements()
    {
        var text = "";
        if(localPlayer.IsWinner)
        {
            foreach(Achievement achievement in achievementManager.Achievements)
            {
                if(achievement.Condition()){
                    text += achievement.achievementName + "\n";
                }
            }
        }
        achievementsText.text = text;
    }

    private void ShowEndgameText()
    {
        if(IsEverybodyDead())
        {
            endgameText.text = everybodyIsDeadText[Random.Range(0,everybodyIsDeadText.Length)];
        }
        else if(OnlyOneAlive())
        {
            foreach(PlayerController player in players)
            {
                if(!player.IsDead)
                {
                    soloWinner = player;
                    player.IsWinner = true;
                }
            }
            endgameText.text = soloWinner.NickName + "\n" + soloVictoryText;
        }
        else if(SomeoneGotCocktail())
        {
            var endgameString = "";
            WhoGotTheCocktail();
            if(playersWhoGotTheCocktail.Count == 1)
            {
                endgameString += playersWhoGotTheCocktail.First() + "\n" + gotTheDrinkText;
            }
            else
            {
                foreach(string name in playersWhoGotTheCocktail)
                {
                    endgameString += name + " ";
                }
                endgameString += "\n" + sharedDrinkText;
            }
            endgameText.text = endgameString;
        }
    }

    private void WhoGotTheCocktail()
    {
        foreach(PlayerController player in players)
        {
            if(player.GotTheCocktail)
            {
                playersWhoGotTheCocktail.Add(player.NickName);
                player.IsWinner = true;
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

    public bool OnlyOneAlive()
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
        foreach(PlayerController player in players)
        {
            if(!player.IsDead)
            {
                return false;
            }
        }
        return true;
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
                print("COUNTDOWN");
                ChangeStateTo(BattleState.PLAYERACTION);
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

    public void PlayAgain()
    {
        ResetEverything();
        AudioManager.Instance.StopAllExcept();
        PlayRandomBackgroundMusic();
    }

    private void PlayRandomBackgroundMusic()
    {
        var music = AudioManager.Instance.PlayRandomFromGroupDelayedReturnSound("Music");
        artistText.text = music.artistName;
        musicText.text = music.name;

    }

    private void ResetEverything()
    {
        ResetPlayersActions();
        ClearAllActionLists();
        ResetPlayersVariables();
        ResetEndgameCanvas();
        ResetWhoGotCocktailAndWinners();
        ResetCage();
        countdownTimer.ResetCountdown();
        countdownTimer.ActivateTimerCanvas();
        ChangeStateTo(BattleState.START);
        isEnd = false;
    }

    private void ResetWhoGotCocktailAndWinners()
    {
        soloWinner = null;
        playersWhoGotTheCocktail.Clear();
    }

    private void ResetPlayersVariables()
    {
        foreach(PlayerController player in players)
        {
            player.ResetBullets();
            player.ResetPositionAndStepCount();
            player.ResetKillCount();
            player.ResetName();
            player.ResetSpriteColor();
            player.ResetWinner();
            player.ResetIsDead();
            player.ResetGotCocktail();
            player.EnableAgain();
        }
    }

    private void ResetCage()
    {
        cageState = CageState.NORMAL;
        cage.SetActive(true);
    }

    private void ResetEndgameCanvas()
    {
        endgameText.text = "";
        achievementsText.text = "";
        endgameCanvas.SetActive(false);
    }

    public void ExitSaloon()
    {
    }
}
