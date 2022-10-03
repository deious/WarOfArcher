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
        gameObject.GetComponent<Rigidbody2D>().isKinematic = true;  // ���� �� �ٸ� �������� �̵� ����

        //StartCoroutine(StopExplosion());
        Invoke(nameof(StopExplosion), _boomDuration);

        if (!_explosionArea.activeSelf)
        {
            Debug.LogError("���� ��ź ������ ����");
        }
    }

    void StopExplosion()
    {
        _explosionArea.SetActive(false);
    }
}
