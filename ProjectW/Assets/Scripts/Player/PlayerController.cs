using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    [SerializeField] private GameObject _avatar = null;
    private DefaultCharacter _avatarClass = null;
    public GameObject Avatar { get { return _avatar; } }
    public DefaultCharacter AvatarClass { get { return _avatarClass; } }
    private bool _isControllerActivated = false;
    private bool _isRespawn = false;
    public bool IsRespawn { get { return _isRespawn; } }

    private const float c_respawnMoveOffset = 0.5f;
    public float RespawnMoveOffset { get { return c_respawnMoveOffset; } }

    public void PreProcess()
    {
        ActivateController();
    }
    public void PostProcess()
    {
        DeactivateController();
    }

    public void CheckRespawnPlayerProcess()
    {
        ChangeControllerCharacter();
        _avatarClass = _avatar.GetComponent<DefaultCharacter>();
        if (_avatarClass._isDeath)
        {
            _isRespawn = true;
        }
    }
    private void ChangeControllerCharacter()
    {
        _avatar = TurnManager.s_instance.CurrentCharacter;
    }

    private void ActivateController()
    {
        _isControllerActivated = true;
        if (_isRespawn)
        {
            GameManager.s_instance.CallRespawn(TurnManager.s_instance.CurrentPlayer.NickName);
        }
        else
        {
            _avatarClass.PreProcess();
        }
    }
    private void DeactivateController()
    {
        _avatarClass.ReturnDefaultState();
        _isControllerActivated = false;
    }
    private void FixedUpdate()
    {
        if (_avatarClass && _isRespawn && _isControllerActivated)
        {
            _avatar.transform.position = new Vector2(_avatar.transform.position.x - c_respawnMoveOffset, _avatar.transform.position.y);
        }
    }
    private void Update()
    {
        /// Turn Check
        if (!_isControllerActivated || !_avatar || !_avatar.GetPhotonView().IsMine)
        {
            return;
        }

        if (_isRespawn)
        {
            if (InputManager.s_instance._respawnKeydown)
            {
                photonView.RPC("RespawnCharacter", RpcTarget.All);
            }

            //_avatar.transform.position = Vector3.Lerp(_avatar.transform.position, PlayerRespawn.RespawnEndPoint.position, 0.0003f);

            if (Vector3.Distance(_avatar.transform.position, PlayerRespawn.RespawnEndPoint.position) <= 1.0f)
            {
                photonView.RPC("RespawnCharacter", RpcTarget.All);
            }
            return;
        }

        Control();
        Act();
    }

    private void Control()
    {
        if (InputManager.s_instance._shootKeyDown)
        {
            _avatarClass.ShootKeyDown();
        }

        if (InputManager.s_instance._quickSlotN1KeyDown)
        {
            GameManager.s_instance.SwitchQuickItem(0);
        }
        if (InputManager.s_instance._quickSlotN2KeyDown)
        {
            GameManager.s_instance.SwitchQuickItem(1);
        }
        if (InputManager.s_instance._quickSlotN3KeyDown)
        {
            GameManager.s_instance.SwitchQuickItem(2);
        }
    }

    private void Act()
    {
        if (!_avatarClass.GetCurrentPlayerState(DefaultCharacter.PlayerState.IsAttack))
        {
            _avatarClass.Move(InputManager.s_instance._horizontalAxisRaw, InputManager.s_instance._jumpKeyDown, InputManager.s_instance._backFlipKeydown);
            _avatarClass.Aim(InputManager.s_instance._verticalAxisRaw);
        }
    }

    [PunRPC]
    private void RespawnCharacter()
    {
        _avatarClass.Respawn();
        GameManager.s_instance.CallRespawn();
        _isRespawn = false;
    }
}