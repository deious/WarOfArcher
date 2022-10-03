using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : WeaponItem
{
    [System.Serializable]
    private class Data
    {
        public int _damage = 0;
        public float _swingRange = 0f;
        public float _swingPower = 0f;
        public float _swingPowerDamp = 0f;
    }

    [SerializeField] private Data _data = null;

    private RaycastHit2D[] _playersInRange = null;
    private Rigidbody2D _rigidBody2D = null;
    private Rigidbody2D _currenPlayerRigidBody = null;
    private float _targetDistance = 0f;

    public override void UseWeapon(Vector3 aimDirection)
    {
        _isUsed = true;
        _playersInRange = Physics2D.LinecastAll(transform.root.position, transform.root.position + aimDirection * _data._swingRange);
        _currenPlayerRigidBody = transform.root.GetComponent<Rigidbody2D>();
        TurnManager.s_instance.DecreaseRemainTime();
        Shoot(aimDirection);
    }

    public override void UseWeaponAI(Vector3 aimDirection, float power)
    {
        _isUsed = true;
        _playersInRange = Physics2D.LinecastAll(transform.root.position, transform.root.position + aimDirection * _data._swingRange);
        _currenPlayerRigidBody = transform.root.GetComponent<Rigidbody2D>();
        TurnManager.s_instance.DecreaseRemainTime();
        Shoot(aimDirection);
    }


    private void Shoot(Vector3 aimDirection)
    {
        for (int i = 0; i < _playersInRange.Length; i++)
        {
            _rigidBody2D = _playersInRange[i].transform.gameObject.GetComponent<Rigidbody2D>();
            if (_playersInRange[i].collider.tag != "Player" || _rigidBody2D == _currenPlayerRigidBody)
            {
                continue;
            }
            _targetDistance = (_playersInRange[i].transform.position - transform.root.position).magnitude;
            _rigidBody2D.velocity = Vector2.zero;
            if (_data._swingRange > _targetDistance)
            {
                _rigidBody2D.AddForce(aimDirection * _data._swingPower * Mathf.Pow(_data._swingPowerDamp, _targetDistance), ForceMode2D.Impulse);
                if (TurnManager.s_instance.CurrentPlayer.IsLocal)
                {
                    _playersInRange[i].transform.root.GetComponent<DefaultCharacter>().CallHit((int)(_data._damage * (1 - (_targetDistance / _data._swingRange))));
                }
            }
        }
    }

    public override void CancelWeapon()
    {
        _isUsed = false;
    }
}
