using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager s_instance = null;
    private bool _isGameStarted = false;

    private void Awake()
    {
        s_instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();           //Photon Cloud에 연결되는 시작 지점
        PhotonNetwork.GameVersion = Application.version;
    }

    //private void Update()
    //{
    //    Debug.Log(PhotonNetwork.NetworkClientState.ToString());
    //}

    public void CreateOrJoinRoom()
    {
        PhotonNetwork.JoinRandomRoom();

        //if (PhotonNetwork.CountOfRooms > 0)
        //{
        //    Debug.Log("CountOfRoom > 0");
        //    PhotonNetwork.JoinRoom("testRoom");         //string 이름의 room에 연결
        //}
        //else
        //{
        //    Debug.Log("CountofRoom == 0");
        //    RoomOptions options = new RoomOptions();
        //    options.MaxPlayers = 8;
        //    PhotonNetwork.CreateRoom("testRoom", options);  //option 설정의 string 이름의 room 생성
        //}
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        CreateNewRoom();
    }

    private void CreateNewRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 8;
        string roomName = "ProjectW" + PhotonNetwork.CountOfRooms;
        PhotonNetwork.CreateRoom(roomName, roomOptions);
        Debug.Log(PhotonNetwork.CountOfRooms);
    }

    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        if (!_isGameStarted)
        {
            _isGameStarted = true;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            InitPlayerProperties();
            PhotonNetwork.LoadLevel(sceneName);
        }
    }

    private void InitPlayerProperties()
    {
        for (int idx = 0; idx < PhotonNetwork.CurrentRoom.PlayerCount; idx++)
        {
            Hashtable _playerProperty = new Hashtable();
            _playerProperty["KillCnt"] = 0;
            _playerProperty["DeathCnt"] = 0;
            _playerProperty["Index"] = idx;
            if (idx % 2 == 0)
            {
                _playerProperty["TeamColor"] = "Red";
                PhotonNetwork.PlayerList[idx].NickName = "<color=red>" + PhotonNetwork.PlayerList[idx].NickName + "</color>";
            }
            else
            {
                _playerProperty["TeamColor"] = "Blue";
                PhotonNetwork.PlayerList[idx].NickName = "<color=blue>" + PhotonNetwork.PlayerList[idx].NickName + "</color>";
            }
            PhotonNetwork.PlayerList[idx].CustomProperties = _playerProperty;
        }
    }
}