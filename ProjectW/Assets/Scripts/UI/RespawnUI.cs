using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class RespawnUI : MonoBehaviour
{
    [SerializeField] private Transform _markDirection = null;
    [SerializeField] private TextMeshProUGUI _playerName = null;
    [SerializeField] private TextMeshProUGUI _respawnKeyDownInfo = null;
    private Vector2 _targetPostion;
    private RectTransform _respawnUIRectTransform;
    [SerializeField] private Vector3 _defalutPosition;
    private bool _isAIRespawn = false;
    private const float c_rotateSpeed = 1.0f;
    private float _respawnMoveOffset;
    public bool _isStartRespawn = false;

    private void Start()
    {
        _respawnUIRectTransform = GetComponent<RectTransform>();
        _defalutPosition = _respawnUIRectTransform.localPosition;
    }
    public void SetRespawnPlayerInfo()
    {
        _respawnUIRectTransform.localPosition = _defalutPosition;

        _playerName.text = "<color=blue>Alpha</color>";
        _respawnKeyDownInfo.text = "";
        _isAIRespawn = true;
        _respawnMoveOffset = GameManager.s_instance.AIController.RespawnMoveOffset;
    }
    public void SetRespawnPlayerInfo(string playerName)
    {
        _respawnUIRectTransform.localPosition = _defalutPosition;

        _playerName.text = playerName;
        _respawnKeyDownInfo.text = "스페이스바를 눌러 떨어집니다";
        _isAIRespawn = false;
        _respawnMoveOffset = GameManager.s_instance.PlayerController.RespawnMoveOffset;
    }
    private void FixedUpdate()
    {
        if (_isStartRespawn)
        {
            Vector3 _targetPostion;
            if (_isAIRespawn)
            {
                _targetPostion = GameManager.s_instance.AIController.transform.position;
            }
            else
            {
                _targetPostion = TurnManager.s_instance.CurrentCharacter.transform.position;
            }


            //플레이어 이동에 따른 UI 이동 처리
            Vector3 worldpos = Camera.main.WorldToViewportPoint(_targetPostion);
            if(worldpos.x < 0.95f && worldpos.x > 0.05f)
            {
                if(_markDirection.rotation != Quaternion.Euler(0,0,0))
                {
                    Quaternion rotation = Quaternion.RotateTowards(_markDirection.rotation, Quaternion.Euler(0,0,0), 1.0f);
                    _markDirection.rotation = rotation;
                }
                Vector2 targetPosX = new Vector2(_targetPostion.x, _respawnUIRectTransform.position.y);
                transform.position = targetPosX;

            }
            else
            {
                //플레이어 지점으로 마크 가르키는 방향 계산
                Vector2 direction = new Vector2(_markDirection.position.x - _targetPostion.x,
                                    _markDirection.position.y - _targetPostion.y);

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion angleAxis = Quaternion.AngleAxis(angle + 90f, Vector3.forward);
                Quaternion rotation = Quaternion.RotateTowards(_markDirection.rotation, angleAxis,1.0f);
                _markDirection.rotation = rotation;

                if(worldpos.x > 0.95f)
                {
                    worldpos.x = 0.95f;
                    worldpos = Camera.main.ViewportToWorldPoint(worldpos);
                    worldpos = new Vector3(worldpos.x, transform.position.y, transform.position.z);
                    transform.position = worldpos;
                }
                else
                {
                    worldpos.x = 0.05f;
                    worldpos = Camera.main.ViewportToWorldPoint(worldpos);
                    worldpos = new Vector3(worldpos.x, transform.position.y, transform.position.z);
                    transform.position = worldpos;
                }
            }
        }
    }



}
