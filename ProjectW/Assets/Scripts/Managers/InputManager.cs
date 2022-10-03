using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager s_instance = null;
    private bool _isPlayerTurn = false;

    public float _horizontalAxisRaw = 0f;
    public float _verticalAxisRaw = 0f;
    public Vector3 _mousePos = Vector3.zero;
    public float _mouseScrollAxis = 0f;
    public bool _escKeyDown = false;
    public bool _aimKeyDown = false;
    public bool _jumpKeyDown = false;
    public bool _jumpKey = false;
    public bool _shootKeyStay = false;
    public bool _shootKeyDown = false;
    public bool _inventoryKeyDown = false;
    public bool _backFlipKey = false;
    public bool _backFlipKeydown = false;
    public bool _respawnKeydown = false;
    public bool _tabKey = false;

    public bool _quickSlotN1KeyDown = false;
    public bool _quickSlotN2KeyDown = false;
    public bool _quickSlotN3KeyDown = false;
    public bool _quickSlotN4KeyDown = false;
    public bool _quickSlotN5KeyDown = false;


    private void Awake()
    {
        if (s_instance)
        {
            Debug.Log("InputManager already exist!!");
            return;
        }
        s_instance = this;

    }

    private void Update()
    {
        if (_isPlayerTurn && TurnManager.s_instance.CurrentPlayer.IsLocal && !TurnManager.s_instance.IsAITurn)
        {
            _horizontalAxisRaw = Input.GetAxisRaw("Horizontal");
            _verticalAxisRaw = Input.GetAxisRaw("Vertical");
            _aimKeyDown = Input.GetKeyDown(KeyCode.Space);
            _shootKeyStay = Input.GetKey(KeyCode.Space);
            _shootKeyDown = Input.GetKeyDown(KeyCode.Space);
            _jumpKey = Input.GetKey(KeyCode.LeftControl);
            _jumpKeyDown = Input.GetKeyDown(KeyCode.LeftControl);
            _backFlipKeydown = Input.GetKeyDown(KeyCode.LeftAlt);
            _backFlipKey = Input.GetKey(KeyCode.LeftAlt);

            _quickSlotN1KeyDown = Input.GetKeyDown(KeyCode.Alpha1);
            _quickSlotN2KeyDown = Input.GetKeyDown(KeyCode.Alpha2);
            _quickSlotN3KeyDown = Input.GetKeyDown(KeyCode.Alpha3);
            _quickSlotN4KeyDown = Input.GetKeyDown(KeyCode.Alpha4);
            _quickSlotN5KeyDown = Input.GetKeyDown(KeyCode.Alpha5);
        }
        _tabKey = Input.GetKey(KeyCode.Tab);
        _respawnKeydown = Input.GetKeyDown(KeyCode.Space);
        _escKeyDown = Input.GetKeyDown(KeyCode.Escape);
        _mouseScrollAxis = Input.GetAxis("Mouse ScrollWheel");
        _mousePos = Input.mousePosition;
    }


    public void PreProcess()
    {
        if (TurnManager.s_instance.CurrentCharacter.GetComponent<DefaultCharacter>()._isDeath)
        {
            return;
        }

        _isPlayerTurn = true;
    }
    public void PostProcess()
    {
        _isPlayerTurn = false;
    }

    public void StartPlayerTurn()
    {
        _isPlayerTurn = true;
    }
}
