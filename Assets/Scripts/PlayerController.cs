using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    private Player photonPlayer;
    private int playerId;

    [PunRPC]
    private void InitializePlayer(Player player)
    {
        photonPlayer = player;
        playerId = player.ActorNumber;
        GameplayManager.Instance.Players.Add(this);
        if(!photonView.IsMine) DisablePlayer();
    }

    private void DisablePlayer()
    {
        // throw new NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
