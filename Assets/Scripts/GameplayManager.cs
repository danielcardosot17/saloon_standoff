using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameplayManager : MonoBehaviourPunCallbacks
{
    public static GameplayManager Instance {get; private set;}
    [SerializeField] private string prefabLocation;
    [SerializeField] private Transform[] spawnLocations;
    [SerializeField] private TMP_Text artistText;
    [SerializeField] private TMP_Text musicText;
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
        players = new List<PlayerController>();
        playersInGame = 0;
        photonView.RPC("AddPlayer", RpcTarget.AllBuffered);
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
        print("CreatePlayer");
        print(PhotonNetwork.LocalPlayer.GetPlayerNumber());
        print(PhotonNetwork.LocalPlayer.NickName);
        var spriteNumber = Random.Range(0,player.PlayerSprites[PhotonNetwork.LocalPlayer.GetPlayerNumber()].sprites.Length);
        player.photonView.RPC("InitializePlayer", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer, spriteNumber);
    }

    [PunRPC]
    public void PlayBackgroundMusicRPC(int randomNumber)
    {
        AudioManager.Instance.PlayFromGroupDelayedReturnSound("Music",randomNumber);
        artistText.text = AudioManager.Instance.GetArtistName("Music",randomNumber);
        musicText.text = AudioManager.Instance.GetSoundName("Music",randomNumber);
    }
}
