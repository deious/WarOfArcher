using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterBoom : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] _clusterBoom = null;
    [SerializeField] private float _boomDuration = 2.0f;
    [SerializeField] private float _boomReturnWaitTime = 0.1f;
    private int _boomCnt = 0;
    private bool _isEvent = false;

    private void OnEnable()
    {
        _isEvent = false;
        InitBoom();
    }
    private void InitBoom()
    {
        _boomCnt = _clusterBoom.Length;
        
        for (int i = 0; i < _boomCnt; i++)
        {
            _clusterBoom[i].GetComponent<MiniBoom>().SetMiniBoom(gameObject.transform);
        }
    }
    // �ֻ�� ��������Ʈ ���ְ� ���� �ֵ� ������ �ð� ��������� �� ������ ��굵 �Ѱ���� ��
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isEvent)
        {
            _isEvent = true;
            StartCoroutine(BoomProgress());
        }
    }

    IEnumerator BoomProgress()
    {
        TurnOnBoom();

        yield return new WaitForSeconds(_boomDuration);
        ActiveBoom();

        yield return new WaitForSeconds(_boomReturnWaitTime);
        TurnOffBoom();
    }

    void TurnOnBoom()
    {
        for (int i = 0; i < _boomCnt; i++)
        {
            _clusterBoom[i].SetActive(true);

            if (!_clusterBoom[i].activeSelf)
            {
                Debug.LogError("�̴� �� SetActive Ȱ��ȭ ����");
            }
        }
    }
    void ActiveBoom()
    {
        for (int i = 0; i < _boomCnt; i++)
        {
            _clusterBoom[i].transform.Find("ExplosionArea").GetComponent<CircleCollider2D>().enabled = true;

            if (!_clusterBoom[i].activeSelf)
            {
                Debug.LogError("�̴� �� ��Ŭ �ݶ��̴� ����");
            }
        }
    }

    void TurnOffBoom()
    {
        for (int i = 0; i < _boomCnt; i++)
        {
            _clusterBoom[i].transform.Find("ExplosionArea").GetComponent<CircleCollider2D>().enabled = false;
            _clusterBoom[i].SetActive(false);

            if (_clusterBoom[i].activeSelf)
            {
                Debug.LogError("�̴� �� ���� ����");
            }
        }
    }
}
