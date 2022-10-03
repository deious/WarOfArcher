using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPun
{
    public enum RESULT { RED, BLUE, DRAW };

    //public GameObject   _myCharacter = null;
    [SerializeField] private UIManager _uiManager = null;
    [SerializeField] private TurnManager _turnManager = null;
    [SerializeField] private MapManager _mapManager = null;
    [SerializeField] private InputManager _inputManager = null;
    [SerializeField] private CameraController _cameraController = null;
    [SerializeField] private PlayerController _playerController = null;
    public PlayerController PlayerController { get { return _playerController; } }
    [SerializeField] private _AIController _aiController = null;
    public _AIController AIController { get { return _aiController; } }
    [SerializeField] private ItemStorage _itemStorage = null;
    [SerializeField] private PlayerSpawner _playerSpawner = null;
    [SerializeField] private ChatSystem _chatSystem = null;

    [SerializeField] private const int c_deathCnt = 1;
    [SerializeField] private int _numberOfPlayer;
    public int NumberOfPlayer { get { return _numberOfPlayer; } }
    private bool _isAIExist = false;
    public bool IsAIExist { get { return _isAIExist; } }
    public int _redTeamDeathCnt;
    public int _blueTeamDeathCnt;
    public int _prevTeamDeathCnt;

    public static GameManager s_instance = null;
    private bool _isMasterClient = false;

    private HashSet<int> _damagedViewID = new HashSet<int>();

    private void Awake()
    {
        s_instance = this;
        _isMasterClient = PhotonNetwork.IsMasterClient;
        _uiManager.SetLoadingScreenText(1);

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SetRespawnSeed", RpcTarget.All, Random.Range(0, _playerSpawner._respawnSeeds.Count * _playerSpawner._respawnSeeds[0].Count));
        }
    }

    [PunRPC]
    private void SetRespawnSeed(int index)
    {
        _playerSpawner.RespawnSeedIndex = index;
    }

    private void Start()
    {
        StartCoroutine(GameSettingProcess());
    }

    IEnumerator GameSettingProcess()
    {
        while (_playerSpawner.RespawnSeedIndex < 0)
        {
            yield return new WaitForSeconds(0.1f);
        }
        _playerSpawner.SpawnPlayer();
        _mapManager.Init();

        do
        {
            Debug.Log($"총 플레이어 수:  {PhotonNetwork.CurrentRoom.PlayerCount}, 현재 스폰된 플레이어 수:  {_turnManager.GetPlayerIDList().Count}");
            _uiManager.SetLoadingScreenText(1);
            yield return new WaitForSeconds(0.3f);

            _uiManager.SetLoadingScreenText(2);
            yield return new WaitForSeconds(0.3f);

            _uiManager.SetLoadingScreenText(3);
            yield return new WaitForSeconds(0.3f);
        } while (PhotonNetwork.CurrentRoom.PlayerCount != _turnManager.GetPlayerIDList().Count);

        PlayerSettingProcess();
        if (_isAIExist)
        {
            while (!PhotonNetwork.GetPhotonView(TurnManager.s_instance.AIViewID))
            {
                yield return new WaitForSeconds(0.1f);
            }
            AISettingProcess();
        }

        _redTeamDeathCnt = c_deathCnt * _numberOfPlayer;
        _blueTeamDeathCnt = c_deathCnt * _numberOfPlayer;
        _uiManager.Init();

        _uiManager.DeactivateLoadingScreen();

        _turnManager.Init();
        _cameraController.Init();
        EnableManager();
    }
    private void PlayerSettingProcess()
    {
        List<int> num = _turnManager.GetPlayerIDList();

        for (int i = 0; i < num.Count; i++)
        {
            //_chatSystem.SendSystemMessage($"플레이어 초기화 : {num[i]}");
            PhotonNetwork.GetPhotonView(num[i]).GetComponent<DefaultCharacter>().Init();
        }

        _isAIExist = PhotonNetwork.CurrentRoom.PlayerCount % 2 != 0;
        _numberOfPlayer = PhotonNetwork.CurrentRoom.PlayerCount + (_isAIExist ? 1 : 0);

        if (PhotonNetwork.CurrentRoom.PlayerCount % 2 != 0)
        {
            _playerSpawner.SpawnAI(_isMasterClient);
        }
    }
    private void AISettingProcess()
    {
        _aiController = PhotonNetwork.GetPhotonView(TurnManager.s_instance.AIViewID).GetComponent<_AIController>();
        _aiController.Init();
    }
    private void EnableManager()
    {
        _cameraController.enabled = true;
        _inputManager.enabled = true;
    }
    public IEnumerator TurnPreProcess()
    {
        _damagedViewID.Clear();

        _inputManager.PreProcess();
        _cameraController.PreProcess();
        _itemStorage.PreProcess(_isMasterClient);

        if (IsSuddenDeathCondition())
        {
            _uiManager.StartSuddenDeath();
            _chatSystem.SendSystemMessage("물이 차오르기 시작합니다.");
            _mapManager.CallWorldEvent();
            while (_mapManager._isWaterWorldEvent)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        CheckGameFinish();

        if (_turnManager.IsAITurn)
        {
            _aiController.PreProcess();
            _uiManager.SetTurnIndicator(_numberOfPlayer - 1);
        }
        else
        {
            _playerController.CheckRespawnPlayerProcess();
            _uiManager.SetTurnIndicator(_turnManager.CurrentPlayerNum);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            _turnManager.photonView.RPC("DecreaseWorldEventCount", RpcTarget.All);
            _mapManager.UpdateWindSystem();
            _turnManager.photonView.RPC("ResetRemainTime", RpcTarget.All);
            _uiManager.photonView.RPC("PopUpUI", RpcTarget.All);
        }

        yield return new WaitForSeconds(2.0f);      // 기존 4초에서 2초로 변경

        if (PhotonNetwork.IsMasterClient)
        {
            _uiManager.photonView.RPC("PopDownUI", RpcTarget.All);
            _turnManager.photonView.RPC("SetTurnPrevTime", RpcTarget.All, PhotonNetwork.Time);
            _turnManager.photonView.RPC("TurnOnTurnTimer", RpcTarget.All);
        }

        if (PhotonNetwork.IsMasterClient && _turnManager.IsAITurn)
        {
            _aiController.AITurnStart();
            _chatSystem.SendSystemMessage("<color=blue>Alpha</color>의 턴이 시작됩니다!");
        }
        else if (!_turnManager.IsAITurn)
        {
            _playerController.PreProcess();
            _chatSystem.SendSystemMessage(_turnManager.CurrentPlayer.NickName + "의 턴이 시작됩니다!");
        }

        CalcPrevTeamDeathCnt();
    }

    public IEnumerator TurnPostProcess()
    {
        if (_turnManager.IsAITurn)
        {
            _aiController.PostProcess();
        }
        else
        {
            _inputManager.PostProcess();
            _playerController.PostProcess();
        }

        _uiManager.PostProcess();

        _chatSystem.SendSystemMessage("모든 움직임이 정지할 때 까지 대기합니다.");
        int escapeLoopCnt = 0;
        while (!_turnManager.IsTurnFinish())
        {
            Debug.Log("종료 대기");
            yield return new WaitForSeconds(1.0f);
            escapeLoopCnt++;

            if (escapeLoopCnt > 7)
            {
                Debug.Log("정지 대기 강제 탈출");
                break;
            }
        }

        DamagedPlayerProcess();
        yield return new WaitForSeconds(1.0f);
        UpdatePlayerKillCnt();

        _cameraController.PostProcess();

        CheckGameFinish();

        if (PhotonNetwork.IsMasterClient)
        {
            if (!_turnManager.IsAITurn)
            {
                _itemStorage.PostProcess();
            }
            _turnManager.EndPostProcess();
        }
    }

    private void DamagedPlayerProcess()
    {
        foreach (int viewID in _damagedViewID)
        {
            GameObject character = PhotonNetwork.GetPhotonView(viewID).gameObject;
            character.GetComponent<PlayerInfoDisplay>().ShowPlayerInfoUI();
            if (character.GetComponent<DefaultCharacter>())
            {
                SetIndexPlayerHealthUI((int)character.GetPhotonView().Owner.CustomProperties["Index"], character.GetComponent<DefaultCharacter>().InfoData._hp);
                character.GetComponent<DefaultCharacter>()._accumulateDamage = 0;
                if (character.GetComponent<DefaultCharacter>()._isDeath)
                {
                    SetIndexPlayerHealthUI((int)character.GetPhotonView().Owner.CustomProperties["Index"], 0);
                    character.transform.Find("Body").GetComponent<Animator>().SetTrigger("DieTrigger");
                }
            }
            else
            {
                _uiManager.SetIndexPlayerHealthUI(_numberOfPlayer - 1, character.GetComponent<DefaultCharacterAI>().InfoData._hp);
                character.GetComponent<DefaultCharacterAI>()._accumulateDamage = 0;
                if (character.GetComponent<DefaultCharacterAI>()._isDeath)
                {
                    SetAIHealthUI(0);
                    //TODO : AI 죽는 모션 업데이트 필요
                    character.GetComponent<DefaultCharacterAI>().ReadyRespawn();
                }
            }
        }
    }


    public void DecreaseTeamDeathCnt(string teamColor)
    {
        if (teamColor == "Red")
        {
            _redTeamDeathCnt--;
        }

        if (teamColor == "Blue")
        {
            _blueTeamDeathCnt--;
        }
    }
    public void SaveDamagedPlayer(int viewID)
    {
        _damagedViewID.Add(viewID);
    }

    public void ChangeCameraMode(CameraController.CameraMode cameraMode)
    {
        _cameraController.ChangeMode(cameraMode);
    }

    public bool GetPlayerControllerState(DefaultCharacter.PlayerState playerState)
    {
        if (!TurnManager.s_instance.CurrentCharacter)
        {
            return false;
        }

        return TurnManager.s_instance.CurrentCharacter.GetComponent<DefaultCharacter>().GetCurrentPlayerState(playerState);
    }
    public void SwitchQuickItem(int quickNumber)
    {
        _uiManager.SwitchQuickSlotItem(quickNumber);
        //SoundController.s_instance.transform.Find("ItemEquipSound").GetComponent<AudioSource>().Play();
        SoundController.s_instance._audio[3].GetComponent<AudioSource>().Play();
    }

    public void CallRespawn()
    {
        _uiManager.CallRespawnUI();
    }
    public void CallRespawn(string NickName)
    {
        _uiManager.CallRespawnUI(NickName);
        Camera.main.GetComponent<CameraController>().CeaseZoomInOut();
    }

    private bool IsSuddenDeathCondition()
    {
        return _turnManager.CurrentPlayerNum == 0 && _turnManager._remainWorldEventCnt <= 0 && !_turnManager.IsAITurn;
    }
    //승리 조건을 체크하는 함수
    private void CheckGameFinish()
    {
        if (_redTeamDeathCnt <= 0 && _blueTeamDeathCnt <= 0)
        {
            SoundController.s_instance._audio[6].GetComponent<AudioSource>().Play();
            FinishGame(RESULT.DRAW);
        }
        else if (_redTeamDeathCnt <= 0)
        {
            if ((string)PhotonNetwork.LocalPlayer.CustomProperties["TeamColor"] == "Blue")
            {
                SoundController.s_instance._audio[4].GetComponent<AudioSource>().Play();
            }
            else
            {
                SoundController.s_instance._audio[5].GetComponent<AudioSource>().Play();
            }
            FinishGame(RESULT.BLUE);
        }
        else if (_blueTeamDeathCnt <= 0)
        {
            if ((string)PhotonNetwork.LocalPlayer.CustomProperties["TeamColor"] == "Red")
            {
                SoundController.s_instance._audio[4].GetComponent<AudioSource>().Play();
            }
            else
            {
                SoundController.s_instance._audio[5].GetComponent<AudioSource>().Play();
            }
            FinishGame(RESULT.RED);
        }
    }

    //게임 종료
    private void FinishGame(RESULT winTeam)
    {
        _playerController.enabled = false;
        _cameraController.enabled = false;
        _inputManager.enabled = false;
        _turnManager.enabled = false;
        if (PhotonNetwork.IsMasterClient)
        {
            SendPlayerKDResult();
        }
        _uiManager.DisplayResultScreen(winTeam);
        _uiManager.enabled = false;
        _mapManager.enabled = false;
    }


    private void CalcPrevTeamDeathCnt()
    {
        _prevTeamDeathCnt = _redTeamDeathCnt + _blueTeamDeathCnt;
    }

    private void UpdatePlayerKillCnt()
    {
        if (_turnManager.IsAITurn)
        {
            DefaultCharacterAI aiCharacter = _aiController.gameObject.GetComponent<DefaultCharacterAI>();
            aiCharacter._killCnt += _prevTeamDeathCnt - (_redTeamDeathCnt + _blueTeamDeathCnt);

            //본인을 죽인 것은 킬카운트 계산하지 않음
            if (aiCharacter._isDeath)
            {
                aiCharacter._killCnt--;
            }
            _chatSystem.SendSystemMessage($"<color=blue>Alpha</color> : Kill :  {aiCharacter._killCnt} / Death : {aiCharacter._deathCnt}");

        }
        else
        {
            _turnManager.CurrentPlayer.CustomProperties["KillCnt"] = (int)_turnManager.CurrentPlayer.CustomProperties["KillCnt"] + (_prevTeamDeathCnt - (_redTeamDeathCnt + _blueTeamDeathCnt));

            //본인을 죽인 것은 킬카운트 계산하지 않음
            if (_turnManager.CurrentCharacter.GetComponent<DefaultCharacter>()._isDeath)
            {
                _turnManager.CurrentPlayer.CustomProperties["KillCnt"] = (int)_turnManager.CurrentPlayer.CustomProperties["KillCnt"] - 1;
            }
            _chatSystem.SendSystemMessage($"{_turnManager.CurrentPlayer.NickName} : Kill : {_turnManager.CurrentPlayer.CustomProperties["KillCnt"]} / Death : {_turnManager.CurrentPlayer.CustomProperties["DeathCnt"]}");
        }
    }
    private void SendPlayerKDResult()
    {
        for (int idx = 0; idx < NumberOfPlayer; idx++)
        {
            if (PhotonNetwork.PlayerList.Length == idx)
            {
                photonView.RPC("SendAIEnemyKD", RpcTarget.Others, _aiController.gameObject.GetComponent<DefaultCharacterAI>()._killCnt, _aiController.gameObject.GetComponent<DefaultCharacterAI>()._deathCnt);
                return;
            }
            photonView.RPC("SendIdxPlayerKD", RpcTarget.Others, idx, (int)PhotonNetwork.PlayerList[idx].CustomProperties["KillCnt"], (int)PhotonNetwork.PlayerList[idx].CustomProperties["DeathCnt"]);
        }
    }

    [PunRPC]
    private void SendIdxPlayerKD(int idx, int kill, int death)
    {
        PhotonNetwork.PlayerList[idx].CustomProperties["KillCnt"] = kill;
        PhotonNetwork.PlayerList[idx].CustomProperties["DeathCnt"] = death;
    }

    [PunRPC]
    private void SendAIEnemyKD(int kill, int death)
    {
        _aiController.gameObject.GetComponent<DefaultCharacterAI>()._killCnt = kill;
        _aiController.gameObject.GetComponent<DefaultCharacterAI>()._deathCnt = death;
    }

    public void RefreshCurrentPlayerUI()
    {
        _uiManager.SetIndexPlayerHealthUI((int)_turnManager.CurrentPlayer.CustomProperties["Index"], 200);
    }
    public void RefreshAIUI()
    {
        _uiManager.SetIndexPlayerHealthUI(_numberOfPlayer - 1, 200);
    }
    public void SetIndexPlayerHealthUI(int idx, int health)
    {
        _uiManager.SetIndexPlayerHealthUI(idx, health);
    }
    public void SetAIHealthUI(int health)
    {
        _uiManager.SetIndexPlayerHealthUI(_numberOfPlayer - 1, health);
    }
    [PunRPC]
    public void StartTurnTimerUI()
    {
        _uiManager.TurnTimerUIStart();
    }
}