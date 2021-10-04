using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private float getActionsDelay;
    [SerializeField] private Button playAgainBtn;
    [SerializeField] private float firstStartWaitTime;
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
    private List<int> targetNumbers;
    private CageState cageState;
    public CageState CageState { get => cageState; private set => cageState = value; }
    public static BattleSystem Instance;
    private bool isEnd = false;
    private List<string> playersWhoGotTheCocktail;
    private bool isEverybodyReady = false;

    // this bool tries to minimaze the delay difference for players of the first start
    private bool readyToFirstStart = false;

    public List<string> PlayersWhoGotTheCocktail { get => playersWhoGotTheCocktail; set => playersWhoGotTheCocktail = value; }
    public GameObject Cage { get => cage; private set => cage = value; }

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
        targetNumbers = new List<int>();
        playersWhoGotTheCocktail = new List<string>();
        if(BattleSystem.Instance.BattleMode == BattleMode.DEFAULT)
        {
            MaxBulletCount = 1;
        }
        
        endgameCanvas.SetActive(false);
        countdownTimer.ResetCountdown();
        readyToFirstStart = false;
        // only master client gives PlayAgain
        if(NetworkManager.Instance.IsMasterClient())
        {
            StartCoroutine(WaitForStartThenPlayAgain());
        }
        // AudioManager.Instance.StopAllExcept();
        // SyncBackgroundMusic();
    }

    IEnumerator WaitForStartThenPlayAgain()
    {
        yield return new WaitForSeconds(firstStartWaitTime);
        PlayAgain();
    }

    private void SyncBackgroundMusic()
    {
        if(NetworkManager.Instance.IsMasterClient())
        {
            // RPC
            PlayRandomBackgroundMusicRPC();
        }
    }

    private void PlayRandomBackgroundMusicRPC()
    {
        // Didnt want to put photon view on BattleSystem too
        var randomNumber = Random.Range(0,AudioManager.Instance.SoundGroupLength("Music"));
        GameplayManager.Instance.photonView.RPC("PlayBackgroundMusicRPC",RpcTarget.AllViaServer,randomNumber);
    }

    private void ChangeStateTo(BattleState newState)
    {
        BattleState = newState;
    }

    void Update()
    {
        if(!isEnd && IsEverybodyReady() && readyToFirstStart)
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
                    UpdatePlayersActions();
                    // go idle while wait for players to update their actions 
                    print("IDLE");
                    ChangeStateTo(BattleState.IDLE);
                    StartCoroutine(WaitThenGetActionsThenChangeState());
                    break;
                }
                case BattleState.RESULT:
                {
                    print("RESULT");
                    // compare all the action lists
                    // need to give time between Results and next phase.
                    CalculateResults();
                    ChangeStateTo(BattleState.IDLE);
                    print("IDLE");
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
                    break;
                }
                default: break;
            }
        }
    }

    IEnumerator WaitThenGetActionsThenChangeState()
    {
        yield return new WaitForSeconds(getActionsDelay);
        GetPlayersActions();
        ChangeStateTo(BattleState.RESULT);
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
                    targetNumbers.Add(player.TargetNumber);
                    break;
                }
                default: break;
            }
        }
    }

    private void UpdatePlayersActions()
    {
        localPlayer.photonView.RPC("UpdateActionAndTargetRPC", RpcTarget.All, (byte)localPlayer.ChosenAction, localPlayer.ChosenTarget);
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
                targetNumbers.Remove(player.TargetNumber); //doesnt have bullets to shoot. lose the turn!
                player.DryShoot();
            }
        }
    }

    private void DodgeBullet()
    {
        foreach(PlayerController player in dodgePlayers)
        {
            player.Dodge();
            if(targetNumbers.Contains(player.PlayerNumber))
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
        foreach(int targetNumber in targetNumbers)
        {   
            if(targetNumber == -5) // Someone shot the cage!!
            {
                cageState = CageState.BROKEN;
                StartCoroutine(PlayCageBreakAudio());
            }
            else
            {
                var targetPlayer = players.Find(player => player.PlayerNumber == targetNumber);
                if(idlePlayers.Contains(targetPlayer) || loadPlayers.Contains(targetPlayer) || shootPlayers.Contains(targetPlayer))
                {
                    // must give time to bulletLine
                    StartCoroutine(DoAfterTimeCoroutine(localPlayer.AudioMaxDelay,() => {
                        targetPlayer.Die();
                    }));
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
        playAgainBtn.interactable = NetworkManager.Instance.IsMasterClient() && PhotonNetwork.PlayerList.Length >= 2;
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
        targetNumbers.Clear();
    }

    private void ResetPlayersActions()
    {
        foreach(PlayerController player in players)
        {
            player.ResetAction();
        }
    }

    // only masterclient can PlayAgain
    public void PlayAgain()
    {
        // RPC needed here
        // ResetEverything();
        photonView.RPC("ResetEverything",RpcTarget.All);
        SyncBackgroundMusic();
    }

    [PunRPC]
    public void ResetEverything()
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
        AudioManager.Instance.StopAllExcept();
        isEnd = false;
        readyToFirstStart = true;
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

    private bool IsEverybodyReady()
    {
        // want to check only in the beginning
        if(isEverybodyReady)
        {
            return true;
        }
        else
        {
            foreach(PlayerController player in players)
            {
                if(!player.IsReady)
                {
                    isEverybodyReady = false;
                    return false;
                }
            }
            isEverybodyReady = true;
            return true; 
        }
    }

    public static IEnumerator DoAfterTimeCoroutine(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }

}
