using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class LobbyMenu : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text playerList;
    [SerializeField] private Button startGameBtn;

    [PunRPC]
    public void UpdatePlayerList()
    {
        playerList.text = NetworkManager.Instance.GetPlayerlist();
        startGameBtn.interactable = NetworkManager.Instance.IsMasterClient() && PhotonNetwork.PlayerList.Length >= 2;
    }

}
