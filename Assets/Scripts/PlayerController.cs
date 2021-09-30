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
    private int playerId;
    private int playerNumber;
    private PlayerActions action;
    public PlayerActions Action { get => action; private set => action = value; }
    private int bulletCount = 0;
    private int maxBulletCount;
    public int BulletCount { get => bulletCount; private set => bulletCount = value; }

    private bool isDead = false;
    public bool IsDead { get => isDead; private set => isDead = value; }

    [PunRPC]
    private void InitializePlayer(Player player)
    {
        photonPlayer = player;
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
        IsDead = false;
        bulletCount = 0;
        maxBulletCount = BattleSystem.Instance.MaxBulletCount;
    }

    void Update()
    {
        
    }

    private void ResetAction()
    {
        action = PlayerActions.IDLE;
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

    private void ShootAction()
    {
        action = PlayerActions.SHOOT;
    }
}
