using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Screen")]
    public GameObject _mainScreen = null;
    public GameObject _lobbyScreen = null;

    [Header("Main Screen")]
    public Button _enterButton = null;

    [Header("Lobby Screen")]
    public List<TextMeshProUGUI> _playerNameList = null;
    public List<Image> _readyCheckList = null;
    public List<Image> _readyCheckBoxList = null;
    public Button _readyOrPlayButton = null;
    public TextMeshProUGUI _buttonName = null;
    public LobbyChatSystem _chatSystem = null;

    [Header("Game Info")]
    [SerializeField] TextMeshProUGUI _networkConnectionInfo = null;
    [SerializeField] TextMeshProUGUI _gameVersionText = null;
    [SerializeField] private string _sceneName = "";
    [SerializeField] private int _readyPlayers = 0;
    [SerializeField] private int _playerIdx = 0;
    [SerializeField] private bool _isReady = false;

    private void Start()
    {
        //������ ����Ǳ� ������ enter ��ư ��Ȱ��ȭ
        _enterButton.interactable = false;
        _isReady = false;
        _gameVersionText.text =  "Version : " + PhotonNetwork.AppVersion;
    }

    private void Update()
    {
        _networkConnectionInfo.text = PhotonNetwork.NetworkClientState.ToString();

        if (PhotonNetwork.InRoom)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnLeaveButton();
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        //������ ����Ǹ� enter��ư Ȱ��ȭ
        _enterButton.interactable = true;
    }

    public void SetScreen(GameObject screen)
    {
        //��� ȭ�� DIsable
        _mainScreen.SetActive(false);
        _lobbyScreen.SetActive(false);

        //Ȱ��ȭ �ϴ� ȭ�鸸 Enable
        screen.SetActive(true);
    }

    public void OnUpdatePlayerNameInput(TMP_InputField nameInput)
    {
        PhotonNetwork.NickName = nameInput.text;            //�÷��̾� �г��� ����
    }
    public void OnEnterButton()
    {
        if (PhotonNetwork.LocalPlayer.NickName == "")
        {
            Debug.Log("���ӿ� ���� �г����� �Է����ּ���.");
            return;
        }

        NetworkManager.s_instance.CreateOrJoinRoom();
    }
    public void OnEndEditEvent()
    {
        if (PhotonNetwork.LocalPlayer.NickName == "")
        {
            Debug.Log("���ӿ� ���� �г����� �Է����ּ���.");
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            NetworkManager.s_instance.CreateOrJoinRoom();
        }
    }
    public override void OnJoinedRoom()
    {
        //Room�� ���� �� lobbyScreen���� ��ȯ
        SetScreen(_lobbyScreen);
        if (PhotonNetwork.IsMasterClient)
        {
            _isReady = true;
            _readyPlayers = 1;
        }
        _playerIdx = PhotonNetwork.CurrentRoom.PlayerCount;

        _chatSystem.ClearChatText();
        string msg = "GameVersion is " + PhotonNetwork.GameVersion;
        _chatSystem.SendSystemMessage(msg);
        if (PhotonNetwork.CurrentRoom.PlayerCount % 2 != 0)
        {
            _chatSystem.SendSystemMessage("Ȧ�� �ο� ���� �� ������� �ڵ����� AI�� �����˴ϴ�.");
        }
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _readyPlayers = _isReady ? _readyPlayers : _readyPlayers + 1;
            _isReady = true;
        }
        UpdateLobbyUI();
    }

    //LobbyScreen�� PlayerName�� ������ input������ ������Ʈ
    [PunRPC]
    private void UpdateLobbyUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _buttonName.text = "����";
        }
        else
        {
            _buttonName.text = "�غ�";
        }

        for (int playerNum = 0; playerNum < PhotonNetwork.CurrentRoom.MaxPlayers; playerNum++)
        {
            if (playerNum < PhotonNetwork.PlayerList.Length)
            {
                _playerNameList[playerNum].text = PhotonNetwork.PlayerList[playerNum].NickName;
                _readyCheckBoxList[playerNum].gameObject.SetActive(true);
                if (_playerIdx - 1 == playerNum)
                {
                    photonView.RPC("ShowReadyState", RpcTarget.All, playerNum, _isReady);
                }
            }
            else
            {
                _playerNameList[playerNum].text = "...";
                _readyCheckList[playerNum].gameObject.SetActive(false);
                _readyCheckBoxList[playerNum].gameObject.SetActive(false);
            }
        }
    }

    public void OnReadyOrPlayButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (_readyPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                NetworkManager.s_instance.photonView.RPC("ChangeScene", RpcTarget.All, _sceneName);
            }
            else
            {
                _chatSystem.SendSystemMessage("��� �÷��̾ �غ� ���� �ʾҽ��ϴ�!");
            }
        }
        else
        {
            _isReady = !_isReady;
            photonView.RPC("UpdateReadyState", RpcTarget.MasterClient, _isReady);
            photonView.RPC("UpdateLobbyUI", RpcTarget.All);
        }
    }
    [PunRPC]
    public void OnLeaveButton()
    {
        if (_isReady)
        {
            photonView.RPC("UpdateReadyState", RpcTarget.MasterClient, !_isReady);
        }
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SendReadyCntToOthers", RpcTarget.All, _readyPlayers);
        }
        photonView.RPC("UpdatePlayerIdx", RpcTarget.Others, _playerIdx);
        _isReady = false;
        PhotonNetwork.LeaveRoom();
        SetScreen(_mainScreen);
    }
    //Ready���¿� ���� readyPlayer�� ������Ʈ
    [PunRPC]
    private void UpdateReadyState(bool isReady)
    {
        if (isReady)
        {
            _readyPlayers++;
        }
        else
        {
            _readyPlayers--;
        }
    }
    //Ready���¿� ���� Player�� ���� ����
    [PunRPC]
    private void ShowReadyState(int idx, bool isReady)
    {
        _readyCheckList[idx].gameObject.SetActive(isReady);
    }
    //�����Ͱ� ���� ��, �����ϴ� readyPlayer���� �Ѱ��ְ� ����
    [PunRPC]
    private void SendReadyCntToOthers(int readyCnt)
    {
        _readyPlayers = readyCnt;
    }
    //�÷��̾ ���� ��, ������ �÷��̾��� idx���� ���� �÷��̾��� idx�� �ϳ��� ����
    [PunRPC]
    private void UpdatePlayerIdx(int idx)
    {
        if (_playerIdx > idx)
        {
            _playerIdx--;
        }
    }
}