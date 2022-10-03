using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;

public class Rope : DefaultBullet
{
    public GameObject _owner = null;
    public GameObject _ownerRotationCenter = null;
    public bool _isHooked = false;
    private Rigidbody2D _ownerRigidBody = null;
    [SerializeField] private int _groundLayerMask = 0;
    [SerializeField] private Data _data = null;

    [System.Serializable]
    private class Data
    {
        public float _cancelDamp = 0f;
        public float _minLength = 0f;
        public float _maxLength = 0f;
        public float _climbSpeed = 0f;
        public float _maxLinecastOffset = 0f;
        public float _stepLinecastOffset = 0f;
        public float _swingPower = 0f;
        public float _stepLength = 0f;
        public float _defaultSwingPower = 0f;
    }


    [SerializeField] private List<Vector2> _anchorsList = new List<Vector2>();
    private Vector2 _preAnchorDir = Vector2.zero;
    private Vector2 _nextAnchorDir = Vector2.zero;
    private Vector2 _ownerPos = Vector2.zero;

    public DistanceJoint2D _ownerDistacnceJoint = null;
    private LineRenderer _ropeLineRenderer = null;
    [SerializeField] private List<float> _combinedAnchorLen = new List<float>();
    [SerializeField] private Material _material;
    private RaycastHit2D _firstHitRaycast2D;

    public bool CanThrow
    {
        get
        {
            if (_owner)
            {
                return Vector2.Distance(_ownerRotationCenter.transform.position, transform.position) < _data._maxLength;
            }
            else
            {
                Debug.LogWarning("현재 이 로프에는 주인이 없습니다");
                return false;
            }
        }
    }

    private void Awake()
    {
        _ropeLineRenderer = gameObject.GetComponent<LineRenderer>();
        _ropeLineRenderer.material = _material;
        _groundLayerMask = 1 << LayerMask.NameToLayer("Ground");
    }

    private void OnEnable()
    {
        if (!_owner)
        {
            Debug.LogError("이 로프 오브젝트의 주인이 없습니다");
            return;
        }

        if (!(_ownerDistacnceJoint = _owner.GetComponent<DistanceJoint2D>()))
        {
            Debug.Log($"이 {_owner.name} 플레이어에게 DistacneJoint 컴포넌트가 없습니다");
            return;
        }
        _ownerDistacnceJoint.anchor = Vector2.up;
        _ownerDistacnceJoint.connectedBody = GetComponent<Rigidbody2D>();

        _ownerRigidBody = _owner.GetComponent<Rigidbody2D>();

        _ropeLineRenderer.enabled = true;
        _ropeLineRenderer.positionCount = 2;
        _ropeLineRenderer.SetPosition(0, Vector3.zero);
        _ropeLineRenderer.SetPosition(1, Vector3.zero);

        _combinedAnchorLen.Clear();
        _anchorsList.Clear();
    }
    private void OnDisable()
    {
        if (_ownerDistacnceJoint)
        {
            _ownerDistacnceJoint.enabled = false;
            _ownerDistacnceJoint.distance = _data._maxLength;
            _ownerDistacnceJoint.connectedBody = null;
        }

        if (_anchorsList.Count > 0)
        {
            Vector2 swingNormalDir = SwingNormalDir(_ownerRotationCenter.transform.position);
            _ownerRigidBody.velocity = swingNormalDir * Vector2.Dot(swingNormalDir, _ownerRigidBody.velocity) * _data._cancelDamp;
            //취소 시 근사 속도로 변경
        }

        _owner = null;
        _ownerRigidBody = null;
        _ownerRotationCenter = null;

        _isHooked = false;
        _ropeLineRenderer.enabled = false;
    }

    private void Update()
    {
        if (!_owner)
        {
            return;
        }

        _ownerPos = _ownerRotationCenter.transform.position;

        ManageRopeLine();

        if (!_isHooked)
        {
            TryHook();
        }
        else
        {
            ManageAnchors();
            ControlRope();
            ForceCancel();
        }
    }

    private void TryHook()
    {
        if (!(_firstHitRaycast2D = Physics2D.Linecast(_ownerPos, transform.position, _groundLayerMask)))
        {
            return;
        }
        photonView.RPC("CompleteHook", RpcTarget.All, HookedPos(), _ownerPos);
    }

    [PunRPC]
    private void CompleteHook(Vector2 firstHitPos, Vector2 ownerPos)
    {
        transform.position = firstHitPos;
        _isHooked = true;
        _ownerRigidBody.gravityScale = 0f;
        _ownerDistacnceJoint.distance = Vector2.Distance(transform.position, ownerPos);
        if (TurnManager.s_instance.CurrentPlayer.IsLocal)
        {
            _ownerDistacnceJoint.enabled = true;
        }
        AddAnchor(firstHitPos, ownerPos);
    }

    private Vector2 HookedPos()
    {
        int repeat = (int)(_data._maxLinecastOffset / _data._stepLinecastOffset) + 1;
        for (int i = 0; i < repeat; i++)
        {
            if (!Physics2D.Linecast(_ownerRotationCenter.transform.position, _firstHitRaycast2D.point + _data._stepLinecastOffset * i * _firstHitRaycast2D.normal.normalized, _groundLayerMask))
            {
                return _firstHitRaycast2D.point + (_firstHitRaycast2D.normal.normalized * _data._stepLinecastOffset * i);
            }
        }
        return _firstHitRaycast2D.point + (_firstHitRaycast2D.normal.normalized * _data._maxLinecastOffset);
    }
    private void ManageAnchors()
    {
        transform.position = _anchorsList.Last();

        if (ShouldAddAnchor())
        {
            photonView.RPC("AddAnchor", RpcTarget.All, _nextAnchorDir, _ownerPos);
        }

        if (CanDeleteAnchor())
        {
            photonView.RPC("DeleteAnchor", RpcTarget.All, _ownerPos);
        }
    }

    private void ManageRopeLine()
    {
        if (_anchorsList.Count == 0)
        {
            _ropeLineRenderer.SetPosition(0, transform.position);
            _ropeLineRenderer.SetPosition(1, _ownerPos);
        }
        else
        {
            _ropeLineRenderer.SetPosition(0, _anchorsList.First());
            _ropeLineRenderer.SetPosition(_anchorsList.Count, _ownerPos);
        }
    }
    private bool ShouldAddAnchor()
    {
        RaycastHit2D hit;
        if (!(hit = Physics2D.Linecast(_ownerPos, _anchorsList.Last(), _groundLayerMask)))
        {
            return false;
        }
        int repeat = (int)(_data._maxLinecastOffset / _data._stepLinecastOffset) + 1;
        for (int i = 0; i < repeat; i++)
        {
            Vector3 nextPos = hit.point + _data._stepLinecastOffset * i * hit.normal.normalized;
            if (!Physics2D.Linecast(_ownerPos, nextPos, _groundLayerMask) && !Physics2D.Linecast(nextPos, _anchorsList.Last(), _groundLayerMask))
            {
                _nextAnchorDir = hit.point + _data._stepLinecastOffset * i * hit.normal.normalized;
                return true;
            }
        }
        return false;
    }

    private bool CanDeleteAnchor()
    {
        if (_anchorsList.Count <= 2)
        {
            return false;
        }
        _preAnchorDir = _anchorsList[_anchorsList.Count - 2];
        Vector2 dir = _anchorsList.Last() - _anchorsList[_anchorsList.Count - 2];
        Vector2 _stepDir = dir.normalized * _data._stepLength;
        for (int i = 1; i < dir.magnitude / _stepDir.magnitude; i++)
        {
            _preAnchorDir += _stepDir;
            if (Physics2D.Linecast(_ownerPos, _preAnchorDir, _groundLayerMask))
            {
                return false;
            }
            if (_data._stepLength * i >= (_anchorsList[_anchorsList.Count - 2] - _anchorsList.Last()).magnitude)
            {
                return true;
            }
        }
        return true;
    }
    private void ControlRope()
    {
        ControlRopeLength();
        ControlRopeSwing();
    }

    private void ControlRopeLength()
    {
        if (_ownerDistacnceJoint.distance + _combinedAnchorLen.Sum() > _data._maxLength)
        {
            _ownerDistacnceJoint.distance = _data._maxLength - _combinedAnchorLen.Sum();
        }
        else if (_ownerDistacnceJoint.distance + _combinedAnchorLen.Sum() < _data._minLength)
        {
            _ownerDistacnceJoint.distance = _data._minLength - _combinedAnchorLen.Sum();
        }
        _ownerDistacnceJoint.distance -= InputManager.s_instance._verticalAxisRaw * _data._climbSpeed * Time.deltaTime;
    }

    private void ControlRopeSwing()
    {
        if (InputManager.s_instance._horizontalAxisRaw == 0)
        {
            return;
        }

        Vector2 swingDir = SwingNormalDir(_ownerPos);
        Vector2 ropeForce = (_data._swingPower * (_ownerDistacnceJoint.distance + _combinedAnchorLen.Sum()) + _data._defaultSwingPower) * swingDir * InputManager.s_instance._horizontalAxisRaw * Time.deltaTime;
        if (Vector2.Dot(swingDir * InputManager.s_instance._horizontalAxisRaw, _ownerRigidBody.velocity) < 0)
        {
            ropeForce *= 2f;
        }
        _ownerRigidBody.AddForce(ropeForce);
    }

    private Vector2 SwingNormalDir(Vector2 ownerPos)
    {
        Vector2 lastAnchorPosFromOwner = _anchorsList.Last() - ownerPos;
        return (Quaternion.AngleAxis(-90, Vector3.forward) * lastAnchorPosFromOwner).normalized;
    }

    [PunRPC]
    private void AddAnchor(Vector2 pos, Vector2 ownerPos)
    {
        _anchorsList.Add(pos);
        if (_anchorsList.Count > 1)
        {
            _combinedAnchorLen.Add(Vector2.Distance(_anchorsList.Last(), _anchorsList[_anchorsList.Count - 2]));
        }
        SetLastAnchor(ownerPos);
    }

    [PunRPC]
    private void DeleteAnchor(Vector2 ownerPos)
    {
        _combinedAnchorLen.RemoveAt(_combinedAnchorLen.Count - 1);
        _anchorsList.RemoveAt(_anchorsList.Count - 1);
        SetLastAnchor(ownerPos);
    }

    private void SetLastAnchor(Vector3 ownerPos)
    {
        _ownerDistacnceJoint.distance = Vector2.Distance(ownerPos, _anchorsList.Last());
        _ownerDistacnceJoint.anchor = Vector2.up;
        _ownerDistacnceJoint.connectedAnchor = Vector2.zero;
        DrawRopeLine(ownerPos);
    }

    private void ForceCancel()
    {
        if (Vector2.Distance(transform.position, _ownerPos) + _combinedAnchorLen.Sum() <= _data._maxLength)
        {
            return;
        }
        Debug.Log("로프 강제 취소");
        _owner.GetComponent<DefaultCharacter>().CallCancelItem();
    }

    private void DrawRopeLine(Vector3 ownerPos)
    {
        _ropeLineRenderer.positionCount = _anchorsList.Count + 1;
        _ropeLineRenderer.SetPosition(_anchorsList.Count - 1, _anchorsList.Last());
        _ropeLineRenderer.SetPosition(_anchorsList.Count, ownerPos);
    }
}
