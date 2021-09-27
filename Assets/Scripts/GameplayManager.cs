using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GameplayManager : MonoBehaviourPunCallbacks
{
    public static GameplayManager Instance {get; private set;}

    [SerializeField] private string prefabLocation;
    [SerializeField] private Transform[] spawnLocations;
    private int playersInGame = 0;
    public List<PlayerController> Players { get => players; private set => players = value; }
    private List<PlayerController> players;

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

    private void Start() {
        photonView.RPC("AddPlayer", RpcTarget.AllBuffered);
        Players = new List<PlayerController>(); 
    }

    [PunRPC]
    private void AddPlayer()
    {
        playersInGame++;
        if(playersInGame == PhotonNetwork.PlayerList.Length)
        {
            CreatePlayer();
        }
    }

    private void CreatePlayer()
    {
        var playerObj = PhotonNetwork.Instantiate(prefabLocation, spawnLocations[PhotonNetwork.PlayerList.Length - 1].position, Quaternion.identity);
        var player = playerObj.GetComponent<PlayerController>();
        player.photonView.RPC("InitializePlayer", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }
}
