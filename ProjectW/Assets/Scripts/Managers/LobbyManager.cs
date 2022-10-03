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
        //서버에 연결되기 전까지 enter 버튼 비활성화
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
        //서버에 연결되면 enter버튼 활성화
        _enterButton.interactable = true;
    }

    public void SetScreen(GameObject screen)
    {
        //모든 화면 DIsable
        _mainScreen.SetActive(false);
        _lobbyScreen.SetActive(false);

        //활성화 하는 화면만 Enable
        screen.SetActive(true);
    }

    public void OnUpdatePlayerNameInput(TMP_InputField nameInput)
    {
        PhotonNetwork.NickName = nameInput.text;            //플레이어 닉네임 설정
    }
    public void OnEnterButton()
    {
        if (PhotonNetwork.LocalPlayer.NickName == "")
        {
            Debug.Log("게임에 사용될 닉네임을 입력해주세요.");
            return;
        }

        NetworkManager.s_instance.CreateOrJoinRoom();
    }
    public void OnEndEditEvent()
    {
        if (PhotonNetwork.LocalPlayer.NickName == "")
        {
            Debug.Log("게임에 사용될 닉네임을 입력해주세요.");
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            NetworkManager.s_instance.CreateOrJoinRoom();
        }
    }
    public override void OnJoinedRoom()
    {
        //Room에 입장 시 lobbyScreen으로 전환
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
            _chatSystem.SendSystemMessage("홀수 인원 시작 시 블루팀에 자동으로 AI가 생성됩니다.");
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

    //LobbyScreen의 PlayerName을 각각의 input값으로 업데이트
    [PunRPC]
    private void UpdateLobbyUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _buttonName.text = "시작";
        }
        else
        {
            _buttonName.text = "준비";
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
                _chatSystem.SendSystemMessage("모든 플레이어가 준비 되지 않았습니다!");
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
    //Ready상태에 따라 readyPlayer값 업데이트
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
    //Ready상태에 따라 Player의 상태 변경
    [PunRPC]
    private void ShowReadyState(int idx, bool isReady)
    {
        _readyCheckList[idx].gameObject.SetActive(isReady);
    }
    //마스터가 나갈 때, 관리하던 readyPlayer값을 넘겨주고 나감
    [PunRPC]
    private void SendReadyCntToOthers(int readyCnt)
    {
        _readyPlayers = readyCnt;
    }
    //플레이어가 나갈 때, 나가는 플레이어의 idx보다 높은 플레이어의 idx를 하나씩 낮춤
    [PunRPC]
    private void UpdatePlayerIdx(int idx)
    {
        if (_playerIdx > idx)
        {
            _playerIdx--;
        }
    }
}