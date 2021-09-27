using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class EnterGameMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text roomName;
    [SerializeField] private TMP_Text userName;
    [SerializeField] private TMP_Text playerList;

    private void Update() {
        UpdatePlayerConnectedCount();
    }
    
    public void CreateRoom()
    {
        NetworkManager.Instance.CreateRoom(roomName.text);
        NetworkManager.Instance.SetUsername(userName.text);
    }

    public void JoinRoom()
    {
        NetworkManager.Instance.JoinRoom(roomName.text);
        NetworkManager.Instance.SetUsername(userName.text);
    }
    public void UpdatePlayerConnectedCount()
    {
        playerList.text = NetworkManager.Instance.GetPlayerCount() + " Players online.";
    }
    
}
