using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Launcher : MonoBehaviourPunCallbacks
{
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        Connect();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Kết nối thành công");
        
        base.OnConnectedToMaster();
    }

    public void Connect()
    {
        Debug.Log("Đang kết nối...");
        PhotonNetwork.GameVersion = "0.0.1";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnJoinedRoom()
    {
        StartGame();
        base.OnJoinedRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Create();
        base.OnJoinRandomFailed(returnCode, message);
    }

    public void Join()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void StartGame()
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }

    public void Create()
    {
        PhotonNetwork.CreateRoom("");
    }
}
