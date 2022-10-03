using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class DebugUI : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public Button btnUIToggle;
    public GameObject backPanel;
    public TextMeshProUGUI txtUIToggle;

    [Header("Debug List")]
    public TextMeshProUGUI txtConnectionInfo;
    public TextMeshProUGUI txtLobbyorRoom;
    public TextMeshProUGUI txtPlayerCount;
    public TextMeshProUGUI txtIsMasterClient;
    public TextMeshProUGUI txtUserID;

    private string curUserId;

    private static DebugUI instance;
    public static DebugUI Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance != null)
        {
            if(GetComponent<DebugUI>()!=instance)
                Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        backPanel.SetActive(true);
    }

    private void Update()
    {
        txtConnectionInfo.text = "ConnectionInfo: " + PhotonNetwork.NetworkClientState.ToString();
        txtIsMasterClient.text = "IsMasterClient: " + PhotonNetwork.IsMasterClient;
        
        curUserId = "UserID: " + PhotonNetwork.LocalPlayer.UserId;
        if (txtUserID.text != curUserId)
        {
            txtUserID.text = curUserId;
        }

        if (!PhotonNetwork.IsConnected)
        {
            txtLobbyorRoom.text = "LobbyorRoom: " + "Null";
            txtPlayerCount.text = "Player Count: " + "Null";
        }
        else
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                txtLobbyorRoom.text = "LobbyorRoom: " + PhotonNetwork.CurrentRoom.Name;
                txtPlayerCount.text = "Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount;
            }
            else if (PhotonNetwork.CurrentLobby != null)
            {
                txtLobbyorRoom.text = "LobbyorRoom: " + PhotonNetwork.CurrentLobby;
                txtPlayerCount.text = "Player Count: " + (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms);
            }
        }
    }

    public override void OnJoinedLobby()
    {
        
    }

    public override void OnJoinedRoom()
    {
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        txtPlayerCount.text = "Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        txtPlayerCount.text = "Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public void OnClickUIToggle()
    {
        bool bToggle = !backPanel.activeSelf;
        backPanel.SetActive(bToggle);

        if (bToggle)
        {
            txtUIToggle.text = "Close Debug UI";
        }
        else
        {
            txtUIToggle.text = "Open Debug UI";
        }
    }


}
