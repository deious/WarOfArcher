using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController s_instance = null;
    public GameObject[] _audio;

    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.Log("사운드 컨트룰러 생성 오류");
            Destroy(s_instance);
            return;
        }

        s_instance = this;

        _audio = new GameObject[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            _audio[i] = transform.GetChild(i).gameObject;
        }
    }
}
