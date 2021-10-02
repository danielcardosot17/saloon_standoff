using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public enum PlayerActions { IDLE, LOAD, SHOOT, DODGE }
public enum PlayerPosition { INITIAL, MIDLE, FINAL }
public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private int playerLayer;
    [SerializeField] private PlayerSprite[] playerSprites;
    [SerializeField] private GameObject targetCrossHair;
    [SerializeField] private Vector3 crossHairOffset;
    [SerializeField] private float moveTime;
    
    [Range(1,3)]
    [SerializeField] private int maxMoveFoward = 2;
    [SerializeField] private Vector3[] finalPositions;
    private Player photonPlayer;
    private string nickName;
    public string NickName { get => nickName; private set => nickName = value; }
    
    private int playerId;
    private int playerNumber;
    private int killCount = 0;
    public int KillCount { get => killCount; private set => killCount = value; }
    private int bulletsUsed = 0;
    public int BulletsUsed { get => bulletsUsed; set => bulletsUsed = value; }
    private PlayerActions action;
    public PlayerActions Action { get => action; private set => action = value; }
    private int bulletCount = 0;
    private int maxBulletCount;
    public int BulletCount { get => bulletCount; private set => bulletCount = value; }
    private bool isDead = false;
    public bool IsDead { get => isDead; private set => isDead = value; }

    private GameObject target = null; // might be the cage
    public GameObject Target { get => target; private set => target = value; }
    
    private Vector3 moveStep; // not include spawns
    private int nextPosition = 0;
    private bool gotTheCocktail = false;
    private bool isDisabled = false;
    private bool isWinner = false;
    public bool IsWinner { get => isWinner; set => isWinner = value; }
    private bool isLocalPlayer = false;
    public bool IsLocalPlayer { get => isLocalPlayer; set => isLocalPlayer = value; }

    public bool GotTheCocktail { get => gotTheCocktail; private set => gotTheCocktail = value; }

    public PlayerSprite[] PlayerSprites { get => playerSprites; private set => playerSprites = value; }

    [PunRPC]
    private void InitializePlayer(Player player, int spriteNumber)
    {
        photonPlayer = player;
        nickName = photonPlayer.NickName;
        playerId = player.ActorNumber;
        playerNumber = player.GetPlayerNumber();
        ChangePlayerSprite(spriteNumber);
        moveStep = (finalPositions[playerNumber] - transform.position)/maxMoveFoward;
        GameplayManager.Instance.Players.Add(this);
        BattleSystem.Instance.Players.Add(this);
        playerName.text = player.NickName;
        if(photonView.IsMine)
        {
            isLocalPlayer = true;
            BattleSystem.Instance.LocalPlayer = this;
        }
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
        nextPosition = 0;
        killCount = 0;
        maxBulletCount = BattleSystem.Instance.MaxBulletCount;
    }

    void Update()
    {
        if(!isDead && photonView.IsMine && !isDisabled)
        {
            //only accepts input while countdown
            if(BattleSystem.Instance.BattleState == BattleState.COUNTDOWN)
            {
                if(Input.GetKey(KeyCode.R))
                {
                    LoadAction();
                }
                if(Input.GetKey(KeyCode.Space))
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
        // #if UNITY_EDITOR
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            ShootCollisionCheck(ray);
        }
        // #endif
    }

    private void ShootCollisionCheck(Ray ray)
    {
        RaycastHit2D hit;
        int playerLayerMask = 1 << playerLayer;
        hit = Physics2D.Raycast(ray.origin, ray.direction, 20f, playerLayerMask);
        if(hit.collider != null)
        {
            ShootAction(hit.collider.gameObject);
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
        action = PlayerActions.IDLE;
        DisableCrosshair();
    }

    private void LoadAction()
    {
        print("LoadAction");
        action = PlayerActions.LOAD;
        DisableCrosshair();
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
        if(nextPosition < maxMoveFoward)
        {
            print("Load");
            LoadAnimation();
            print(bulletCount);
            if(bulletCount < maxBulletCount)
            {
                bulletCount++;
                UpdateBulletCanvas();
            }
            print(bulletCount);
        }
        else
        {
            print("GetCocktail");
            GetCocktail();
        }
    }


    private void DodgeAction()
    {
        print("DodgeAction");
        action = PlayerActions.DODGE;
        DisableCrosshair();
    }

    public void Dodge()
    {
        print("Dodge");
        if(nextPosition < maxMoveFoward)
        {
            MoveFoward();
            nextPosition++;
        }
    }

    private void GetCocktail()
    {
        if(BattleSystem.Instance.CageState == CageState.BROKEN)
        {
            PlayCocktailAudio();
            gotTheCocktail = true;
        }
    }

    public void MoveFoward()
    {
        StartCoroutine(LerpPosition(transform.position + moveStep, moveTime));
    }

    private void ShootAction(GameObject target)
    {
        print("ShootAction");
        this.target = target;
        PutCrosshairOnTarget();
        action = PlayerActions.SHOOT;
    }

    public void DisableForEndgame()
    {
        DisableCrosshair();
        playerName.gameObject.SetActive(false);
        isDisabled = true;
    }

    public void Shoot()
    {
        print("Shoot");
        bulletCount--;
        bulletsUsed++;
        UpdateBulletCanvas();
        ShootAnimation();
    }
    public void DryShoot()
    {
        print("DryShoot");
        DryShootAnimation();
    }

    private void PutCrosshairOnTarget()
    {
        targetCrossHair.transform.position = target.transform.position + crossHairOffset;
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
    }
    
    public void Die()
    {
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
        PlayRadomDryShotAudio();
    }

    private void ShootAnimation()
    {
        PlayRandomShotAudio();
    }

    private void LoadAnimation()
    {
        PlayRandomLoadAudio();
    }

    private void UpdateBulletCanvas()
    {
        
    }
    
    private void PlayCocktailAudio()
    {
        
    }
    private void PlayRadomDeathAudio()
    {
        
    }
    
    private void PlayRandomShotAudio()
    {
        
    }

    private void PlayRadomDryShotAudio()
    {
        
    }

    public void PlayRandomDodgeAudio()
    {
        
    }

    private void PlayRandomLoadAudio()
    {
        
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
}
