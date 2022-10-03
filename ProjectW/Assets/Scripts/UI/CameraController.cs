using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



public class CameraController : MonoBehaviour
{
    public enum CameraMode { DefaultMode, PlayerMode, ShootMode, RespawnMode, LastMode }

    [SerializeField] private Camera _camera = null;
    [SerializeField] private Vector3 _velocity = Vector3.zero;
    [SerializeField] private Vector3 _cameraDestination = Vector3.zero;
    [SerializeField] private CameraMode _cameraMode;

    [Header("CameraMove")]
    [SerializeField] private GameObject _cameraTarget = null;
    [SerializeField] private float _currentSize = 0f;
    [SerializeField] private bool _isFocusTarget = false;
    [Space(50)]

    [Header("Edge")]
    [SerializeField] private float _mouseSleepTime = 0f;
    [SerializeField] private float _mapPivotX = 0f;
    [SerializeField] private float _mapPivotY = 0f;
    [SerializeField] private float _mapAvailableWidth = 0f;
    [SerializeField] private float _mapAvailableHeight = 0f;
    [SerializeField] private float _screenHalfHeight = 0f;
    [SerializeField] private float _screenHalfWidth = 0f;
    [Space(50)]

    [Header("State")]

    [SerializeField] private Data _data = null;
    [SerializeField] private Vector3 _mousePoint = Vector3.zero;
    [SerializeField] private Vector3 _preMousePoint = Vector3.zero;
    [SerializeField] private float _mouseScrollWheel = 0f;
    [SerializeField] private int _width = 1920, _height = 1080;
    [SerializeField] private Vector2 destinFromMapPivot = Vector2.zero;
    [SerializeField] private bool _isActiveFocus = true;


    [System.Serializable]
    private class Data
    {
        [Header("CameraMove")]
        public Vector3 _offset = Vector3.zero;
        public Vector3 _dropPosOffset = Vector3.zero;
        public float _defaultSize = 0f;
        public float _minSize = 0f;
        public float _maxSize = 0f;
        public float _halfSize = 0f;
        public float _smoothTime = 0f;
        [Space(50)]

        [Header("Screen")]
        public Vector2 _screenBorderRate = Vector2.zero;
        public Vector2 _mapOffset = Vector2.zero;


        public float _maxSleepTime = 0f;
        public float _speedInScreenBorder = 0f;
        [Space(50)]

        [Header("Control")]
        public float _scrollPower = 0f;
        public float _zoomCancelTime = 0f;
    }

    public void Init()
    {
        _camera = GetComponent<Camera>();
        _camera.orthographic = true;
        _currentSize = _data._defaultSize;
        _mapPivotX = MapManager.s_instance.BackgroundPositionX;
        _mapPivotY = MapManager.s_instance.BackgroundPositionY;
        _data._halfSize = (_data._maxSize + _data._minSize) * 0.5f;

        Screen.SetResolution(_width, _height, true);
    }
    public void PreProcess()
    {
        if (TurnManager.s_instance.CurrentCharacter.GetComponent<DefaultCharacter>()._isDeath)
        {
            return;
        }
        ChangeMode(CameraMode.PlayerMode);
    }
    public void PostProcess()
    {
        ChangeMode(CameraMode.DefaultMode);
    }


    private void LateUpdate()
    {
        if (!_cameraTarget)
        {
            return;
        }

        UpdateCameraSettings();

        UpdateCameraZoom();
        UpdateScreenBorder();
        UpdateTarget();

        GoDestination();
    }


    private void UpdateCameraSettings()
    {
        _mousePoint = InputManager.s_instance._mousePos;
        _mouseScrollWheel = InputManager.s_instance._mouseScrollAxis * _data._scrollPower;

        _screenHalfHeight = _camera.orthographicSize;
        _screenHalfWidth = _screenHalfHeight * Screen.width / Screen.height;

        _mapAvailableWidth = MapManager.s_instance.SpriteX - _data._mapOffset.x * 2 - _screenHalfWidth * 2;
        _mapAvailableHeight = MapManager.s_instance.SpriteY - _data._mapOffset.y * 2 - _screenHalfHeight * 2;

        destinFromMapPivot = new Vector2(_cameraDestination.x - _mapPivotX, _cameraDestination.y - _mapPivotY);

        _isFocusTarget = IsPlayerNotAFK();
    }

    private void UpdateCameraZoom()
    {
        if (_mouseScrollWheel != 0 && _isActiveFocus)
        {
            _cameraDestination = transform.position + (_mousePoint - new Vector3(Screen.width / 2, Screen.height / 2)) / Screen.height * _camera.orthographicSize;
            _currentSize -= _mouseScrollWheel;
        }

        if (_currentSize < _data._minSize)
        {
            _currentSize = _data._minSize;
        }
        else if (_currentSize > _data._maxSize)
        {
            _currentSize = _data._maxSize;
        }
    }


    //TODO : 함수 이름 조정 필요
    private bool IsPlayerNotAFK()
    {
        if (_mouseScrollWheel != 0 || IsOutScreenContent())
        {
            _mouseSleepTime = 0;
            return false;
        }

        switch (_cameraMode)
        {
            case CameraMode.DefaultMode:
                break;
            case CameraMode.PlayerMode:
                _mouseSleepTime += Time.deltaTime;
                return _mouseSleepTime > _data._maxSleepTime;
            case CameraMode.ShootMode:
                if (_cameraTarget.gameObject.activeSelf)
                {
                    return true;
                }
                break;
            case CameraMode.RespawnMode:
                break;
            case CameraMode.LastMode:
                break;
            default:
                break;
        }
        return false;
    }

    private float CurrentOffsetFromTarget()
    {
        return Vector2.Distance(LimitMapBorder(_cameraTarget.transform.position), transform.position);
    }

    private void UpdateScreenBorder()
    {
        Vector3 moveDir = (_mousePoint - new Vector3(Screen.width / 2, Screen.height / 2, 0)).normalized;
        moveDir.z = 0;
        if (IsOutScreenContent())
        {
            _cameraDestination += moveDir * _data._speedInScreenBorder * Time.deltaTime;
        }
    }

    private bool IsOutScreenContent()
    {
        return _mousePoint.x < (_data._screenBorderRate.x * Screen.width) || _mousePoint.x > ((1 - _data._screenBorderRate.x) * Screen.width)
            || _mousePoint.y < (_data._screenBorderRate.y * Screen.height) || _mousePoint.y > ((1 - _data._screenBorderRate.y) * Screen.height);
    }

    private Vector3 LimitMapBorder(Vector3 pos)
    {
        if (Mathf.Abs(destinFromMapPivot.x) > (_mapAvailableWidth / 2))
        {
            pos.x = _mapPivotX + (_mapAvailableWidth / 2) * (destinFromMapPivot.x > 0 ? 1 : -1);
        }
        if (Mathf.Abs(destinFromMapPivot.y) > (_mapAvailableHeight / 2))
        {
            pos.y = _mapPivotY + (_mapAvailableHeight / 2) * (destinFromMapPivot.y > 0 ? 1 : -1);
        }
        pos.z = _data._offset.z;
        return pos;
    }


    private void GoDestination()
    {
        transform.position = Vector3.SmoothDamp(transform.position, LimitMapBorder(_cameraDestination), ref _velocity, _data._smoothTime);
        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _currentSize, Time.deltaTime * _data._zoomCancelTime);
    }

    private void UpdateTarget()
    {
        if (!_isFocusTarget)
        {
            return;
        }
        _cameraDestination = _cameraTarget.transform.position;
    }

    public void ChangeMode(CameraMode cameraMode)
    {
        _cameraTarget = null;
        _isActiveFocus = true;
        switch (this._cameraMode = cameraMode)
        {
            case CameraMode.DefaultMode:
                _cameraTarget = gameObject;
                _currentSize = _data._maxSize;
                break;
            case CameraMode.PlayerMode:
                if (TurnManager.s_instance.IsAITurn)
                {
                    _cameraTarget = GameManager.s_instance.AIController.gameObject;
                }
                else
                {
                    _cameraTarget = TurnManager.s_instance.CurrentCharacter;
                }
                if (_currentSize > _data._halfSize)
                {
                    _currentSize = _data._halfSize;
                }
                break;
            case CameraMode.ShootMode:
                foreach (Transform bullet in ItemStorage.s_instance.BulletStorage)
                {
                    if (bullet.gameObject.activeSelf)
                    {
                        _cameraTarget = bullet.gameObject;
                        break;
                    }
                }
                if (_currentSize < _data._halfSize)
                {
                    _currentSize = _data._halfSize;
                }
                break;
            case CameraMode.RespawnMode:
                if (TurnManager.s_instance.IsAITurn)
                {
                    _cameraTarget = GameManager.s_instance.AIController.gameObject;
                }
                else
                {
                    _cameraTarget = TurnManager.s_instance.CurrentCharacter;
                }
                if (_currentSize < _data._halfSize)
                {
                    _currentSize = _data._halfSize;
                }
                StartCoroutine(WaitForConvertToPlayerMode());
                break;
            case CameraMode.LastMode:
                _cameraTarget = gameObject;
                break;
            default:
                break;
        }
        _isFocusTarget = true;
        UpdateTarget();
    }
    public void CeaseZoomInOut()
    {
        _isActiveFocus = false;
    }
    private IEnumerator WaitForConvertToPlayerMode()
    {
        yield return new WaitForSeconds(2.0f);
        ChangeMode(CameraMode.PlayerMode);
    }
}