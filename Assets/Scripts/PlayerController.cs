using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public enum PlayerActions : byte
{ IDLE = 0, LOAD, SHOOT, DODGE }
public enum PlayerPosition { INITIAL, MIDLE, FINAL }
public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private int playerLayer;
    [SerializeField] private PlayerSprite[] playerSprites;
    [SerializeField] private GameObject targetCrossHair;
    [SerializeField] private Vector3 crossHairOffset;
    [SerializeField] private float moveTime;
    
    [Range(1,2)]
    [SerializeField] private int maxMoveFoward = 2;
    [SerializeField] private Vector3[] finalPositions;

    [Range(0,3)]
    [SerializeField] private float audioMaxDelay = 1;
    [SerializeField] private Transform bulletOrigin;
    [SerializeField] private LineRenderer bulletLine;
    [SerializeField] private float bulletLineTime;
    [SerializeField] private GameObject bulletCanvas;

    private Player photonPlayer;
    private string nickName;
    public string NickName { get => nickName; private set => nickName = value; }
    
    private int playerId;
    private int playerNumber;
    public int PlayerNumber { get => playerNumber; private set => playerNumber = value; }
    private int killCount = 0;
    public int KillCount { get => killCount; private set => killCount = value; }
    private int bulletsUsed = 0;
    public int BulletsUsed { get => bulletsUsed; set => bulletsUsed = value; }
    private PlayerActions action;
    public PlayerActions Action { get => action; private set => action = value; }
    private PlayerActions chosenAction;
    public PlayerActions ChosenAction { get => chosenAction; private set => chosenAction = value; }
    private int bulletCount = 0;
    private int maxBulletCount;
    public int BulletCount { get => bulletCount; private set => bulletCount = value; }
    private bool isDead = false;
    public bool IsDead { get => isDead; private set => isDead = value; }

    private GameObject target = null; // might be the cage
    public GameObject Target { get => target; private set => target = value; }
    private int targetNumber = -10; // -10 = nothing, -5 = cage
    public int TargetNumber { get => targetNumber; private set => targetNumber = value; }
    private int chosenTarget = -10;
    public int ChosenTarget { get => chosenTarget; private set => chosenTarget = value; }
    
    private Vector3 originalPosition;
    private Color originalColor;
    private Vector3 moveStep; // not include spawns
    private int stepCount = 0;
    private bool gotTheCocktail = false;
    private bool isDisabled = false;
    private bool isWinner = false;
    public bool IsWinner { get => isWinner; set => isWinner = value; }
    private bool isLocalPlayer = false;

    public bool IsLocalPlayer { get => isLocalPlayer; set => isLocalPlayer = value; }

    public bool GotTheCocktail { get => gotTheCocktail; private set => gotTheCocktail = value; }

    public PlayerSprite[] PlayerSprites { get => playerSprites; private set => playerSprites = value; }
    public float AudioMaxDelay { get => audioMaxDelay; set => audioMaxDelay = value; }

    private bool isReady = false;
    public bool IsReady { get => isReady; private set => isReady = value; }

    [PunRPC]
    private void InitializePlayer(Player player, int spriteNumber)
    {
        photonPlayer = player;
        nickName = photonPlayer.NickName;
        playerId = player.ActorNumber;
        playerNumber = player.GetPlayerNumber();
        ChangePlayerSprite(spriteNumber);
        originalPosition = transform.position;
        originalColor = this.GetComponent<SpriteRenderer>().color;
        moveStep = (finalPositions[playerNumber] - transform.position)/maxMoveFoward;
        GameplayManager.Instance.Players.Add(this);
        BattleSystem.Instance.Players.Add(this);
        playerName.text = player.NickName;
        if(photonView.IsMine)
        {
            isLocalPlayer = true;
            BattleSystem.Instance.LocalPlayer = this;
        }
        isReady = true;
    }

    private void ChangePlayerSprite(int spriteNumber)
    {
        this.GetComponent<SpriteRenderer>().sprite = playerSprites[playerNumber].sprites[spriteNumber];
    }

    private void DisablePlayer(PlayerController player)
    {
        // cant disable like this otherwise script wont work. Only in death may disable
        DisableCrosshair();
        player.GetComponent<Collider2D>().enabled = false;
        player.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetAction();
        isDead = false;
        isDisabled = false;
        isWinner = false;
        gotTheCocktail = false;
        bulletCount = 0;
        stepCount = 0;
        killCount = 0;
        targetNumber = -10;
        chosenTarget = targetNumber;
        bulletLine.enabled = false;
        bulletCanvas.SetActive(false);
        maxBulletCount = BattleSystem.Instance.MaxBulletCount;
    }

    void Update()
    {
        if(!isDead && photonView.IsMine && !isDisabled)
        {
            //only accepts input while countdown
            if(BattleSystem.Instance.BattleState == BattleState.COUNTDOWN)
            {
                if(Input.GetKeyDown(KeyCode.R))
                {
                    LoadAction();
                }
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    DodgeAction();
                }
                ShootActionCheck();
            }
        }
    }

    private void ShootActionCheck()
    {
        if(Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
            ShootCollisionCheck(ray);
        }
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            ShootCollisionCheck(ray);
        }
    }

    private void ShootCollisionCheck(Ray ray)
    {
        RaycastHit2D hit;
        int playerLayerMask = 1 << playerLayer;
        hit = Physics2D.Raycast(ray.origin, ray.direction, 20f, playerLayerMask);
        if(hit.collider != null)
        {
            ShootAction();
            if(hit.collider.gameObject.name.Equals("Cage"))
            {
                targetNumber = -5;
                chosenTarget = targetNumber;
            }
            else
            {
                targetNumber = hit.collider.gameObject.GetComponent<PlayerController>().PlayerNumber;
                chosenTarget = targetNumber;
            }
            this.target = hit.collider.gameObject;
            PutCrosshairOnTarget();
        }
    }

    private bool  IsTargetDead()
    {
        if(target != null)
        {
            if(!target.name.Equals("Cage")) // its a player
            {
                if(target.GetComponent<PlayerController>().IsDead) return true;
            }
        }
        return false;
    }

    public void ResetAction()
    {
        print("ResetAction");
        print(nickName);
        action = PlayerActions.IDLE;
        chosenAction = PlayerActions.IDLE;
        DisableCrosshair();
    }

    private void LoadAction()
    {
        print("LoadAction");
        print(nickName);
        chosenAction = PlayerActions.LOAD;
        DisableCrosshair();
    }

    private void DodgeAction()
    {
        print("DodgeAction");
        print(nickName);
        chosenAction = PlayerActions.DODGE;
        DisableCrosshair();
    }

    private void ShootAction()
    {
        print("ShootAction");
        print(nickName);
        chosenAction = PlayerActions.SHOOT;
    }

    [PunRPC]
    public void UpdateActionAndTargetRPC(byte chosenAction, int chosenTarget)
    {
        print("UpdateActionAndTargetRPC");
        print(nickName);
        print(chosenTarget);
        action = (PlayerActions)chosenAction;
        print(action.ToString());
        if(action == PlayerActions.SHOOT)
        {
            targetNumber = chosenTarget;
            SetTargetObject(chosenTarget);
        }
    }

    private void SetTargetObject(int chosenTarget)
    {
        if(chosenTarget == -5) // cage
        {
            target = BattleSystem.Instance.Cage;
        }
        else
        {
            target = BattleSystem.Instance.Players.Find(player => player.PlayerNumber == chosenTarget).gameObject;
        }
    }

    public void GetKillCount()
    {
        if(IsTargetDead()) killCount++;
    }

    // to get th cocktail the player must LOAD
    // so the player will be vunerable
    // and that makes DODGE a viable options still
    // more than one player may get share the cocktail!!
    public void Load()
    {
        if(stepCount >= maxMoveFoward && BattleSystem.Instance.CageState == CageState.BROKEN)
        {
            print("GetCocktail");
            print(nickName);
            GetCocktail();
        }
        else
        {
            print("Load");
            print(nickName);
            LoadAnimation();
            print(bulletCount);
            if(bulletCount < maxBulletCount)
            {
                bulletCount++;
                UpdateBulletCanvas();
            }
            print(bulletCount);
        }
    }

    private void GetCocktail()
    {
        PlayRandomCocktailAudio();
        gotTheCocktail = true;
    }

    public void Dodge()
    {
        print("Dodge");
        print(nickName);
        if(stepCount < maxMoveFoward)
        {
            MoveFoward();
            stepCount++;
        }
        if(stepCount == maxMoveFoward)
        {
            BattleSystem.Instance.ChangeLoadButtonText();
        }
    }

    public void MoveFoward()
    {
        DodgeAnimation();
        StartCoroutine(LerpPosition(transform.position + moveStep, moveTime));
    }

    public void DisableForEndgame()
    {
        DisableCrosshair();
        playerName.gameObject.SetActive(false);
        isDisabled = true;
        isReady = false;
    }

    public void Shoot()
    {
        print("Shoot");
        print(nickName);
        bulletCount--;
        bulletsUsed++;
        UpdateBulletCanvas();
        ShootAnimation();
    }

    public void DryShoot()
    {
        print("DryShoot");
        print(nickName);
        DryShootAnimation();
    }

    private void PutCrosshairOnTarget()
    {
        targetCrossHair.transform.position = this.target.transform.position + crossHairOffset;
        EnableCrosshair();
    }

    private void EnableCrosshair()
    {
        targetCrossHair.SetActive(true);
    }

    private void DisableCrosshair()
    {
        targetCrossHair.SetActive(false);
        target = null;
        targetNumber = -10;
        chosenTarget = targetNumber;
    }
    
    public void Die()
    {
        print("Die");
        print(nickName);
        PlayRadomDeathAudio();
        isDead = true;
        playerName.text += "\n" + "DEAD";
        this.GetComponent<SpriteRenderer>().color = Color.red;
        ResetAction();
        this.enabled = false;
        DisablePlayer(this);
    }

    // All audio will be RPC, so other players may hear loading shooting and dodging
    
    private void DryShootAnimation()
    {
        var randomDelay = Random.Range(0.0f,audioMaxDelay/2);
        PlayRadomDryShotAudio(audioMaxDelay);
    }

    private void ShootAnimation()
    {
        var randomDelay = Random.Range(0.0f,audioMaxDelay/2);
        PlayRandomShotAudio(randomDelay);
        StartCoroutine(TurnOnBulletLine(randomDelay));
        StartCoroutine(TurnOffBulletLine(randomDelay + bulletLineTime));
    }

    IEnumerator TurnOnBulletLine(float afterTime)
    {
        yield return new WaitForSeconds(afterTime);
        SetBulletLine();
        bulletLine.enabled = true;
    }

    private void SetBulletLine()
    {
        bulletLine.SetPosition(0,bulletOrigin.position);
        bulletLine.SetPosition(1,Target.transform.position);
    }

    IEnumerator TurnOffBulletLine(float afterTime)
    {
        yield return new WaitForSeconds(afterTime);
        bulletLine.enabled = false;
    }

    private void LoadAnimation()
    {
        // load has 3 steps
        // take out shells if has already shot
        // load shell in
        // cock gun
        // these sounds must be played in sequence
        var shellAudioLength = 0.0f;
        var loadAudioLength = 0.0f;

        if(bulletsUsed > 0 || bulletCount > 0) // if he has a bullet in the gun already
        {
            shellAudioLength = PlayRandomShellAudio();
        }
        loadAudioLength = PlayRandomLoadAudio(shellAudioLength);
        PlayRandomCockAudio(shellAudioLength + loadAudioLength);
    }

    private float PlayRandomShellAudio(float delay = 0)
    {
        var randomNumber = Random.Range(0,AudioManager.Instance.SoundGroupLength("Shell"));
        return AudioManager.Instance.PlayFromGroupDelayedReturnSound("Shell", randomNumber, delay).source.clip.length;
    }

    private float PlayRandomLoadAudio(float delay = 0)
    {
        var randomNumber = Random.Range(0,AudioManager.Instance.SoundGroupLength("Load"));
        return AudioManager.Instance.PlayFromGroupDelayedReturnSound("Load", randomNumber, delay).source.clip.length;
    }

    private float PlayRandomCockAudio(float delay = 0)
    {
        var randomNumber = Random.Range(0,AudioManager.Instance.SoundGroupLength("Cock"));
        return AudioManager.Instance.PlayFromGroupDelayedReturnSound("Cock", randomNumber, delay).source.clip.length;
    }

    private void DodgeAnimation()
    {
        ////////////////////
    }

    private void UpdateBulletCanvas()
    {
        if(bulletCount > 0)
        {
            bulletCanvas.SetActive(true);
        }
        else
        {
            bulletCanvas.SetActive(false);
        }
    }
    
    private void PlayRandomCocktailAudio()
    {
        var randomNumber = Random.Range(0,AudioManager.Instance.SoundGroupLength("Cocktail"));
        AudioManager.Instance.PlayFromGroupDelayedReturnSound("Cocktail", randomNumber);
    }
    private void PlayRadomDeathAudio(float delay = 0)
    {
        var randomNumber = Random.Range(0,AudioManager.Instance.SoundGroupLength("Death"));
        AudioManager.Instance.PlayFromGroupDelayedReturnSound("Death", randomNumber, delay);
    }
    
    private void PlayRandomShotAudio(float delay = 0)
    {
        var randomNumber = Random.Range(0,AudioManager.Instance.SoundGroupLength("Shot"));
        AudioManager.Instance.PlayFromGroupDelayedReturnSound("Shot", randomNumber, delay);
    }

    private void PlayRadomDryShotAudio(float delay = 0)
    {
        var randomNumber = Random.Range(0,AudioManager.Instance.SoundGroupLength("Dryshot"));
        AudioManager.Instance.PlayFromGroupDelayedReturnSound("Dryshot", randomNumber, delay);
    }

    public void PlayRandomDodgeAudio(float delay = 0)
    {
        var randomNumber = Random.Range(0,AudioManager.Instance.SoundGroupLength("Dodge"));
        AudioManager.Instance.PlayFromGroupDelayedReturnSound("Dodge", randomNumber, delay);
    }

    IEnumerator LerpPosition(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }

    public void ResetPositionAndStepCount()
    {
        transform.position = originalPosition;
        stepCount = 0;
    }

    public void ResetBullets()
    {
        bulletCount = 0;
        bulletsUsed = 0;
        UpdateBulletCanvas();
    }

    public void ResetKillCount()
    {
        killCount = 0;
    }

    public void ResetName()
    {
        playerName.text = nickName;
    }

    public void ResetSpriteColor()
    {
        this.GetComponent<SpriteRenderer>().color = originalColor;
    }

    public void ResetWinner()
    {
        isWinner = false;
    }

    public void ResetIsDead()
    {
        isDead = false;
    }

    public void EnableAgain()
    {
        
        playerName.gameObject.SetActive(true);
        isDisabled = false;
        
        GetComponent<Collider2D>().enabled = true;
        this.enabled = true;
        isReady = true;
    }

    public void ResetGotCocktail()
    {
        gotTheCocktail = false;
    }

    public void LoadButtonPress()
    {
        if(!isDead && photonView.IsMine && !isDisabled)
        {
            //only accepts input while countdown
            if(BattleSystem.Instance.BattleState == BattleState.COUNTDOWN)
            {
                LoadAction();
            }
        }
    }

    public void DodgeButtonPress()
    {
        if(!isDead && photonView.IsMine && !isDisabled)
        {
            //only accepts input while countdown
            if(BattleSystem.Instance.BattleState == BattleState.COUNTDOWN)
            {
                DodgeAction();
            }
        }
    }
}
