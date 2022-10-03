using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class DefaultCharacterAI : MonoBehaviourPun
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
    public DefaultItem _equipItem = null;
    public WeaponItem _equipWeapon = null;
    public UtilityItem _equipUtility = null;
    public AimRotation _aimRotation = null;
    public Animator _animator = null;

    [Header("State Values")]
    public EquipItemType _equipItemType;
    public int _accumulateDamage = 0;
    public int _deathCnt = 0;
    public int _killCnt = 0;
    public bool _isFlying = false;
    public bool _isDeath = false;
    private string nowItemName = "";
    private Quaternion _firstQuaternion = Quaternion.identity;
    public enum PlayerState { IsAttack, IsActive }

    [PunRPC]
    public void PreProcess()
    {
        SetBoolState();
        GetComponent<PlayerInfoDisplay>().Init();
        _animator.SetTrigger("ReturnIdleTrigger");
        _cursorPointUI.SetActive(true);
        _aimRotation.transform.localRotation = _firstQuaternion;
        _aimRotation.AimingPoint.SetActive(true);
        _aimChargingBar.gameObject.SetActive(false);
        TurnManager.s_instance.DecreaseRemainTime(30);
        GameManager.s_instance.StartTurnTimerUI();
        GameManager.s_instance.SwitchQuickItem(0);
    }

    private void SetBoolState()
    {
        _isFlying = false;
        _isDeath = false;
    }

    public void Init()
    {
        Debug.Log("AICharacter Init");
        _teamColor = "Blue";
        _rigidbody2d = GetComponent<Rigidbody2D>();
        _rigidbody2d.bodyType = RigidbodyType2D.Dynamic;
        _aimRotation = GetComponentInChildren<AimRotation>();
        _aimRotation.AimingPoint.SetActive(false);
        _firstQuaternion = _aimRotation.transform.localRotation;
        _characterBody = transform.Find("Body");
        _infoData._hp = _infoData._maxHp;
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
        if (_infoData._hp >= calcDamage)
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
        GameManager.s_instance.SetAIHealthUI(0);
        if (photonView.IsMine && TurnManager.s_instance.IsAITurn)
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
        ReadyRespawn();
    }

    private void Die()
    {
        if (!_isDeath)
        {
            _isDeath = true;
            GameManager.s_instance.ChangeCameraMode(CameraController.CameraMode.LastMode);
            GameManager.s_instance.DecreaseTeamDeathCnt(_teamColor);
            _deathCnt++;
            _animator.SetTrigger("DieTrigger");
            //gameObject.transform.Find("DieEffect").gameObject.SetActive(true);
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
        if (_characterBody)
        {
            _isFlying = !(CheckUnder(Vector3.left * _infoData._bothSideOffset) || CheckUnder(Vector3.zero) || CheckUnder(Vector3.right * _infoData._bothSideOffset));
        }
    }

    private bool CheckUnder(Vector3 startPos)
    {
        Debug.DrawLine(transform.position + startPos, transform.position + startPos + Vector3.down * _infoData._underCheckOffset, Color.blue);
        foreach (RaycastHit2D raycast2D in Physics2D.RaycastAll(transform.position + startPos, Vector2.down, _infoData._underCheckOffset))
        {
            if (raycast2D.collider.gameObject.CompareTag("Ground"))
            {
                return true;
            }
            else if (raycast2D.collider.gameObject.CompareTag("Player") && raycast2D.collider.gameObject != _characterBody.gameObject)
            {
                return true;
            }
        }
        return false;
    }

    public int GetDirection()
    {
        return _characterBody.localScale.x > 0 ? 1 : -1;
    }
    public void UseItemAI(float power)
    {
        _aimChargingBar.gameObject.SetActive(false);
        Vector3 dir = _aimRotation.AimingPoint.transform.position - _equipTransform.transform.position;
        switch (_equipItemType)
        {
            case EquipItemType.noItem:
                _animator.SetTrigger("ReturnIdleTrigger");
                break;
            case EquipItemType.weaponItem:
                _equipWeapon.UseWeaponAI(dir.normalized, power);
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
        _animator.SetTrigger("ReturnIdleTrigger");
        switch (_equipItemType)
        {
            case EquipItemType.noItem:
                break;
            case EquipItemType.weaponItem:
                _equipWeapon.CancelWeapon();
                break;
            case EquipItemType.utilityItem:
                _equipUtility.CancelUtility();
                break;
            default:
                Debug.Log("There is no Cancel function for this item type!!");
                break;
        }

        Idle();
    }
    private void Idle()
    {
        _animator.SetTrigger("ReturnIdleTrigger");
    }

    public void ReturnDefaultState()
    {
        UnEquipItem();
        _animator.SetTrigger("ReturnIdleTrigger");
        _aimRotation.AimingPoint.SetActive(false);
        _aimChargingBar.gameObject.SetActive(false);
        _cursorPointUI.SetActive(false);
    }

    [PunRPC]
    public void Respawn()
    {
        _isDeath = false;
        _rigidbody2d.simulated = true;
        _rigidbody2d.velocity = Vector2.zero;
        _infoData._hp = _infoData._maxHp;
        GetComponent<PlayerInfoDisplay>().SetHealth(_infoData._hp);
        GameManager.s_instance.RefreshAIUI();
        GameManager.s_instance.CallRespawn();
        PreProcess();
        GameManager.s_instance.ChangeCameraMode(CameraController.CameraMode.RespawnMode);
    }
    [PunRPC]
    protected void DecreaseRemainTime()
    {
        TurnManager.s_instance.DecreaseRemainTime();
    }
    [PunRPC]
    public void SwitchItemAI(string nextItemName)
    {
        UnEquipItem();
        EquipItem(nextItemName);
    }
    private void UnEquipItem()
    {
        if (_equipItem)
        {
            switch (_equipItemType)
            {
                case EquipItemType.weaponItem:
                    _equipWeapon.CancelWeapon();
                    _equipWeapon = null;
                    break;
                case EquipItemType.utilityItem:
                    _equipUtility.CancelUtility();
                    _equipUtility = null;
                    break;
                default:
                    Debug.LogError("This ItemType has no definition");
                    break;
            }
            _equipItem.transform.position = ItemStorage.s_instance.WeaponStorage.transform.position;
            _equipItem.transform.parent = ItemStorage.s_instance.WeaponStorage;
            _equipItemType = EquipItemType.noItem;
            _equipItem = null;
            nowItemName = "";
        }
    }
    private void EquipItem(string nextItemName)
    {
        if (nextItemName == "" || nowItemName == nextItemName)
        {
            return;
        }
        //장비 장착
        foreach (Transform item in ItemStorage.s_instance.WeaponStorage)
        {
            if (item.name.Contains(nextItemName))
            {
                if (item.GetComponent<WeaponItem>() == false)
                {
                    return;
                }
                _equipItem = item.GetComponent<DefaultItem>();

                _equipItem.transform.parent = _equipTransform;
                _equipItem.transform.localPosition = Vector3.zero;
                nowItemName = nextItemName;

                if (_equipUtility = item.GetComponent<UtilityItem>())
                {
                    _equipItemType = EquipItemType.utilityItem;
                    if (_equipUtility._isAimType == false)
                    {
                        _equipUtility.UseUtility(Vector3.zero);
                    }
                }
                else if (_equipWeapon = item.GetComponent<WeaponItem>())
                {
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
