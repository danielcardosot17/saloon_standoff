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

    [SerializeField] private int numberOfPlayerSprites;
    [SerializeField] private string[] prefabLocations;
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
        var prefab = prefabLocations[PhotonNetwork.LocalPlayer.GetPlayerNumber()] + "/Player_" + Random.Range(0,numberOfPlayerSprites);
        print("aaaaaaaaaaaaaa" + prefab);
        var playerObj = PhotonNetwork.Instantiate(prefab, spawnLocations[PhotonNetwork.LocalPlayer.GetPlayerNumber()].position, Quaternion.identity);
        var player = playerObj.GetComponent<PlayerController>();
        player.photonView.RPC("InitializePlayer", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }
}
