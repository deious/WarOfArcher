using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class ShowWindUI : MonoBehaviourPun
{
    [SerializeField] private Transform _windRotaion = null;
    [SerializeField] private Image[] _windImageList = null;

    public int windImageCnt { get { return _windImageList.Length; } }
    public static ShowWindUI s_insatnce;

    private void Awake()
    {
        if (s_insatnce)
        {
            Debug.LogError("ShowWindUI 생성 에러");
            return;
        }
        s_insatnce = this;
    }

    [PunRPC]
    public void UpdateWindUI(int windLevel)
    {
        for (int i = 0; i < _windImageList.Length; i++)
        {
            _windImageList[i].enabled = false;
        }

        if (windLevel == 0)
        {
            return;
        }
        else
        {
            for (int i = 0; i < Mathf.Abs(windLevel); i++)
            {
                _windImageList[i].enabled = true;
            }
            _windRotaion.localScale = windLevel > 0 ? new Vector3(-0.4f, 0.4f, 0.4f) : new Vector3(0.4f, 0.4f, 0.4f);
        }
    }
}
