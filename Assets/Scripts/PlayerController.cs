using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text playerName;
    private Player photonPlayer;
    private int playerId;
    private PlayerActions action;
    public PlayerActions Action { get => action; private set => action = value; }
    private int bulletCount = 0;
    private int maxBulletCount;
    public int BulletCount { get => bulletCount; private set => bulletCount = value; }


    [PunRPC]
    private void InitializePlayer(Player player)
    {
        photonPlayer = player;
        playerId = player.ActorNumber;
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
        bulletCount = 0;
        maxBulletCount = BattleSystem.Instance.MaxBulletCount;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ResetAction()
    {
        Action = PlayerActions.IDLE;
    }

    private void LoadAction()
    {
        bulletCount++;
    }

}
