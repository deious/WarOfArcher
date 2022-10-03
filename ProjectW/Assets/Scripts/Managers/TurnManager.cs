using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TurnManager : MonoBehaviourPun
{
    private Player _currentPlayer = null;
    public Player CurrentPlayer { get { return _currentPlayer; } }
    [SerializeField] private int _currentPlayerNum = 0;
    [SerializeField] private List<int> _playerViewIDList = null;
    public List<int> PlayerViewIDList { get { return _playerViewIDList; } }
    [SerializeField] private int _aiViewID;
    public int AIViewID { get { return _aiViewID; } }

    [SerializeField] private GameObject _currentCharacter = null;
    public GameObject CurrentCharacter { get { return _currentCharacter; } }
    public int CurrentPlayerNum { get { return _currentPlayerNum; } }

    [SerializeField] private double _remainTime;                //턴에 남은 시간 체크
    public double RemainTime { get { return _remainTime; } }
    public bool _turnTimer = false;           //타이머 on/off
    [SerializeField] private const int c_offsetTime = 5;
    [SerializeField] private double _maxTime = 30.0;            //한 턴의 시간
    private double _prevTime = 0f;                //턴이 시작된 시점
    private double _deltaTime = 0f;              //턴이 지난 시간

    public int _remainWorldEventCnt = 0;               // 남은 월드 이벤트 카운트
    public bool _isTurnFinished = true;

    [SerializeField] private bool _isAITurn = false;
    public bool IsAITurn { get { return _isAITurn; } }

    public static TurnManager s_instance = null;
    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError("턴 매니저 생성 에러");
            Destroy(this.gameObject);
            return;
        }
        s_instance = this;
        _currentPlayerNum = 0;
        _currentPlayer = PhotonNetwork.PlayerList[_currentPlayerNum];        //첫 턴은 0번째 플레이어
    }

    private void Update()
    {
        if (_isTurnFinished)
        {
            return;
        }

        if (_turnTimer)
        {
            _deltaTime = PhotonNetwork.Time - _prevTime;
            _prevTime = PhotonNetwork.Time;
            _remainTime = _remainTime > 0 ? _remainTime - _deltaTime : 0;
        }

        if (_currentPlayer.IsLocal && _remainTime <= 1)
        {
            photonView.RPC("EndTurn", RpcTarget.All);
        }
    }

    public void Init()
    {
        _currentPlayerNum = 0;
        _currentPlayer = PhotonNetwork.PlayerList[_currentPlayerNum];

        if (_currentPlayer.IsLocal)                                         //0번째 플레이어를 로컬로 가지는 클라이언트에서 턴을 실행
        {
            if (PhotonNetwork.GetPhotonView(_playerViewIDList[_currentPlayerNum]))
            {
                photonView.RPC("SetCurrentCharacter", RpcTarget.AllBufferedViaServer, _playerViewIDList[_currentPlayerNum]);
            }
            else
            {
                Debug.LogError("해당 플레이어의 포톤 뷰 아이디가 비어있습니다");
            }
            photonView.RPC("BeginTurn", RpcTarget.AllBufferedViaServer);
        }
    }
    [PunRPC]
    public void DecreaseWorldEventCount()
    {
        if (!IsAITurn)
        {
            _remainWorldEventCnt -= 1;
            if (_remainWorldEventCnt < 0)
            {
                _remainWorldEventCnt = 0;
            }
        }
    }
    //턴을 돌리는 함수
    [PunRPC]
    private void SetNextTurn()
    {
        _currentPlayerNum = (_currentPlayerNum + 1) % PhotonNetwork.CurrentRoom.PlayerCount;
        _currentPlayer = PhotonNetwork.PlayerList[_currentPlayerNum];     //다음 플레이어에게 턴 넘김

        if (PhotonNetwork.IsMasterClient)
        {
            if(PhotonNetwork.GetPhotonView(_playerViewIDList[_currentPlayerNum]))
            {
                photonView.RPC("SetCurrentCharacter", RpcTarget.AllBufferedViaServer, _playerViewIDList[_currentPlayerNum]);
            }
            else
            {
                Debug.LogError("해당 플레이어의 포톤 뷰 아이디가 비어있습니다");
            }
            photonView.RPC("BeginTurn", RpcTarget.AllBufferedViaServer);
        }
    }

    //현재 턴의 캐릭터를 _currentCharacter로 찾아옴
    [PunRPC]
    private void SetCurrentCharacter(int viewID)
    {
        _currentCharacter = PhotonNetwork.GetPhotonView(viewID).gameObject;
    }

    [PunRPC]
    private void BeginTurn()
    {
        StartCoroutine(GameManager.s_instance.TurnPreProcess());
    }

    [PunRPC]
    private void EndTurn()
    {
        _remainTime = 0;
        _isTurnFinished = true;
        _turnTimer = false;
        StartCoroutine(GameManager.s_instance.TurnPostProcess());
    }

    public void EndPostProcess()
    {
        if (CheckAITurn())
        {
            photonView.RPC("SetIsAITurn", RpcTarget.All, _isAITurn);
            photonView.RPC("BeginTurn", RpcTarget.AllBufferedViaServer);
        }
        else
        {
            photonView.RPC("SetIsAITurn", RpcTarget.All, _isAITurn);
            photonView.RPC("SetNextTurn", RpcTarget.AllBufferedViaServer);
        }
    }

    public bool IsTurnFinish()
    {
        for (int i = 0; i < GameManager.s_instance.NumberOfPlayer; i++)
        {
            bool value = false;
            if (i == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                value = (PhotonNetwork.GetPhotonView(_aiViewID).gameObject.GetComponent<Rigidbody2D>().velocity == Vector2.zero);
            }
            else
            {
                value = (PhotonNetwork.GetPhotonView(_playerViewIDList[i]).gameObject.GetComponent<Rigidbody2D>().velocity == Vector2.zero);
            }

            if (value == false)
            {
                return false;
            }
        }

        foreach (Transform bullet in ItemStorage.s_instance.BulletStorage)
        {
            if(bullet.gameObject.activeSelf)
            {
                return false;
            }
        }
        return true;
    }

    //턴타이머 세팅
    [PunRPC]
    public void ResetRemainTime()
    {
        _remainTime = _maxTime;
    }
    [PunRPC]
    public void SetTurnPrevTime(double startTime)
    {
        _prevTime = startTime;
    }
    [PunRPC]
    public void TurnOnTurnTimer()
    {
        _isTurnFinished = false;
        _turnTimer = true;
    }
    public void DecreaseRemainTime(int setRemainTime = c_offsetTime)
    {
        if (_remainTime >= setRemainTime)
        {
            _remainTime = setRemainTime;
        }
    }

    public void AddToViewIDList(int viewID)
    {
        _playerViewIDList.Add(viewID);
        _playerViewIDList.Sort();
    }
    public void SetAIViewID(int viewID)
    {
        _aiViewID = viewID;
    }

    private bool CheckAITurn()
    {
        if (!GameManager.s_instance.IsAIExist)
        {
            return false;
        }
        //이전에 AI 턴 이었던 경우
        if (_isAITurn)
        {
            _isAITurn = false;
            return false;
        }
        //이번에 AI 턴이 온 경우
        if(_currentPlayerNum == PhotonNetwork.CurrentRoom.PlayerCount - 1)
        {
            _isAITurn = true;
            return true;
        }
        //그 외
        return false;
    }

    [PunRPC]
    private void SetIsAITurn(bool isAiTurn)
    {
        _isAITurn = isAiTurn;
    }
    public List<int> GetPlayerIDList()
    {
        return _playerViewIDList;
    }
}