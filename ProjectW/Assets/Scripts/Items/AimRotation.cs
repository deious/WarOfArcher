using UnityEngine;
using System.Collections;

public class AimRotation : MonoBehaviour
{
    public GameObject _handConstraint;
    [SerializeField] private float _minAngle;
    [SerializeField] private float _maxAngle;
    [SerializeField] private float _angle;

    private bool _isHaveHand = false;
    private GameObject _aimingPoint = null;
    [System.Serializable]
    public class Data
    {
        public float _aimAngleSpeed = 0f;
    }

    [SerializeField] private Data _data = null;


    public GameObject AimingPoint { get { return _aimingPoint; } }

    private void Awake()
    {
        _aimingPoint = transform.Find("AimingPoint").gameObject;
        if (!_aimingPoint)
        {
            Debug.LogError("AimingPoint�� �����ϴ�.");
            return;
        }
        _isHaveHand = (_handConstraint != null);
        if(!_isHaveHand)
        {
            Debug.Log("�� ĳ���Ϳ��� handConstraint (ArmIK) �� �����ϴ�.");
            return;
        }
    }

    private void Update()
    {
        if (_isHaveHand)
        {
            _handConstraint.transform.position = _aimingPoint.transform.position;
        }
    }

    public void Aim(float yInput, int dir)
    {
        _angle = GetAngle(dir);
        if (yInput > 0 && _angle <= _maxAngle)
        {
            transform.RotateAround(transform.position, Vector3.forward, _data._aimAngleSpeed * yInput * dir * Time.deltaTime);
        }
        else if(yInput < 0 && _angle >= _minAngle)
        {
            transform.RotateAround(transform.position, Vector3.forward, _data._aimAngleSpeed * yInput * dir * Time.deltaTime);
        }

    }

    public void AimAI(bool isAnglePlus, int dir)
    {
        //_handConstraint.transform.position = _aimingPoint.transform.position; //������ ������ ������ �����Ƿ�

        if (isAnglePlus)
        {
            transform.RotateAround(transform.position, Vector3.forward, _data._aimAngleSpeed * dir * Time.deltaTime);
        }
        else 
        {
            transform.RotateAround(transform.position, Vector3.back, _data._aimAngleSpeed * dir * Time.deltaTime);
        }
    }

    public float GetAngle(int dir)
    {
        Vector2 pos = (_aimingPoint.transform.position - transform.position);
        pos.x *= (dir > 0 ? 1 : -1);
        return Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
    }
}