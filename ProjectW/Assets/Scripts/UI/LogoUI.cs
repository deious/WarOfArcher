using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoUI : MonoBehaviour
{
    [SerializeField] private float c_logoMoveSpeed = 0.5f;
    public Transform _logoMovePoint;
    public Transform _logoDefalutPoint;

    private int _pivotOffset = -20;
    private Vector3 _logoDefaultPosition;
    private Vector3 _logoPointPosition;
    private bool _logoCangeMove = true; // true일 때 Defalut에서 Point로, false 일 때 Point에서 Defalut로

    private void Start()
    {
        _logoDefaultPosition = _logoDefalutPoint.position;
        _logoPointPosition = new Vector3(_logoMovePoint.position.x + _pivotOffset, _logoMovePoint.position.y, _logoMovePoint.position.z);
    }
    private void FixedUpdate()
    {
        if(transform.position.x >= _logoPointPosition.x)
        {
            _logoCangeMove = false;
        }else if(transform.position.x <= _logoDefaultPosition.x)
        {
            _logoCangeMove = true;
        }
        if(_logoCangeMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, _logoPointPosition, c_logoMoveSpeed);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, _logoDefaultPosition, c_logoMoveSpeed);
        }
    }
}
