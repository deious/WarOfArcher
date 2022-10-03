using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniBoom : MonoBehaviour
{
    private bool _isEvent = true;

    public void SetMiniBoom(Transform point)
    {
        gameObject.SetActive(false);
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
        gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        gameObject.transform.Find("ExplosionArea").GetComponent<CircleCollider2D>().enabled = false;
        gameObject.transform.Find("ExplosionArea").gameObject.SetActive(false);
        transform.position = point.position;
        _isEvent = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isEvent)
        {
            return;
        }

        _isEvent = true;
        transform.Find("ExplosionArea").gameObject.SetActive(true);
    }
}
