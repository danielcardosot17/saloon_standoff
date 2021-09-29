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
        enterGameMenu.gameObject.SetActive(true);
        enterGameMenu.UpdatePlayerConnectedCount();
        if(!AudioManager.Instance.IsPlaying("EnterGameMusic"))
        {
            AudioManager.Instance.PlayDelayed("EnterGameMusic");
        }
    }

    public override void OnJoinedRoom()
    {
        ChangeMenu(lobbyMenu.gameObject);
        lobbyMenu.photonView.RPC("UpdatePlayerList",RpcTarget.All);
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

    public void StartGame(string gameSceneName)
    {
        AudioManager.Instance.PlayDelayed("Click1");
        NetworkManager.Instance.photonView.RPC("StartGame",RpcTarget.All, gameSceneName);
    }
}
