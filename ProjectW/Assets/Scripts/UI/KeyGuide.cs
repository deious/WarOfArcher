using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyGuide : MonoBehaviour
{
    private Transform _upDown= null;
    private Transform _leftRight= null;
    private Transform _theOthers= null;

    private Image _up = null;
    private Image _down= null;
    private Image _left= null;
    private Image _right= null;
    private Image _space= null;
    private Image _jump= null;
    private Image _backflip = null;
    private Image[] _mouseImage = null;

    private float _activeAlpha = 0.4f;
    private float _mouseTime = 0f;

    void Start()
    {
        _upDown = transform.Find("UpDown");
        _leftRight = transform.Find("LeftRight");
        _theOthers = transform.Find("TheOthers");

        _up = _upDown.Find("UpKey").GetComponent<Image>();
        _down = _upDown.Find("DownKey").GetComponent<Image>();
        _left = _leftRight.Find("LeftKey").GetComponent<Image>();
        _right = _leftRight.Find("RightKey").GetComponent<Image>();

        _space = _theOthers.Find("SpaceKey").GetComponent<Image>();
        _jump = _theOthers.Find("JumpKey").GetComponent<Image>();
        _backflip = _theOthers.Find("BackFlipKey").GetComponent<Image>();

        _mouseImage = new Image[2];
        _mouseImage[0] = _theOthers.Find("MouseKey").Find("Enable").GetComponent<Image>();
        _mouseImage[1] = _theOthers.Find("MouseKey").Find("Disable").GetComponent<Image>();

        _mouseImage[0].enabled = _mouseImage[1].enabled = false;
    }

    void Update()
    {
        Active(_up, InputManager.s_instance._verticalAxisRaw > 0);
        Active(_down, InputManager.s_instance._verticalAxisRaw < 0);
        Active(_left, InputManager.s_instance._horizontalAxisRaw < 0);
        Active(_right, InputManager.s_instance._horizontalAxisRaw > 0);
        Active(_space, InputManager.s_instance._shootKeyStay);
        Active(_jump, InputManager.s_instance._jumpKey);
        Active(_backflip, InputManager.s_instance._backFlipKey);
        MouseActive(InputManager.s_instance._mouseScrollAxis != 0);
    }

    private void Active(Image image, bool isActive)
    {
        Color dummy = Color.clear;
        dummy = image.color;
        if (isActive)
        {
            dummy.a = _activeAlpha;
        }
        else
        {
            dummy.a = 1f;
        }
        image.color = dummy;
    }

    private void MouseActive(bool isActive)
    {
        if (isActive)
        {
            _mouseTime = 0.5f;
            _mouseImage[0].enabled = true;
            _mouseImage[1].enabled = false;
        }
        else if (_mouseTime <= 0)
        {
            _mouseImage[0].enabled = false;
            _mouseImage[1].enabled = true;
        }

        if (_mouseTime >= 0)
        {
            _mouseTime -= Time.deltaTime;
        }
    }
}
