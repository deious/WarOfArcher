using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boom : MonoBehaviour
{
    public GameObject _explosionArea = null;
    private bool _isEvent = false;

    [SerializeField] float _boomDuration = 0.1f;

    private void OnEnable()
    {
        _isEvent = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isEvent)
        {
            return;
        }
        _explosionArea.SetActive(true);
        _isEvent = true;
        //SoundController.s_instance.transform.Find("LargeExplosionSound").GetComponent<AudioSource>().Play();
        SoundController.s_instance._audio[1].GetComponent<AudioSource>().Play();
        gameObject.GetComponent<Rigidbody2D>().isKinematic = true;  // 폭발 후 다른 곳으로의 이동 방지

        //StartCoroutine(StopExplosion());
        Invoke(nameof(StopExplosion), _boomDuration);

        if (!_explosionArea.activeSelf)
        {
            Debug.LogError("메인 폭탄 비정상 종료");
        }
    }

    void StopExplosion()
    {
        _explosionArea.SetActive(false);
    }
}
