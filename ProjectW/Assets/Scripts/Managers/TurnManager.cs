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

    [SerializeField] private double _remainTime;                //�Ͽ� ���� �ð� üũ
    public double RemainTime { get { return _remainTime; } }
    public bool _turnTimer = false;           //Ÿ�̸� on/off
    [SerializeField] private const int c_offsetTime = 5;
    [SerializeField] private double _maxTime = 30.0;            //�� ���� �ð�
    private double _prevTime = 0f;                //���� ���۵� ����
    private double _deltaTime = 0f;              //���� ���� �ð�

    public int _remainWorldEventCnt = 0;               // ���� ���� �̺�Ʈ ī��Ʈ
    public bool _isTurnFinished = true;

    [SerializeField] private bool _isAITurn = false;
    public bool IsAITurn { get { return _isAITurn; } }

    public static TurnManager s_instance = null;
    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError("�� �Ŵ��� ���� ����");
            Destroy(this.gameObject);
            return;
        }
        s_instance = this;
        _currentPlayerNum = 0;
        _currentPlayer = PhotonNetwork.PlayerList[_currentPlayerNum];        //ù ���� 0��° �÷��̾�
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

        if (_currentPlayer.IsLocal)                                         //0��° �÷��̾ ���÷� ������ Ŭ���̾�Ʈ���� ���� ����
        {
            if (PhotonNetwork.GetPhotonView(_playerViewIDList[_currentPlayerNum]))
            {
                photonView.RPC("SetCurrentCharacter", RpcTarget.AllBufferedViaServer, _playerViewIDList[_currentPlayerNum]);
            }
            else
            {
                Debug.LogError("�ش� �÷��̾��� ���� �� ���̵� ����ֽ��ϴ�");
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
    //���� ������ �Լ�
    [PunRPC]
    private void SetNextTurn()
    {
        _currentPlayerNum = (_currentPlayerNum + 1) % PhotonNetwork.CurrentRoom.PlayerCount;
        _currentPlayer = PhotonNetwork.PlayerList[_currentPlayerNum];     //���� �÷��̾�� �� �ѱ�

        if (PhotonNetwork.IsMasterClient)
        {
            if(PhotonNetwork.GetPhotonView(_playerViewIDList[_currentPlayerNum]))
            {
                photonView.RPC("SetCurrentCharacter", RpcTarget.AllBufferedViaServer, _playerViewIDList[_currentPlayerNum]);
            }
            else
            {
                Debug.LogError("�ش� �÷��̾��� ���� �� ���̵� ����ֽ��ϴ�");
            }
            photonView.RPC("BeginTurn", RpcTarget.AllBufferedViaServer);
        }
    }

    //���� ���� ĳ���͸� _currentCharacter�� ã�ƿ�
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

    //��Ÿ�̸� ����
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
        //������ AI �� �̾��� ���
        if (_isAITurn)
        {
            _isAITurn = false;
            return false;
        }
        //�̹��� AI ���� �� ���
        if(_currentPlayerNum == PhotonNetwork.CurrentRoom.PlayerCount - 1)
        {
            _isAITurn = true;
            return true;
        }
        //�� ��
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