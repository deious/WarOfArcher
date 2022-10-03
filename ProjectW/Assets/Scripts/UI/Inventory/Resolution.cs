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
        _uiCanvasScaler.referenceResolution = new Vector2(_weight, _height); //json 으로 추후 수정
        SetResolution();
    }

    public void SetResolution()
    {
        //Default 해상도 비율
        float fixedAspectRatio = 9f / 16f;

        //현재 해상도의 비율
        float currentAspectRatio = (float)Screen.width / (float)Screen.height;

        //현재 해상도 가로 비율이 더 길 경우
        if (currentAspectRatio > fixedAspectRatio)
        {
            _uiCanvasScaler.matchWidthOrHeight = 0;
        }
        //현재 해상도의 세로 비율이 더 길 경우
        else if (currentAspectRatio < fixedAspectRatio)
        {
            _uiCanvasScaler.matchWidthOrHeight = 1;
        }
    }
}