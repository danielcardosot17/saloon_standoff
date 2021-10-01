using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameplayManager : MonoBehaviourPunCallbacks
{
    public static GameplayManager Instance {get; private set;}
    [SerializeField] private string prefabLocation;
    [SerializeField] private Transform[] spawnLocations;
    private int playersInGame = 0;
    private List<PlayerController> players;
    public List<PlayerController> Players { get => players; private set => players = value; }

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
        players = new List<PlayerController>(); 
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
        var playerObj = PhotonNetwork.Instantiate(prefabLocation, spawnLocations[PhotonNetwork.LocalPlayer.GetPlayerNumber()].position, Quaternion.identity);
        var player = playerObj.GetComponent<PlayerController>();
        var spriteNumber = Random.Range(0,player.PlayerSprites[PhotonNetwork.LocalPlayer.GetPlayerNumber()].sprites.Length);
        player.photonView.RPC("InitializePlayer", RpcTarget.All, PhotonNetwork.LocalPlayer, spriteNumber);
    }
}
