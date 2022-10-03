using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ProjectileWeapon : WeaponItem     //Launcher
{
    [SerializeField] private GameObject _bullet;
    [SerializeField] private LineRenderer _trajectory;
    public GameObject Bullet { get { return _bullet; } set { _bullet = value; } }
    private string _bulletName = "";
    private Transform _bulletPosition = null;
    private AimCharge _aimCharge = null;
    [SerializeField] private Vector3 _aimDirection = Vector3.zero;
    [SerializeField] private bool _isShoot = false;
    [SerializeField] private float _minPower = 0f;
    [SerializeField] private float _maxPower = 0f;
    [SerializeField] private float _powerTime = 0f;
    [SerializeField] private float _maxPowerTime = 0f;
    [SerializeField] private int c_segmentCnt = 25;
    private Animator _animator = null;
    private bool _isGaugeIncrease = true;

    private void Start()
    {
        _bulletName = ItemName + "Bullet";
        _bulletPosition = transform.Find("bulletPosition");
        _isShoot = false;
    }

    private void Update()
    {
        if (_isUsed && !_isShoot && TurnManager.s_instance.CurrentPlayer.IsLocal)
        {
            if (_isGaugeIncrease)
            {
                if (_powerTime >= _maxPowerTime)
                {
                    _isGaugeIncrease = false;
                    _powerTime = _maxPowerTime;
                }
            }
            else
            {
                if (_powerTime <= 0)
                {
                    _isGaugeIncrease = true;
                    _powerTime = 0;
                }
            }

            if (InputManager.s_instance._shootKeyDown)
            {
                photonView.RPC("SwitchOffAimCharge", RpcTarget.All);
                _animator.SetTrigger("ThrowTrigger");
                StartCoroutine(CallShoot(_aimDirection, _powerTime, 0.5f));
            }
            else
            {
                _powerTime += Time.deltaTime * (_isGaugeIncrease ? 1 : -1);
                _aimCharge._powerRate = _powerTime / _maxPowerTime;
                DrawPredictedTrajectory(_aimDirection, _powerTime);
            }
        }
    }

    [PunRPC]
    private void SwitchOffAimCharge()
    {
        _trajectory.enabled = false;
        _aimCharge.gameObject.SetActive(false);
    }

    public override void UseWeapon(Vector3 aimDirection)
    {
        _isUsed = true;
        _isShoot = false;
        _isGaugeIncrease = true;
        _trajectory.enabled = true;

        _bullet.GetComponent<SpriteRenderer>().enabled = false;
        _bullet.GetComponent<Bullet>().SetBullet(_bulletPosition);
        _bullet.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        _animator = transform.root.GetComponent<DefaultCharacter>()._animator;
        _aimCharge = transform.root.GetComponent<DefaultCharacter>()._aimCharge;
        _aimCharge.gameObject.SetActive(true);
        this._aimDirection = aimDirection;
        _powerTime = 0;
    }

    public override void UseWeaponAI(Vector3 aimDirection, float power)
    {
        TurnManager.s_instance.CurrentCharacter.transform.Find("Body").GetComponent<Animator>().SetTrigger("ReturnIdleTrigger");
        photonView.RPC("ShootAI", RpcTarget.AllBufferedViaServer, aimDirection, power, _bulletPosition.position);
    }

    public override void CancelWeapon()
    {
        StopAllCoroutines();
        _trajectory.enabled = false;
        _isUsed = false;
        _isShoot = false;
        if (_aimCharge)
        {
            _aimCharge.gameObject.SetActive(false);
        }
    }

    private void DrawPredictedTrajectory(Vector3 aimDirection, float powerTime)
    {
        Vector2[] segments = new Vector2[c_segmentCnt];
        segments[0] = _bulletPosition.transform.position;

        float power = _minPower + (_maxPower - _minPower) * (powerTime / _maxPowerTime);
        Vector2 bulletVelocity = new Vector2(aimDirection.x, aimDirection.y) * power;

        for (int i = 1; i < c_segmentCnt; i++)
        {
            float timeCurve = (i * Time.fixedDeltaTime * 5);
            segments[i] = segments[0] + bulletVelocity * timeCurve + 0.5f * Physics2D.gravity * Mathf.Pow(timeCurve, 2);
        }
        _trajectory.positionCount = c_segmentCnt;
        for (int i = 0; i < c_segmentCnt; i++)
        {
            _trajectory.SetPosition(i, segments[i]);
        }
    }

    IEnumerator CallShoot(Vector3 aimDirection, float powerTime, float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        photonView.RPC("Shoot", RpcTarget.All, aimDirection, powerTime, _bulletPosition.position);
    }

    [PunRPC]
    private void Shoot(Vector3 aimDirection, float powerTime, Vector3 bulletPos)
    {
        _isShoot = true;
        _trajectory.positionCount = 0;
        _aimCharge.gameObject.SetActive(false);
        TurnManager.s_instance.DecreaseRemainTime();
        _bullet.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        _bullet.transform.position = bulletPos;
        _bullet.SetActive(true);
        GameManager.s_instance.ChangeCameraMode(CameraController.CameraMode.ShootMode);
        _bullet.GetComponent<SpriteRenderer>().enabled = true;
        _bullet.GetComponent<Rigidbody2D>().AddForce(aimDirection * (_minPower + (_maxPower - _minPower) * (powerTime / _maxPowerTime)), ForceMode2D.Impulse);
    }

    [PunRPC]
    private void ShootAI(Vector3 aimDirection, float power, Vector3 bulletPos)
    {
        _isShoot = true;
        Debug.Log("น฿ป็!!");
        TurnManager.s_instance.DecreaseRemainTime();
        _bullet.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        _bullet.GetComponent<Bullet>().SetBullet(_bulletPosition);
        _bullet.SetActive(true);
        GameManager.s_instance.ChangeCameraMode(CameraController.CameraMode.ShootMode);
        _aimCharge = transform.root.GetComponent<DefaultCharacterAI>()._aimCharge;
        _aimCharge.gameObject.SetActive(false);
        _bullet.GetComponent<SpriteRenderer>().enabled = true;
        _bullet.GetComponent<Rigidbody2D>().velocity = aimDirection * power;
    }
}