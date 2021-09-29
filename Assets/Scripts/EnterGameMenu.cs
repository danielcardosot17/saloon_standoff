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
        AudioManager.Instance.PlayDelayed("Click1");
        NetworkManager.Instance.CreateRoom(roomName.text);
        NetworkManager.Instance.SetUsername(userName.text);
    }

    public void JoinRoom()
    {
        AudioManager.Instance.PlayDelayed("Click2");
        NetworkManager.Instance.JoinRoom(roomName.text);
        NetworkManager.Instance.SetUsername(userName.text);
    }
    public void UpdatePlayerConnectedCount()
    {
        playerList.text = NetworkManager.Instance.GetPlayerCount() + " Players online.";
    }
    
}
