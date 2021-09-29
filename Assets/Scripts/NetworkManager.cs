using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance {get; private set;}

    [SerializeField] private int maxPlayers;
    private RoomOptions roomOptions = new RoomOptions();
    
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

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        roomOptions.MaxPlayers = (byte)maxPlayers;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conection success!");
    }

    public void CreateRoom(string roomName)
    { 
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
    public void SetUsername(string username)
    {
        PhotonNetwork.NickName = username;
    }

    public string GetPlayerlist()
    {
        var playerList = "";
        foreach(var player in PhotonNetwork.PlayerList)
        {
            playerList += player.NickName + "\n";
        }
        return playerList;
    }

    public int GetPlayerCount()
    {
        return PhotonNetwork.CountOfPlayers;
    }

    public void ExitLobby()
    {
        PhotonNetwork.LeaveRoom();
    }
    
    [PunRPC]
    public void StartGame(string gameSceneName)
    {
        AudioManager.Instance.StopAllExcept();
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }
}
