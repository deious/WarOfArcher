using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReactiveButton : MonoBehaviour
{
    [SerializeField] private const float c_buttonUpScale = 1.2f;
    public Color _textColorOffset;
    public int _textFontSizeOffset = 20;

    private Color _textDefalutColor;
    public TextMeshProUGUI _textInButton;
    private float _textFontSize;
    private void Start()
    {
        _textFontSize = _textInButton.fontSize;
        _textDefalutColor = _textInButton.color;
    }
    public void OnMouseEnter()
    {
        transform.localScale = new Vector3(c_buttonUpScale, c_buttonUpScale, c_buttonUpScale);
        //_textInButton.localScale = new Vector3(_buttonUpScale, _buttonUpScale, _buttonUpScale);
        _textInButton.GetComponent<TextMeshProUGUI>().fontSize = _textFontSize + _textFontSizeOffset;
        _textInButton.GetComponent<TextMeshProUGUI>().color = _textColorOffset;
    }
    public void OnMouseExit()
    {
        transform.localScale = new Vector3(1, 1, 1);
        //_textInButton.localScale = new Vector3(1, 1, 1);
        _textInButton.GetComponent<TextMeshProUGUI>().fontSize = _textFontSize;
        _textInButton.GetComponent<TextMeshProUGUI>().color = _textDefalutColor;
    }
}
