using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimCharge : MonoBehaviour
{
    private Transform _background = null;
    private Transform _fill = null;
    private Transform _minCircleMask = null;
    public float _powerRate = 0f;

    private float _minScale = 0f;
    private float _maxScale = 0f;

    public void Init()
    {
        _background = transform.Find("BackGround");
        _fill = transform.Find("Fill");
        _minCircleMask = transform.Find("CircleMask");

        if (!_background)
        {
            Debug.LogError("BackGround 오브젝트가 없습니다");
            return;
        }
        if (!_fill)
        {
            Debug.LogError("Fill 오브젝트가 없습니다");
            return;
        }
        if(!_minCircleMask)
        {
            Debug.LogError("CircleMask 오브젝트가 없습니다");
            return;
        }
        _minScale = _minCircleMask.localScale.x;
        _maxScale = _background.localScale.x;
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    private void Update()
    {
        if (_fill)
        {
            if (TurnManager.s_instance.CurrentPlayer.IsLocal || TurnManager.s_instance.IsAITurn)
            {
                _fill.localScale = new Vector3(1, 1, 1) * ((_maxScale - _minScale) * _powerRate + _minScale);
            }
        }
    }
}
