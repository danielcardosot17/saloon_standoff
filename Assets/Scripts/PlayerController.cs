using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using UnityEngine;

// public enum PlayerState { ALIVE, DEAD }
public enum PlayerActions { IDLE, LOAD, SHOOT, DODGE }
public enum PlayerPosition { INITIAL, MIDLE, FINAL }
public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private Transform midPos;
    [SerializeField] private Transform finalPos;
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

    private bool isDead = false;
    public bool IsDead { get => isDead; private set => isDead = value; }

    private GameObject target = null; // might be the cage
    public GameObject Target { get => target; private set => target = value; }

    [PunRPC]
    private void InitializePlayer(Player player)
    {
        photonPlayer = player;
        nickName = photonPlayer.NickName;
        playerId = player.ActorNumber;
        playerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();
        GameplayManager.Instance.Players.Add(this);
        BattleSystem.Instance.Players.Add(this);
        playerName.text = player.NickName;
        if(!photonView.IsMine) DisablePlayer(this);
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
        //only accepts input while countdown
        if(BattleSystem.Instance.BattleState == BattleState.COUNTDOWN)
        {

        }
    }

    private void ResetAction()
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
    }

    private void MoveFoward()
    {
        throw new NotImplementedException();
    }

    private void ShootAction(GameObject target)
    {
        action = PlayerActions.SHOOT;
        this.target = target;
    }
}
