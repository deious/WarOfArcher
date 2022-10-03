using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    [SerializeField] private GameObject _damageViewer;

    [SerializeField] private Transform _target;
    [SerializeField] private const float _offsetScale = 0.14f;
    [SerializeField] private float _staticOffset = 3.0f;
    private float _dynamicOffset= 0f;

    private Camera _mainCam;
    // Start is called before the first frame update
    void Start()
    {
        _mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        _dynamicOffset = _mainCam.orthographicSize * _offsetScale;
        Vector3 pos = _mainCam.WorldToScreenPoint(_target.position + new Vector3(0, _staticOffset + _dynamicOffset, 0));

        if (transform.position != pos)
        {
            transform.position = pos;
        }
    }
}
