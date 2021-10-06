using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnterGameMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text roomName;
    [SerializeField] private TMP_Text userName;
    [SerializeField] private TMP_Text playerCount;
    [SerializeField] private TMP_Text roomCount;
    [SerializeField] private TMP_Text roomList;

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
        playerCount.text = NetworkManager.Instance.GetPlayerCount() + " Players online.";
    }

    public void UpdateRoomCount(int roomCount)
    {
        this.roomCount.text = roomCount + " open Saloons";
    }
    
    public void UpdateRoomList(string roomNames)
    {
        roomList.text = roomNames;
    }

}
