using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Resolution : MonoBehaviour
{
    private CanvasScaler _uiCanvasScaler = null;
    private int _weight = 1920, _height = 1080;
    
    private void Start()
    {
        _uiCanvasScaler = GetComponent<CanvasScaler>();
        _uiCanvasScaler.referenceResolution = new Vector2(_weight, _height); //json ���� ���� ����
        SetResolution();
    }

    public void SetResolution()
    {
        //Default �ػ� ����
        float fixedAspectRatio = 9f / 16f;

        //���� �ػ��� ����
        float currentAspectRatio = (float)Screen.width / (float)Screen.height;

        //���� �ػ� ���� ������ �� �� ���
        if (currentAspectRatio > fixedAspectRatio)
        {
            _uiCanvasScaler.matchWidthOrHeight = 0;
        }
        //���� �ػ��� ���� ������ �� �� ���
        else if (currentAspectRatio < fixedAspectRatio)
        {
            _uiCanvasScaler.matchWidthOrHeight = 1;
        }
    }
}