using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MapManager : MonoBehaviourPun
{
    public static MapManager s_instance = null;
    public GameObject _waterWorldEvent = null;
    public GameObject _eventObject = null;
    public GameObject[] _eventLevel = null;
    [SerializeField] private GameObject _background = null;

    public GameObject Background { get { return _background; } }
    private Transform _windsTransform = null;
    private Animator[] _windsAnimators = null;

    [SerializeField] private int _currWindForce = 0; //바람의 세기 음수, 양수로 방향 구분
    public int CurrWindForce { get { return _currWindForce; } }
    [SerializeField] private int _minWindForce = 0;
    [SerializeField] private int _maxWindForce = 0;
    [SerializeField] private const float c_targetYPos = 0.25f;
    [SerializeField] private const float c_duration = 2.5f;
    [SerializeField] private Vector3 _yVelocity = Vector3.zero;
    private int _windLevel = 0;
    private int _windLevelStep = 0;
    private int _windLevelCnt = 0;
    public int windLevel { get { return _windLevel; } }
    public int windLevelStep { get { return _windLevelStep; } }
    public int windLevelCnt { get { return _windLevelCnt; } }

    public bool _isWaterWorldEvent = false;

    private int _spriteX = 0;
    private int _spriteY = 0;
    private int _backgroundPositionX = 0;
    private int _backgroundPositionY = 0;
    private int _levelIndex = 0;
    private int _maxLevelIndex = 0;
    private float _waterEventTime = 3;
    public int SpriteX
    {
        get { return _spriteX; }
        set { _spriteX = value; }
    }

    public int SpriteY
    {
        get { return _spriteY; }
        set { _spriteY = value; }
    }

    public int BackgroundPositionX
    {
        get { return _backgroundPositionX; }
        set { _backgroundPositionX = value; }
    }

    public int BackgroundPositionY
    {
        get { return _backgroundPositionY; }
        set { _backgroundPositionY = value; }
    }
    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError("맵 매니저 생성 에러");
            Destroy(this.gameObject);
            return;
        }
        s_instance = this;

        _eventLevel = new GameObject[_eventObject.transform.childCount];
        for (int i = 0; i < _eventObject.transform.childCount; i++)
        {
            _eventLevel[i] = _eventObject.transform.GetChild(i).gameObject;
        }
    }

    public void Init()
    {
        _windsTransform = ReturnWindsTransform();
        if (_windsTransform)
        {
            _windsAnimators = _windsTransform.GetComponentsInChildren<Animator>();
        }

        if (ShowWindUI.s_insatnce.windImageCnt > 1)
        {
            _windLevelCnt = ShowWindUI.s_insatnce.windImageCnt - 1;
            _windLevelStep = (_maxWindForce - _minWindForce) / _windLevelCnt;
        }
        else
        {
            Debug.LogError("WindImage의 갯수가 1 이하입니다.");
            return;
        }

        _spriteX = (int)_background.GetComponent<SpriteRenderer>().bounds.size.x;
        _spriteY = (int)_background.GetComponent<SpriteRenderer>().bounds.size.y;
        _backgroundPositionX = (int)_background.transform.position.x;
        _backgroundPositionY = (int)_background.transform.position.y;
        _maxLevelIndex = _eventObject.transform.childCount - 1;
    }

    public void CallWorldEvent()
    {
        if (_levelIndex >= _maxLevelIndex)
        {
            _levelIndex = _maxLevelIndex;
            return;
        }

        _isWaterWorldEvent = true;
        _levelIndex++;
        _waterEventTime = 0;
    }


    private void FixedUpdate()
    {
        if (!_isWaterWorldEvent)
        {
            return;
        }

        _waterEventTime += Time.deltaTime;

        if (_waterEventTime < c_duration)
        {
            _waterWorldEvent.transform.position = Vector3.SmoothDamp(_waterWorldEvent.transform.position, new Vector3(0, _eventLevel[_levelIndex].transform.position.y, 0), ref _yVelocity, c_duration);
        }
        else
        {
            _isWaterWorldEvent = false;
        }
    }


    private Transform ReturnWindsTransform()
    {
        foreach (Transform mapManangerChild in transform)
        {
            if (mapManangerChild.name == "Map")
            {
                Transform mapTransform = mapManangerChild;
                foreach (Transform mapChild in mapTransform)
                {
                    if (mapChild.name == "Winds")
                    {
                        return mapChild;
                    }
                }
                Debug.LogError("Map 오브젝트에 Winds 오브젝트가 없습니다");
                return null;
            }
        }
        Debug.LogError("MapManager 오브젝트에 Map 오브젝트가 없습니다");
        return null;
    }

    public void UpdateWindSystem()
    {
        _currWindForce = Random.Range(-_maxWindForce, _maxWindForce);
        if (Mathf.Abs(_currWindForce) < _minWindForce)
        {
            _currWindForce = 0;
            _windLevel = 0;
        }
        else
        {
            _windLevel = ((Mathf.Abs(_currWindForce) - _minWindForce) / _windLevelStep);
            bool isPlus = (_currWindForce > 0);
            _currWindForce = (_minWindForce + (_windLevelStep * _windLevel));
            _windLevel++;
            if(!isPlus)
            {
                _currWindForce *= -1;
                _windLevel *= -1;
            }
        }
        photonView.RPC("SetWindSystem", RpcTarget.All, _currWindForce, _windLevel);
    }

    [PunRPC]
    public void SetWindSystem(int windForce, int windLevel)
    {
        _currWindForce = windForce;
        _windLevel = windLevel;
        Debug.Log($"이번 턴 바람의 세기: {windForce}  바람의 레벨: {windLevel}");
    }
}
