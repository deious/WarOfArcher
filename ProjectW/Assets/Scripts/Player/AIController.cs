using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class _AIController : MonoBehaviourPun
{
    //Contorller 부분
    [Header("Pre-Setting Values")]
    [SerializeField] private AimRotation _aimRotation = null;
    [SerializeField] private AimCharge _aimCharge = null;
    [SerializeField] private DefaultCharacterAI _avatarClass = null;
    [SerializeField] private Animator _ani = null;

    //AI 추가 부분
    [Header("Auto-Setting Values")]
    [SerializeField] List<int> enemyViewIDList = null;
    [SerializeField] private GameObject _currentTurnTarget = null;
    [SerializeField] private bool _isAiming = false;

    [SerializeField] private float _targetAngleAI = 0f;
    [SerializeField] private float _targetLocalAngleAI = 0f;
    private float _currentAngleAI = 0f;

    private float c_maxPowerTime = 3f;
    private float c_minShootVelocity = 10f;
    private float c_maxShootVelocity = 70f;

    private float _powerTime = 0f;
    private float _targetPowerRate = 0f;
    [SerializeField] private float _targetShootVelocity = 0f;


    [SerializeField] private const float c_firePower = 1.8f;
    [SerializeField] private Transform[] _RespawnPoints = null;

    private float _respawnPointX = 0f;
    private bool _isRespawn = false;
    public bool IsRespawn { get { return _isRespawn; } }
    private const float c_radianShortDistance = 1.56f;
    private const float c_radianMiddleDistance = 1.4f;

    private const float c_respawnMoveOffset = 0.5f;
    public float RespawnMoveOffset { get { return c_respawnMoveOffset; } }

    enum AI_STATE
    {
        AI_ASSESS_ENVIRONMENT = 0,
        AI_MOVE,
        AI_CHOOSE_TARGET,
        AI_AIM,
        AI_FIRE,
        AI_AIM_ROTATION,
        AI_AIM_CHARGE,
        AI_End
    }
    AI_STATE aiState = 0;

    public DefaultCharacterAI AvatarClass { get { return _avatarClass; } }

    public bool IsAimimg
    {
        get { return _isAiming; }
        set { _isAiming = value; }
    }
    public void Init()
    {
        InitAvatar();
        InitRespawnPoints();
    }
    public void PreProcess()
    {
        InitEnemyViewIDList();
        _currentTurnTarget = null;
        _targetShootVelocity = c_minShootVelocity;
        aiState = AI_STATE.AI_ASSESS_ENVIRONMENT;
        _respawnPointX = _RespawnPoints[Random.Range(1, _RespawnPoints.Length)].position.x;
        _isRespawn = _avatarClass._isDeath;

        if (_isRespawn)
        {
            GameManager.s_instance.CallRespawn();
        }
    }
    public void AITurnStart()
    {
        if (_isRespawn == false)
        {
            _avatarClass.photonView.RPC("PreProcess", RpcTarget.All);
            StartCoroutine(AISequence());
        }
    }
    public void PostProcess()
    {
        DeactivateController();
        StopAllCoroutines();
    }
    private void FixedUpdate()
    {
        if (!_avatarClass || !TurnManager.s_instance.IsAITurn)
        {
            return;
        }

        if (_isRespawn)
        {
            transform.position = new Vector2(transform.position.x - c_respawnMoveOffset, transform.position.y);
            return;
        }

        switch (aiState)
        {
            case AI_STATE.AI_ASSESS_ENVIRONMENT:
                break;
            case AI_STATE.AI_MOVE:
                break;
            case AI_STATE.AI_CHOOSE_TARGET:
                break;
            case AI_STATE.AI_AIM:
                break;
            case AI_STATE.AI_FIRE:
                break;
            case AI_STATE.AI_AIM_ROTATION:
                _currentAngleAI = _aimRotation.GetAngle(_avatarClass.GetDirection());
                if (Mathf.Abs(_targetLocalAngleAI - _currentAngleAI) > 3f)
                {
                    _aimRotation.AimAI(_targetLocalAngleAI > _currentAngleAI, _avatarClass.GetDirection());
                }
                else
                {
                    EndRotation();
                }
                break;
            case AI_STATE.AI_AIM_CHARGE:
                if ((_powerTime / c_maxPowerTime) < _targetPowerRate)
                {
                    _powerTime += Time.deltaTime;
                    _aimCharge._powerRate = (_powerTime / c_maxPowerTime);
                }
                else
                {
                    EndCharge();
                }
                break;
            case AI_STATE.AI_End:
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        if (_isRespawn && TurnManager.s_instance.IsAITurn)
        {
            if (transform.position.x < _respawnPointX)
            {
                _isRespawn = false;
                _avatarClass.photonView.RPC("Respawn", RpcTarget.All);
                AITurnStart();
            }
            if (Vector3.Distance(transform.position, PlayerRespawn.RespawnEndPoint.position) <= 1.0f)
            {
                _isRespawn = false;
                _avatarClass.photonView.RPC("Respawn", RpcTarget.All);
                AITurnStart();
            }
        }
    }
    private IEnumerator AISequence()
    {
        while (aiState != AI_STATE.AI_End)
        {
            yield return new WaitForSeconds(0.2f);
            if (_avatarClass._isFlying)
            {
                Debug.Log("하늘 나는 중");
                continue;
            }
            if (TurnManager.s_instance.IsAITurn)
            {
                switch (aiState)
                {
                    case AI_STATE.AI_ASSESS_ENVIRONMENT:
                        OnStateIdle();
                        break;
                    case AI_STATE.AI_CHOOSE_TARGET:
                        OnStatChoosing();
                        break;
                    case AI_STATE.AI_MOVE:
                        OnStateMoving();
                        break;
                    case AI_STATE.AI_AIM:
                        OnStateAimming();
                        break;
                    case AI_STATE.AI_FIRE:
                        OnStateShooting();
                        break;
                    case AI_STATE.AI_End:
                        _avatarClass._animator.SetTrigger("ReturnIdleTrigger");
                        break;
                    case AI_STATE.AI_AIM_ROTATION:
                        break;
                    case AI_STATE.AI_AIM_CHARGE:
                        break;
                    default:
                        Debug.LogError("정의되지 않은 AI_STATE 입니다.");
                        break;
                }
            }
        }
        photonView.RPC("OnStateEnd", RpcTarget.All);
        yield return null;
    }
    private void OnStateIdle()
    {
        _avatarClass._animator.SetTrigger("ReturnIdleTrigger");
        Debug.Log("상태 변경 및 아이템 선택");
        if (!_currentTurnTarget)
        {
            aiState = AI_STATE.AI_CHOOSE_TARGET;
            return;
        }
        if (CalcTargetAngle() == false)
        {
            aiState = AI_STATE.AI_MOVE;
            return;
        }
        AutoSelectItem();
    }
    private void OnStatChoosing()
    {
        Debug.Log("타겟 선택");
        AutoChooseTarget();
        aiState = AI_STATE.AI_ASSESS_ENVIRONMENT;
    }
    private void OnStateMoving()
    {
        _targetShootVelocity += 5f;
        if (_targetShootVelocity >= c_maxShootVelocity)
        {
            Debug.Log("타켓에게 발사 불가");
            aiState = AI_STATE.AI_End;
            return;
        }
        aiState = AI_STATE.AI_ASSESS_ENVIRONMENT;
    }
    private void OnStateAimming()
    {
        BeginRotation();
    }

    private void BeginRotation()
    {
        photonView.RPC("SwitchAimPoint", RpcTarget.All, true);
        _avatarClass._equipItem.GetComponent<SpriteRenderer>().enabled = false;
        Debug.Log("에임 회전 시작");
        _targetLocalAngleAI = LocalAngle(_targetAngleAI);
        Debug.Log($"현재 각도: {_aimRotation.GetAngle(_avatarClass.GetDirection())}, 목표 각도: {_targetLocalAngleAI}");
        aiState = AI_STATE.AI_AIM_ROTATION;
    }

    private void EndRotation()
    {
        Debug.Log("에임 회전 종료");
        photonView.RPC("SetUpAiming", RpcTarget.All, _targetAngleAI);
        BeginCharge();
    }

    private void OnStateShooting()
    {
        _avatarClass._animator.SetTrigger("ThrowTrigger");
        Debug.Log($"목표 힘: {_targetShootVelocity} 목표 시간: {_targetPowerRate * c_maxPowerTime}");
        _avatarClass.UseItemAI(_targetShootVelocity);
        _avatarClass._animator.SetTrigger("ShootTrigger");
        aiState = AI_STATE.AI_End;
    }

    private void BeginCharge()
    {
        Debug.Log("발사 차징 시작");
        aiState = AI_STATE.AI_AIM_CHARGE;
    }


    private void EndCharge()
    {
        Debug.Log("발사 차징 종료");
        aiState = AI_STATE.AI_FIRE;
    }

    [PunRPC]
    private void OnStateEnd()
    {
        TurnManager.s_instance.DecreaseRemainTime();
    }

    private void InitEnemyViewIDList()
    {
        Debug.Log("적군 리스트 작성");
        enemyViewIDList = TurnManager.s_instance.PlayerViewIDList;
        for (int player = 0; player < enemyViewIDList.Count; player++)
        {
            if (enemyViewIDList[player] == photonView.ViewID // TODO: 같은 팀일 때 처리 필요
                    )
            {
                enemyViewIDList.Remove(player);
                player--;
            }
        }
    }
    private void InitAvatar()
    {
        _avatarClass = GetComponent<DefaultCharacterAI>();
        _avatarClass.Init();
        _aimRotation = _avatarClass._aimRotation;
        _aimCharge = _avatarClass._aimCharge;
    }
    private void DeactivateController()
    {
        _avatarClass.ReturnDefaultState();
        aiState = AI_STATE.AI_End;
    }

    private void AutoChooseTarget()
    {
        for (int enemy = 0; enemy < enemyViewIDList.Count; enemy++)
        {
            if (enemyViewIDList[enemy] != photonView.ViewID)
            {
                _currentTurnTarget = PhotonNetwork.GetPhotonView(enemyViewIDList[enemy]).gameObject;
                return;
            }
        }
        Debug.Log("타켓 찾기에 실패했습니다.");
    }

    private void AutoSelectItem()
    {
        if (Mathf.Abs(_targetAngleAI) > c_radianShortDistance)
        {
            Debug.Log("배트 장착");
            _avatarClass.photonView.RPC("SwitchItemAI", RpcTarget.All, "Bat");
            aiState = AI_STATE.AI_AIM;
        }
        else if (Mathf.Abs(_targetAngleAI) > c_radianMiddleDistance)
        {
            Debug.Log("클러스터붐 장착");
            _avatarClass.photonView.RPC("SwitchItemAI", RpcTarget.All, "ClusterBoom");
            aiState = AI_STATE.AI_AIM;
        }
        else
        {
            Debug.Log("바주카 장착");
            _avatarClass.photonView.RPC("SwitchItemAI", RpcTarget.All, "Bazooka");
            aiState = AI_STATE.AI_AIM;
        }
        _avatarClass._equipItem.GetComponent<SpriteRenderer>().enabled = true;
    }

    private bool CalcTargetAngle()
    {
        float betweenY = (_currentTurnTarget.transform.position.y - transform.position.y);
        float betweenX = (_currentTurnTarget.transform.position.x - transform.position.x);
        float gravity = -Physics2D.gravity.y;


        float targetAlpha = Mathf.Pow(_targetShootVelocity, 4) - gravity * (gravity * Mathf.Pow(betweenX, 2) + 2.0f * betweenY * Mathf.Pow(_targetShootVelocity, 2));

        if (targetAlpha < 0)
        {
            Debug.Log("타겟이 범위 밖에 있습니다");
            return false;
        }

        // Worm is close enough, calculate trajectory
        float maxHeight = _targetShootVelocity * _targetShootVelocity + Mathf.Sqrt(targetAlpha);
        float minHeight = _targetShootVelocity * _targetShootVelocity - Mathf.Sqrt(targetAlpha);

        float MaxAngle = Mathf.Atan(maxHeight / (gravity * betweenX)); // Max Height
        //float MinAngle = Mathf.Atan(minHeight / (gravity * betweenX)); // Min Height

        // We'll use max as its a greater chance of avoiding obstacles
        _targetAngleAI = MaxAngle;


        if (betweenX * _avatarClass.GetDirection() < 0)
        {
            Debug.Log("방향 전환");
            ChangeDirection();
        }
        return true;
    }

    [PunRPC]
    private void SetUpAiming(float rad)
    {
        _aimRotation.transform.eulerAngles = new Vector3(0, 0, Mathf.Abs(rad * Mathf.Rad2Deg) * (_avatarClass.GetDirection() > 0 ? 1 : -1));
        InitCharge();
    }
    private void InitCharge()
    {
        _aimRotation.AimingPoint.SetActive(false);
        _aimCharge.gameObject.SetActive(true);
        _aimCharge._powerRate = _powerTime = 0;
        _targetPowerRate = (_targetShootVelocity - c_minShootVelocity) / (c_maxShootVelocity - c_minShootVelocity);
    }

    private void ChangeDirection()
    {
        Vector3 preLocalScale = _avatarClass._characterBody.transform.localScale;
        preLocalScale.x *= -1;
        _avatarClass._characterBody.transform.localScale = preLocalScale;
    }

    private void BeIdle()
    {
        _isAiming = false;
        _aimRotation.AimingPoint.SetActive(false);
    }

    private void InitRespawnPoints()
    {
        _RespawnPoints = PlayerRespawn.RespawnAIPoints.GetComponentsInChildren<Transform>();
    }

    [PunRPC]
    private void SwitchAimPoint(bool value)
    {
        _isAiming = value;
        _aimRotation.AimingPoint.SetActive(value);
    }

    private float LocalAngle(float rad)
    {
        return Mathf.Abs(rad * _avatarClass.GetDirection() * Mathf.Rad2Deg);
    }
}