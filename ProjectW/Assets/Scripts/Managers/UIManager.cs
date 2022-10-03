using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class UIManager : MonoBehaviourPun
{
    [Header("Loading Screen")]
    public GameObject _loadingScreen;
    public TextMeshProUGUI _loadingScreenText;

    [Header("Notification")]
    public GameObject _notificationUI = null;
    public TextMeshProUGUI _currentPlayerNameNoti = null;
    public TextMeshProUGUI _cuurentPlayerRespawn = null;
    public TextMeshProUGUI _suddenDeathCount = null;
    public TextMeshProUGUI _suddenDeathStart = null;
    public TextMeshProUGUI _WindDirection = null;


    [Header("Environment")]
    public GameObject _environmentUI = null;
    public TextMeshProUGUI _turnTimer = null;
    public ShowWindUI _showWind = null;
    public TextMeshProUGUI _currentPlayerNameTop = null;


    [Header("PlayersInfo")]
    public RespawnUI _playerRespawnUI = null;
    public GameObject _playersInfoUI = null;
    public GameObject _tabGuide = null;
    public List<TextMeshProUGUI> _playerNameList = null;
    public List<Slider> _playerHPSlider = null;
    public List<Image> _turnIndicatorList = null;
    public List<Image> _redTeamDeathCnt = null;
    public List<Image> _blueTeamDeathCnt = null;
    public bool _isActivePlayersInfo = false;

    [Header("ReactiveUI")]
    public GameObject _reactiveUI = null;
    public QuickSlot _quickSlot = null;
    //public Animator _itemInfoAnimator = null;

    [Header("ChatUI")]
    public GameObject _ChatUI = null;

    [Header("Result Screen")]
    public GameObject _resultScreen = null;
    public TextMeshProUGUI _winnerTitle = null;
    public TextMeshProUGUI _loserTitle = null;
    public GameObject _drawTitle = null;
    public List<TextMeshProUGUI> _winnerPlayerList = null;
    public List<TextMeshProUGUI> _winnerPlayerKD = null;
    public List<TextMeshProUGUI> _loserPlayerList = null;
    public List<TextMeshProUGUI> _loserPlayerKD = null;

    [Header("GameExit")]
    public Button _exitButton = null;

    public void Init()
    {
        InitializePlayerList();
        InitializePlayerHPSlider();
        _ChatUI.SetActive(false);
        _notificationUI.SetActive(false);
        _reactiveUI.SetActive(true);
        _environmentUI.SetActive(false);
        _playersInfoUI.SetActive(false);
        _tabGuide.SetActive(false);
        _playerRespawnUI.gameObject.SetActive(false);
        _quickSlot.Init();
    }

    private void InitializePlayerList()
    {
        for (int idx = 0; idx < GameManager.s_instance.NumberOfPlayer; idx++)
        {
            if (idx == PhotonNetwork.PlayerList.Length)
            {
                _playerNameList[idx].text = "<color=blue>Alpha</color>";
            }
            else
            {
                _playerNameList[idx].text = PhotonNetwork.PlayerList[idx].NickName;
            }
            _playerHPSlider[idx].gameObject.SetActive(true);
        }
    }
    private void InitializePlayerHPSlider()
    {
        for (int idx = 0; idx < GameManager.s_instance.NumberOfPlayer; idx++)
        {
            _playerHPSlider[idx].value = 200;
        }
    }

    [PunRPC]
    public void PopUpUI()
    {
        _currentPlayerNameTop.text = TurnManager.s_instance.CurrentPlayer.NickName;
        if (TurnManager.s_instance.IsAITurn)
        {
            _currentPlayerNameTop.text = "<color=blue>Alpha</color>";
        }
        _playersInfoUI.SetActive(true);
        _tabGuide.SetActive(false);
        SetNotificationUI();
    }
    [PunRPC]
    public void PopDownUI()
    {
        SetCurrentPlayerActiveUI();
    }

    public void TurnTimerUIStart()
    {
        _environmentUI.SetActive(true);
    }


    public void PostProcess()
    {
        _environmentUI.SetActive(false);
        _quickSlot.RefreshSelection();
        _isActivePlayersInfo = false;
        ClearTurnIndicator();
        //_itemInfoAnimator.SetBool("IsActive", false);
    }
    // Update is called once per frame
    private void Update()
    {
        UpdateTurnTimerUI();
        UpdateDeathCnt(_redTeamDeathCnt, GameManager.s_instance._redTeamDeathCnt);
        UpdateDeathCnt(_blueTeamDeathCnt, GameManager.s_instance._blueTeamDeathCnt);

        if (_isActivePlayersInfo)
        {
            _playersInfoUI.SetActive(InputManager.s_instance._tabKey);
            _tabGuide.SetActive(!InputManager.s_instance._tabKey);
        }
    }

    private void UpdateDeathCnt(List<Image> images, int cnt)
    {
        for (int i = 0; i < images.Count; i++)
        {
            if (i < cnt)
            {
                images[i].enabled = true;
            }
            else
            {
                images[i].enabled = false;
            }
        }
    }

    public void SetCurrentPlayerActiveUI()
    {
        _notificationUI.SetActive(false);
        _playersInfoUI.SetActive(false);
        _tabGuide.SetActive(true);
        _isActivePlayersInfo = true;
    }

    public void SetLoadingScreenText(int timeCount)
    {
        _loadingScreen.SetActive(true);
        timeCount = timeCount % 4;
        //. ������ 3������ ����
        _loadingScreenText.text = "�ε���";
        for (int count = 0; count < timeCount; count++)
        {
            _loadingScreenText.text += ".";
        }
    }
    public void DeactivateLoadingScreen()
    {
        _loadingScreen.SetActive(false);
        _ChatUI.SetActive(true);
    }

    public void StartSuddenDeath()
    {
        _suddenDeathStart.text = " <color=#3333FF>��</color> �� �������� �����մϴ�"; //6BDCD0
        _suddenDeathStart.transform.parent.gameObject.SetActive(true);
    }

    public void CallRespawnUI(string NickName = "Alpha")
    {
        if (_playerRespawnUI.gameObject.activeSelf)
        {
            _playerRespawnUI._isStartRespawn = false;
            _playerRespawnUI.gameObject.SetActive(false);
            return;
        }

        _playerRespawnUI._isStartRespawn = true;
        _playerRespawnUI.gameObject.SetActive(true);

        if (NickName == "Alpha")
        {
            _playerRespawnUI.SetRespawnPlayerInfo();
            return;
        }

        _playerRespawnUI.SetRespawnPlayerInfo(NickName);
    }

    private void SetNotificationUI()
    {
        _notificationUI.SetActive(true);
        SetCurrentPlayerNameUI();
        if (GameManager.s_instance.PlayerController.IsRespawn && !TurnManager.s_instance.IsAITurn)
        {
            Debug.Log("������ ����");
            _cuurentPlayerRespawn.text = TurnManager.s_instance.CurrentPlayer.NickName + "�� �������� �����ؾ� �մϴ�";
        }
        else if (GameManager.s_instance.AIController.IsRespawn && TurnManager.s_instance.IsAITurn)
        {
            _cuurentPlayerRespawn.text = "<color=blue>Alpha</color>�� �������� �����ؾ� �մϴ�";
        }
        else
        {
            _cuurentPlayerRespawn.text = "";
        }
        SetEnvironmentUI();
    }

    private void UpdateTurnTimerUI()
    {
        _turnTimer.text = "" + (int)TurnManager.s_instance.RemainTime;
    }
    private void SetCurrentPlayerNameUI()
    {
        if (TurnManager.s_instance.IsAITurn)
        {
            _currentPlayerNameNoti.text = "�̹� �� �����ڴ� <color=blue>Alpha</color> �Դϴ�.";
            return;
        }

        _currentPlayerNameNoti.text = "�̹� �� �����ڴ� " + TurnManager.s_instance.CurrentPlayer.NickName + " �Դϴ�";
    }
    private void SetEnvironmentUI()
    {
        _suddenDeathStart.transform.parent.gameObject.SetActive(false);
        if (TurnManager.s_instance._remainWorldEventCnt > 0)
        {
            _suddenDeathCount.text = "<color=#3333FF>��</color>�� ������ �� ���� " + TurnManager.s_instance._remainWorldEventCnt + "�� ���ҽ��ϴ�";
        }
        else
        {
            _suddenDeathCount.text = "";
        }
        if (MapManager.s_instance.windLevel == 0)
        {
            _WindDirection.text = "<color=#66FFFF>�ٶ�</color>�� <color=red>����</color> �ʽ��ϴ�";
        }
        else
        {
            switch (Mathf.Abs(MapManager.s_instance.windLevel))
            {
                case 1:
                    _WindDirection.text = "<color=#66FFFF>�ٶ�</color>�� <color=red>���ϰ� �Ұ�</color> �ֽ��ϴ�";
                    break;
                case 2:
                    _WindDirection.text = "<color=#66FFFF>�ٶ�</color>�� <color=red>�Ұ�</color> �ֽ��ϴ�";
                    break;
                case 3:
                    _WindDirection.text = "<color=#66FFFF>�ٶ�</color>�� <color=red>���ϰ� �Ұ�</color> �ֽ��ϴ�";
                    break;
                default:
                    _WindDirection.text = "<color=#D10000>�ٶ� �ý����� �̻��մϴ�</color>";
                    break;
            }
        }
        _showWind.gameObject.GetPhotonView().RPC("UpdateWindUI", RpcTarget.AllBufferedViaServer, MapManager.s_instance.windLevel);
    }
    public void DisplayResultScreen(GameManager.RESULT winTeam)
    {
        switch (winTeam)
        {
            case GameManager.RESULT.RED:
                for (int i = 0; i < GameManager.s_instance.NumberOfPlayer / 2; i++)
                {
                    _winnerPlayerList[i].text = PhotonNetwork.PlayerList[2 * i].NickName;
                    _winnerPlayerKD[i].text = (int)PhotonNetwork.PlayerList[2 * i].CustomProperties["KillCnt"] + " / " + (int)PhotonNetwork.PlayerList[2 * i].CustomProperties["DeathCnt"];
                    if (2 * i + 1 == PhotonNetwork.PlayerList.Length)
                    {
                        _loserPlayerList[i].text = "<color=blue>Alpha</color>";
                        _loserPlayerKD[i].text = $"{GameManager.s_instance.AIController.gameObject.GetComponent<DefaultCharacterAI>()._killCnt} / {GameManager.s_instance.AIController.gameObject.GetComponent<DefaultCharacterAI>()._deathCnt}";
                    }
                    else
                    {
                        _loserPlayerList[i].text = PhotonNetwork.PlayerList[2 * i + 1].NickName;
                        _loserPlayerKD[i].text = (int)PhotonNetwork.PlayerList[2 * i + 1].CustomProperties["KillCnt"] + " / " + (int)PhotonNetwork.PlayerList[2 * i + 1].CustomProperties["DeathCnt"];
                    }
                }
                _winnerTitle.color = Color.red;
                _loserTitle.color = Color.blue;
                _winnerTitle.gameObject.SetActive(true);
                _loserTitle.gameObject.SetActive(true);
                break;
            case GameManager.RESULT.BLUE:
                for (int i = 0; i < GameManager.s_instance.NumberOfPlayer / 2; i++)
                {
                    if (2 * i + 1 == PhotonNetwork.PlayerList.Length)
                    {
                        _winnerPlayerList[i].text = "<color=blue>Alpha</color>";
                        _winnerPlayerKD[i].text = $"{GameManager.s_instance.AIController.gameObject.GetComponent<DefaultCharacterAI>()._killCnt} / {GameManager.s_instance.AIController.gameObject.GetComponent<DefaultCharacterAI>()._deathCnt}";
                    }
                    else
                    {
                        _winnerPlayerList[i].text = PhotonNetwork.PlayerList[2 * i + 1].NickName;
                        _winnerPlayerKD[i].text = (int)PhotonNetwork.PlayerList[2 * i + 1].CustomProperties["KillCnt"] + " / " + (int)PhotonNetwork.PlayerList[2 * i + 1].CustomProperties["DeathCnt"];
                    }
                    _loserPlayerList[i].text = PhotonNetwork.PlayerList[2 * i].NickName;
                    _loserPlayerKD[i].text = (int)PhotonNetwork.PlayerList[2 * i].CustomProperties["KillCnt"] + " / " + (int)PhotonNetwork.PlayerList[2 * i].CustomProperties["DeathCnt"];
                }
                _winnerTitle.color = Color.blue;
                _loserTitle.color = Color.red;
                _winnerTitle.gameObject.SetActive(true);
                _loserTitle.gameObject.SetActive(true);
                break;
            case GameManager.RESULT.DRAW:
                for (int i = 0; i < GameManager.s_instance.NumberOfPlayer / 2; i++)
                {
                    _winnerPlayerList[i].text = PhotonNetwork.PlayerList[2 * i].NickName;
                    _winnerPlayerKD[i].text = (int)PhotonNetwork.PlayerList[2 * i].CustomProperties["KillCnt"] + " / " + (int)PhotonNetwork.PlayerList[2 * i].CustomProperties["DeathCnt"];
                    if (2 * i + 1 == PhotonNetwork.PlayerList.Length)
                    {
                        _loserPlayerList[i].text = "<color=blue>Alpha</color>";
                        _loserPlayerKD[i].text = $"{GameManager.s_instance.AIController.gameObject.GetComponent<DefaultCharacterAI>()._killCnt} / {GameManager.s_instance.AIController.gameObject.GetComponent<DefaultCharacterAI>()._deathCnt}";
                    }
                    else
                    {
                        _loserPlayerList[i].text = PhotonNetwork.PlayerList[2 * i + 1].NickName;
                        _loserPlayerKD[i].text = (int)PhotonNetwork.PlayerList[2 * i + 1].CustomProperties["KillCnt"] + " / " + (int)PhotonNetwork.PlayerList[2 * i + 1].CustomProperties["DeathCnt"];
                    }
                }
                _drawTitle.SetActive(true);
                break;
            default:
                Debug.LogError("���������� ���� ���");
                break;
        }
        _resultScreen.SetActive(true);
    }
    public void OnExitButton()
    {
        Application.Quit();
    }

    public void SwitchQuickSlotItem(int quickNumber)
    {
        _quickSlot.Slots[quickNumber].ItemClick();
        //_itemInfoAnimator.SetBool("IsActive", true);
    }
    public void ClearTurnIndicator()
    {
        for (int i = 0; i < _turnIndicatorList.Count; i++)
        {
            _turnIndicatorList[i].gameObject.SetActive(false);
        }
    }
    public void SetTurnIndicator(int idx)
    {
        _turnIndicatorList[idx].gameObject.SetActive(true);
    }

    public void SetIndexPlayerHealthUI(int idx, int health)
    {
        _playerHPSlider[idx].value = health;
    }
}