using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JetPack : UtilityItem
{
    [SerializeField] private Rigidbody2D _ownerBody = null;
    [SerializeField] private Data _data = null;

    private Transform _background = null;
    private Transform _fill = null;
    private Transform _jetpackEffect = null;

    private float _useTime = 0;
    private Vector3 _backgroundScale = Vector3.zero;
    private Vector3 _effectScale = Vector3.zero;

    private DefaultCharacter _character = null;

    [System.Serializable]
    private class Data
    {
        public float _verticalForce = 0f;
        public float _horizontalForce = 0f;
        public float _useMaxTime = 0;
        public Vector3 _gaugeOffset = Vector3.zero;
        public Vector3 _effectOffset = Vector3.zero;
    }
    private void Start()
    {
        _background = transform.Find("BackGround");
        _fill = transform.Find("Fill");
        _jetpackEffect = transform.Find("JetPackEffect");

        _backgroundScale = _background.transform.localScale;
        _effectScale = _jetpackEffect.transform.localScale;

        FillFuel();

        if (!_background)
        {
            Debug.LogError("BackGround 오브젝트가 없습니다");
        }
        if (!_fill)
        {
            Debug.LogError("Fill 오브젝트가 없습니다");
        }
        if (!_jetpackEffect)
        {
            Debug.LogError("JetPackEffect 오브젝트가 없습니다");
        }
    }
    public override void UseUtility(Vector3 aimDirection)
    {
        if (_useTime <= 0)
        {
            CancelUtility();
        }
        else
        {
            _isUsed = true;
            _ownerBody = transform.root.GetComponent<Rigidbody2D>();
            _character = transform.root.GetComponent<DefaultCharacter>();
            gameObject.SetActive(true);
        }
    }
    public override void CancelUtility()
    {
        _isUsed = false;
        gameObject.SetActive(false);
    }
    private void Update()
    {
        if (_isUsed)
        {
            if (_useTime > 0)
            {
                if (InputManager.s_instance._verticalAxisRaw > 0)
                {
                    _useTime -= Time.deltaTime;
                    _ownerBody.AddForce(Vector2.up * _data._verticalForce * Time.deltaTime);
                }
                if (InputManager.s_instance._horizontalAxisRaw != 0)
                {
                    _ownerBody.AddForce(Vector2.right * _data._horizontalForce * InputManager.s_instance._horizontalAxisRaw * Time.deltaTime);
                }
            }
            else
            {
                CancelUtility();
            }
            _fill.localScale = new Vector3(_character.GetDirection() > 0 ? _backgroundScale.x : (-_backgroundScale.x), _backgroundScale.y * (_useTime / _data._useMaxTime), _backgroundScale.z);
            _jetpackEffect.localScale = new Vector3(_character.GetDirection() > 0 ? _effectScale.x : (-_effectScale.x), _effectScale.y, _effectScale.z);
            _jetpackEffect.position = transform.root.position + _data._effectOffset;
            _background.transform.position = transform.root.position + _data._gaugeOffset;
            _fill.transform.position = _background.transform.position + (_backgroundScale.y * (1 - (_useTime / _data._useMaxTime)) * 0.5f * Vector3.down);
        }
    }

    public void FillFuel()
    {
        _useTime = _data._useMaxTime;
    }
}
