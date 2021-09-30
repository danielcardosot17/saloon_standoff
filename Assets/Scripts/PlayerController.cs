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
    private Player photonPlayer;
    private string nickName;
    public string NickName { get => nickName; private set => nickName = value; }
    
    private int playerId;
    private int playerNumber;
    private PlayerActions action;
    public PlayerActions Action { get => action; private set => action = value; }
    private int bulletCount = 0;
    private int maxBulletCount;
    public int BulletCount { get => bulletCount; private set => bulletCount = value; }
    private int maxMoveFoward;
    private bool isDead = false;
    public bool IsDead { get => isDead; private set => isDead = value; }

    private GameObject target = null; // might be the cage
    public GameObject Target { get => target; private set => target = value; }
    public PlayerSprite[] PlayerSprites { get => playerSprites; private set => playerSprites = value; }

    [PunRPC]
    private void InitializePlayer(Player player, int spriteNumber)
    {
        print("bbbbbbb");
        print(player.NickName);
        print(spriteNumber);
        print(PhotonNetwork.LocalPlayer.GetPlayerNumber());
        print(player.GetPlayerNumber());
        photonPlayer = player;
        nickName = photonPlayer.NickName;
        playerId = player.ActorNumber;
        playerNumber = player.GetPlayerNumber();
        ChangePlayerSprite(spriteNumber);
        GameplayManager.Instance.Players.Add(this);
        BattleSystem.Instance.Players.Add(this);
        playerName.text = player.NickName;
        if(!photonView.IsMine) DisablePlayer(this);
    }

    private void ChangePlayerSprite(int spriteNumber)
    {
        this.GetComponent<SpriteRenderer>().sprite = playerSprites[playerNumber].sprites[spriteNumber];
    }

    private void DisablePlayer(PlayerController player)
    {
        player.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetAction();
        isDead = false;
        target = null;
        bulletCount = 0;
        maxBulletCount = BattleSystem.Instance.MaxBulletCount;
    }

    void Update()
    {
        if(!isDead)
        {
            //only accepts input while countdown
            // if(BattleSystem.Instance.BattleState == BattleState.COUNTDOWN)
            // {
                if(Input.GetKey(KeyCode.Space))
                {
                    DodgeAction();
                }
                if(Input.GetKey(KeyCode.R))
                {
                    LoadAction();
                }
                ShootActionCheck();
            // }
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

    public void ResetAction()
    {
        action = PlayerActions.IDLE;
        target = null;
    }


    private void LoadAction()
    {
        action = PlayerActions.LOAD;
        if(bulletCount < maxBulletCount)
        {
            bulletCount++;
        }
    }

    private void DodgeAction()
    {
        action = PlayerActions.DODGE;
        MoveFoward();
    }

    private void MoveFoward()
    {
        
    }

    private void ShootAction(GameObject target)
    {
        action = PlayerActions.SHOOT;
        this.target = target;
    }

    public void Die()
    {
        isDead = true;
        playerName.text += "\n" + "DEAD";
        this.GetComponent<SpriteRenderer>().color = Color.red;
        DisablePlayer(this);
    }
}
