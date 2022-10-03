using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public enum EquipItemType { noItem, weaponItem, utilityItem }
public class DefaultCharacter : MonoBehaviourPun
{
    [System.Serializable]
    public class Data
    {
        public string _playerName = "";

        public int _hp = 0;
        public int _maxHp = 0;
        public int _deathCnt = 0;

        public float _jumpForce = 0;
        public float _jumpOffset = 0;
        public float _jumpDelayTime = 0;
        public float _backFlipForce = 0;

        public float _moveSpeed = 0;
        public float _bothSideOffset = 0;
        public float _underCheckOffset = 0;
    };

    [SerializeField] private Data _infoData = null;
    public Data InfoData { get { return _infoData; } }


    [Header("Pre-Setting Values")]
    [SerializeField] private string _teamColor;

    [Header("Auto-Setting Values")]
    public GameObject _cursorPointUI = null;
    public Transform _equipTransform = null;
    public Rigidbody2D _rigidbody2d = null;
    public Transform _characterBody = null;
    public Transform _aimChargingBar = null;
    public AimCharge _aimCharge = null;
    public Transform _characterCanvas = null;
    public DefaultItem _equipItem = null;
    public WeaponItem _equipWeapon = null;
    public UtilityItem _equipUtility = null;
    public AimRotation _aimRotation = null;
    public Animator _animator = null;


    [Header("State Values")]
    public EquipItemType _equipItemType;
    public int _accumulateDamage = 0;
    public bool _isJump = false;
    public bool _isFlying = false;
    public bool _isDeath = false;
    public bool _isAttack = false;
    private bool _isWalking = false;
    private bool _isShoot = false;
    private string nowItemName = "";
    private Quaternion _firstQuaternion = Quaternion.identity;
    private DistanceJoint2D _distanceJoint2D = null;

    public enum PlayerState { IsAttack, IsActive }

    [SerializeField] private bool _isActive = false;
    private bool jumpCheck = false;

    public void PreProcess()
    {
        SetBoolState();
        _accumulateDamage = 0;
        GetComponent<PlayerInfoDisplay>().Init();
        _animator.SetTrigger("ReturnIdleTrigger");
        _cursorPointUI.SetActive(true);
        _aimRotation.transform.localRotation = _firstQuaternion;
        _aimRotation.AimingPoint.SetActive(true);
        _aimChargingBar.gameObject.SetActive(false);
        _distanceJoint2D.enabled = false;
        TurnManager.s_instance.DecreaseRemainTime(30);
        GameManager.s_instance.StartTurnTimerUI();

        if (TurnManager.s_instance.CurrentPlayer.IsLocal && !TurnManager.s_instance.IsAITurn)
        {
            GameManager.s_instance.SwitchQuickItem(0);
        }
    }

    private void SetBoolState()
    {
        _isFlying = false;
        _isDeath = false;
        _isAttack = false;
    }

    public void Init()
    {
        Debug.Log("DefaultCharacter Init");
        _teamColor = (string)photonView.Owner.CustomProperties["TeamColor"];
        Debug.Log("Character information is : " + photonView.ViewID + " / " + _teamColor);
        _rigidbody2d = GetComponent<Rigidbody2D>();
        _rigidbody2d.bodyType = RigidbodyType2D.Dynamic;
        _aimRotation = GetComponentInChildren<AimRotation>();
        _aimRotation.AimingPoint.SetActive(false);
        _firstQuaternion = _aimRotation.transform.localRotation;
        _characterBody = transform.Find("Body");
        _infoData._hp = _infoData._maxHp;
        _distanceJoint2D = GetComponent<DistanceJoint2D>();

        if (!_characterBody)
        {
            Debug.LogError("이 character 는 Body object 를 가지고 있지 않습니다");
        }

        _equipTransform = _characterBody.Find("ItemEquipPosition");
        if (!_equipTransform)
        {
            Debug.LogError("이 characterBody 는 ItemEquipPosition object 를 가지고 있지 않습니다");
            return;
        }

        _aimChargingBar = _equipTransform.Find("AimChargingBar");
        if (!_aimChargingBar)
        {
            Debug.LogError("이 ItemEquipPosition 는 AimChargingBar object 를 가지고 있지 않습니다");
            return;
        }
        _aimCharge = _aimChargingBar.GetComponent<AimCharge>();
        _aimCharge.Init();

        _animator = _characterBody.GetComponent<Animator>();
        if (!_animator)
        {
            Debug.LogError("이 characterBody 는 Animator component 를 가지고 있지 않습니다");
            return;
        }

        GetComponent<PlayerInfoDisplay>().Init();
    }


    protected virtual int CalcDamage(int damage) { return damage; }

    public void CallHit(int damage)
    {
        if (damage == 0)
            return;
        photonView.RPC("Hit", RpcTarget.All, damage);
        _animator.SetTrigger("HitTrigger");
    }

    [PunRPC]
    protected virtual void Hit(int damage)
    {
        int calcDamage = CalcDamage(damage);

        if (_infoData._hp > calcDamage)
        {
            _infoData._hp -= calcDamage;
        }
        else
        {
            calcDamage = _infoData._hp;
            _infoData._hp = 0;
            Die();
        }

        _accumulateDamage += calcDamage;
        GameManager.s_instance.SaveDamagedPlayer(gameObject.GetPhotonView().ViewID);
    }

    [PunRPC]
    protected void DeadZoneDie()
    {
        Die();
        GameManager.s_instance.SetIndexPlayerHealthUI((int)photonView.Owner.CustomProperties["Index"], 0);
        if (photonView.IsMine && TurnManager.s_instance.CurrentPlayer.IsLocal)
        {
            photonView.RPC("DecreaseRemainTime", RpcTarget.All);
        }

        //TODO : 익사 애니메이션 실행 후 위치 이동
        _rigidbody2d.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody2d.velocity = Vector2.zero;
        _animator.SetTrigger("DrownTrigger");
        if (gameObject.transform.Find("DrownEffect").gameObject == null)
        {
            Debug.LogWarning("DrownEffect가 없습니다");
        }
        gameObject.transform.Find("DrownEffect").gameObject.SetActive(true);

        if (gameObject.transform.Find("DieEffect").gameObject == null)
        {
            Debug.LogWarning("DieEffect가 없습니다");
        }
        gameObject.transform.Find("DieEffect").gameObject.SetActive(true);
        //Invoke(nameof(ReadyRespawn), 1.5f);       // 훗날 다시 사용 할 수도 있어서 주석처리
    }

    private void Die()
    {
        if (!_isDeath)
        {
            _isDeath = true;
            GameManager.s_instance.ChangeCameraMode(CameraController.CameraMode.LastMode);
            GameManager.s_instance.DecreaseTeamDeathCnt(_teamColor);
            photonView.Owner.CustomProperties["DeathCnt"] = (int)photonView.Owner.CustomProperties["DeathCnt"] + 1;
            //gameObject.transform.Find("DieEffect").gameObject.SetActive(true);        // 훗날 다시 사용 할 수도 있어서 주석처리 하였음
        }
    }

    public void ReadyRespawn()
    {
        gameObject.transform.Find("DrownEffect").gameObject.SetActive(false);
        if (gameObject.transform.Find("DrownEffect").gameObject == null)
        {
            Debug.Log("DrownEffect가 없습니다");
        }

        gameObject.transform.Find("DieEffect").gameObject.SetActive(false);
        if (gameObject.transform.Find("DieEffect").gameObject == null)
        {
            Debug.Log("DieEffect가 없습니다");
        }
        _rigidbody2d.simulated = false;
        _rigidbody2d.bodyType = RigidbodyType2D.Dynamic;
        transform.position = PlayerRespawn.RespawnStartPoint.transform.position;
        _rigidbody2d.velocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeadZone") && !_isDeath)
        {
            if (photonView.IsMine)
            {
                photonView.RPC("DeadZoneDie", RpcTarget.All);
            }
        }
        _isFlying = false;
    }

    private void Update()
    {
        CheckState();
        if (_characterBody)
        {
            if (CheckUnderFrom(Vector3.left * _infoData._bothSideOffset) && CheckUnderFrom(Vector3.zero) && CheckUnderFrom(Vector3.right * _infoData._bothSideOffset))
            {
                _isFlying = true;
            }
            if (!_isFlying && jumpCheck)
            {
                _animator.SetTrigger("LandTrigger");
                jumpCheck = false;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.point.y - transform.position.y < 0.5f)
                {
                    _isFlying = false;
                }
            }
        }
    }
    private bool CheckUnderFrom(Vector3 startPos)
    {
        if (!CheckUnderTo(startPos, Vector3.down * _infoData._underCheckOffset))
        {
            return false;
        }

        if (!CheckUnderTo(startPos, Vector3.zero))
        {
            return false;
        }
        return true;
    }

    private bool CheckUnderTo(Vector3 startPos, Vector3 endPos)
    {
        Debug.DrawLine(transform.position + startPos, transform.position + endPos, Color.blue);
        foreach (RaycastHit2D raycast2D in Physics2D.LinecastAll(transform.position + startPos, transform.position + endPos))
        {
            if (raycast2D.collider.gameObject.CompareTag("Ground"))
            {
                return false;
            }
            else if (raycast2D.collider.gameObject.CompareTag("Player") && raycast2D.collider.gameObject != _characterBody.gameObject)
            {
                return false;
            }
        }
        return true;
    }


    public int GetDirection()
    {
        return _characterBody.localScale.x > 0 ? 1 : -1;
    }

    public void ShootKeyDown()
    {
        _equipItem.GetComponent<SpriteRenderer>().enabled = false;
        switch (_equipItemType)
        {
            case EquipItemType.weaponItem:
                if (!_isAttack)
                {
                    _isAttack = true;
                    photonView.RPC("UseItem", RpcTarget.AllBufferedViaServer);
                    if (_equipWeapon.GetComponent<Bat>())
                    {
                        _animator.SetTrigger("AttackTrigger");
                    }
                }
                break;
            case EquipItemType.utilityItem:
                if (_equipItem._isAimType)
                {
                    if (!_isShoot)
                    {
                        photonView.RPC("UseItem", RpcTarget.All);
                    }
                    else
                    {
                        photonView.RPC("CancelItem", RpcTarget.All);
                    }
                }
                else
                {
                    if (_equipItem._isUsed)
                    {
                        photonView.RPC("CancelItem", RpcTarget.All);
                    }
                    else
                    {
                        photonView.RPC("UseItem", RpcTarget.All);
                    }
                }
                break;
            default:
                break;
        }
    }

    private void CheckState()
    {
        _isWalking = InputManager.s_instance._horizontalAxisRaw != 0 || _isFlying;
        _isActive = _isWalking || _isFlying || _isAttack;
    }
    public void Move(float horizontalAxisRaw, bool jumpKeyDown, bool backFlipKeydown)
    {
        if (!_isFlying)
        {
            //WakeUp();
            if (!_equipItem || !_equipItem._isUsed)
            {
                if (jumpKeyDown || backFlipKeydown)
                {
                    if (!_isJump)
                    {
                        Jump(backFlipKeydown);
                    }
                }

                if (!_isJump && _isWalking && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
                {
                    Walk(horizontalAxisRaw);
                }
                else
                {
                    _animator.SetTrigger("ReturnIdleTrigger");
                }
            }
        }
        ChangeDirection(horizontalAxisRaw);
    }

    public void Aim(float verticalAxisRaw)
    {
        if (!_aimRotation.AimingPoint.activeSelf)
        {
            return;
        }
        int isClockwise = (int)verticalAxisRaw * GetDirection();
        _aimRotation.Aim(verticalAxisRaw, GetDirection());
    }

    private void Walk(float horizontalAxisRaw)
    {
        _animator.SetTrigger("WalkTrigger");
        Vector3 vel = _rigidbody2d.velocity;
        vel.x = horizontalAxisRaw * InfoData._moveSpeed;
        _rigidbody2d.velocity = vel;
    }
    private void Jump(bool backFlipKeydown)
    {
        _isJump = true;
        if (backFlipKeydown)
        {
            _animator.SetTrigger("BackFlipTrigger");
            _rigidbody2d.AddForce((Vector2.up * InfoData._jumpForce * InfoData._backFlipForce) + ((GetDirection() > 0 ? Vector2.left : Vector2.right) * (InfoData._jumpForce * InfoData._jumpOffset)),
                ForceMode2D.Impulse);
        }
        else
        {
            _animator.SetTrigger("JumpTrigger");
            _rigidbody2d.AddForce((Vector2.up * InfoData._jumpForce) + ((GetDirection() > 0 ? Vector2.right : Vector2.left) * (InfoData._jumpForce * InfoData._jumpOffset)),
                ForceMode2D.Impulse);
        }
        Invoke(nameof(DelayJump), InfoData._jumpDelayTime);
    }


    private void ChangeDirection(float horizontalAxisRaw)
    {
        if ((horizontalAxisRaw < 0 && GetDirection() == 1) || (horizontalAxisRaw > 0 && GetDirection() == -1))
        {
            _characterBody.localScale = new Vector3(-_characterBody.localScale.x, _characterBody.localScale.y, _characterBody.localScale.z);
        }
    }
    private void DelayJump()
    {
        _isJump = false;
        jumpCheck = true;
    }
    [PunRPC]
    protected void UseItem()
    {
        _isShoot = true;
        _aimRotation.AimingPoint.SetActive(false);
        Vector3 dir = _aimRotation.AimingPoint.transform.position - _equipTransform.transform.position;
        switch (_equipItemType)
        {
            case EquipItemType.noItem:
                _animator.SetTrigger("ReturnIdleTrigger");
                break;
            case EquipItemType.weaponItem:
                _equipWeapon.UseWeapon(dir.normalized);
                break;
            case EquipItemType.utilityItem:
                _equipUtility.UseUtility(dir.normalized);
                break;
            default:
                Debug.Log("There is no Use function for this item type!!");
                break;
        }
    }
    [PunRPC]
    protected void CancelItem()
    {
        _isShoot = false;
        //_animator.SetTrigger("ReturnIdleTrigger");
        switch (_equipItemType)
        {
            case EquipItemType.noItem:
                break;
            case EquipItemType.weaponItem:
                _equipWeapon.CancelWeapon();
                break;
            case EquipItemType.utilityItem:
                _equipUtility.CancelUtility();
                if (_equipUtility._isAimType)
                {
                    _aimRotation.AimingPoint.SetActive(true);
                    _equipUtility.gameObject.SetActive(true);
                }
                break;
            default:
                Debug.Log("There is no Cancel function for this item type!!");
                break;
        }

        Idle();
    }

    public void CallCancelItem()
    {
        photonView.RPC("CancelItem", RpcTarget.All);
    }

    private void Idle()
    {
        _animator.SetTrigger("ReturnIdleTrigger");
    }

    public void ReturnDefaultState()
    {
        UnEquipItem();
        _distanceJoint2D.enabled = false;
        _animator.SetTrigger("ReturnIdleTrigger");
        _aimRotation.AimingPoint.SetActive(false);
        _cursorPointUI.SetActive(false);
        _isAttack = false;
    }

    [PunRPC]
    public void Respawn()
    {
        _isDeath = false;
        _rigidbody2d.simulated = true;
        _rigidbody2d.velocity = Vector2.zero;
        _infoData._hp = _infoData._maxHp;
        GetComponent<PlayerInfoDisplay>().SetHealth(_infoData._hp);
        InputManager.s_instance.StartPlayerTurn();
        GameManager.s_instance.RefreshCurrentPlayerUI();
        PreProcess();
        GameManager.s_instance.ChangeCameraMode(CameraController.CameraMode.RespawnMode);
    }
    public bool GetCurrentPlayerState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.IsAttack:
                return _isAttack;
            case PlayerState.IsActive:
                return _isActive;
            default:
                Debug.LogError($"Incorrect state name / Input state : {playerState}");
                return false;
        }
    }
    [PunRPC]
    protected void DecreaseRemainTime()
    {
        TurnManager.s_instance.DecreaseRemainTime(1);
    }
    [PunRPC]
    public void SwitchItem(string nextItemName)
    {
        if (nowItemName == nextItemName)
        {
            return;
        }
        UnEquipItem();
        EquipItem(nextItemName);
    }

    private void UnEquipItem()
    {
        _isShoot = false;
        if (_equipItem)
        {
            switch (_equipItemType)
            {
                case EquipItemType.weaponItem:
                    _equipWeapon.CancelWeapon();
                    break;
                case EquipItemType.utilityItem:
                    _equipUtility.CancelUtility();
                    break;
                default:
                    Debug.LogError("This ItemType has no definition");
                    break;
            }
            _equipItem.transform.position = ItemStorage.s_instance.WeaponStorage.transform.position;
            _equipItem.transform.parent = ItemStorage.s_instance.WeaponStorage;
            _equipItemType = EquipItemType.noItem;
            _equipItem = null;
            nowItemName = null;
        }
    }

    private void EquipItem(string nextItemName)
    {
        if(nextItemName == "")
        {
            return;
        }
        //장비 장착
        foreach (Transform item in ItemStorage.s_instance.WeaponStorage)
        {
            if (item.name.Contains(nextItemName))
            {
                if (item.GetComponent<WeaponItem>() == false && _isAttack)
                {
                    return;
                }
                _equipItem = item.GetComponent<DefaultItem>();
                _equipItem.transform.parent = _equipTransform;
                _equipItem.transform.localPosition = Vector3.zero;
                nowItemName = nextItemName;
                _equipItem.GetComponent<SpriteRenderer>().enabled = true;
                if (_equipUtility = item.GetComponent<UtilityItem>())
                {
                    _equipItemType = EquipItemType.utilityItem;
                    if (_equipUtility._isAimType)
                    {
                        _aimRotation.AimingPoint.SetActive(true);
                        _equipItem.gameObject.SetActive(true);
                        _equipItem.transform.localPosition = Vector3.right;
                    }
                    else
                    {
                        _equipUtility.UseUtility(Vector3.zero);
                    }
                }
                else if (_equipWeapon = item.GetComponent<WeaponItem>())
                {
                    _aimRotation.AimingPoint.SetActive(true);
                    _equipItemType = EquipItemType.weaponItem;
                    _equipItem.transform.localPosition = Vector3.right;
                }
                else
                {
                    _equipItemType = EquipItemType.noItem;
                }
                break;
            }
        }
    }
}
