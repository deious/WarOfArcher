using UnityEngine;
using System.Collections;

public class RopeShooter : UtilityItem
{
    [SerializeField] private Rope _rope = null;
    [SerializeField] private bool _isThrowOut = false;
    [SerializeField] private Data _data = null;
    private const float c_waitTime = 0.1f;

    [System.Serializable]
    private class Data
    {
        public float _speed = 0f;
    }

    private void Start()
    {
        Init();
    }

    public override void UseUtility(Vector3 aimDir)
    {
        _isUsed = true;

        _rope.GetComponent<Rope>()._owner = transform.root.gameObject;
        _rope._ownerRotationCenter = transform.root.gameObject.GetComponent<DefaultCharacter>()._aimRotation.gameObject;
        _rope.transform.position = _rope._ownerRotationCenter.transform.position;
        _rope.gameObject.SetActive(true);
        _rope.GetComponent<Rigidbody2D>().velocity = aimDir * _data._speed;

        StartCoroutine(Throw());
    }
    public override void CancelUtility()
    {
        StopAllCoroutines();

        _isUsed = false;
        _rope.gameObject.SetActive(false);
        transform.root.GetComponent<Rigidbody2D>().gravityScale = 1;
    }

    private void Init()
    {
        bool isFind = false;
        foreach (Transform bullet in ItemStorage.s_instance.BulletStorage)
        {
            if (bullet.name.Contains("Hook"))
            {
                _rope = bullet.gameObject.GetComponent<Rope>();
                isFind = true;
                break;
            }
        }
        if (!isFind)
        {
            Debug.LogError($"{nameof(ItemStorage.s_instance.BulletStorage)} 하위에 후크 오브젝트가 없습니다");
        }
    }

    IEnumerator Throw()
    {
        _isThrowOut = true;
        for (int i = 0; i < 50 && _rope.CanThrow; i++)
        {
            if (_rope._isHooked)
            {
                _isThrowOut = false;
                break;
            }
            yield return new WaitForSeconds(c_waitTime);
        }
        if (_isThrowOut)
        {
            transform.root.gameObject.GetComponent<DefaultCharacter>().CallCancelItem();
        }
    }
}