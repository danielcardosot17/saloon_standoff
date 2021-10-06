using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviourPunCallbacks
{
    [SerializeField] private EnterGameMenu enterGameMenu;
    [SerializeField] private LobbyMenu lobbyMenu;

    private void Start()
    {
        enterGameMenu.gameObject.SetActive(false);
        lobbyMenu.gameObject.SetActive(false);
    }
    
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        enterGameMenu.gameObject.SetActive(true);
        enterGameMenu.UpdatePlayerConnectedCount();
        if(!AudioManager.Instance.IsPlaying("EnterGameMusic"))
        {
            AudioManager.Instance.PlayDelayed("EnterGameMusic");
        }
    }

    public override void OnJoinedRoom()
    {
        print("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        ChangeMenu(lobbyMenu.gameObject);
        lobbyMenu.photonView.RPC("UpdatePlayerList",RpcTarget.All);
    }

    public override void OnCreatedRoom()
    {
        print("Created room: " + PhotonNetwork.CurrentRoom.Name);
        // enterGameMenu.photonView.RPC("UpdateRoomList", RpcTarget.All, PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        print("OnRoomListUpdate");
        var roomNames = "";
        var roomCount = 0;
        foreach(RoomInfo info in roomList)
        {
            if(info.PlayerCount > 0)
            {
                print(info.Name);
                var playerMaxAndCount = "";
                playerMaxAndCount = " ... " + info.PlayerCount + "." + info.MaxPlayers;
                roomNames += info.Name + playerMaxAndCount + "\n";
                roomCount++;
            }
        }
        enterGameMenu.UpdateRoomList(roomNames);
        enterGameMenu.UpdateRoomCount(roomCount);
    }

    public void ChangeMenu(GameObject menu)
    {
        enterGameMenu.gameObject.SetActive(false);
        lobbyMenu.gameObject.SetActive(false);
        menu.SetActive(true);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        lobbyMenu.UpdatePlayerList();
    }

    public void ExitLobby()
    {
        AudioManager.Instance.PlayDelayed("Click1");
        NetworkManager.Instance.ExitLobby();
        ChangeMenu(enterGameMenu.gameObject);
    }

    public void StartGame(string sceneName)
    {
        AudioManager.Instance.PlayDelayed("Click1");
        NetworkManager.Instance.photonView.RPC("StartGame",RpcTarget.AllViaServer, sceneName);
    }
}
