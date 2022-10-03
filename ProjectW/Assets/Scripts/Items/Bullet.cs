using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : DefaultBullet
{
    [SerializeField] private float _boomDuration = 0.02f;

    public void SetBullet(Transform pos)
    {
        gameObject.SetActive(false);
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
        gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
        transform.position = pos.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameManager.s_instance.ChangeCameraMode(CameraController.CameraMode.LastMode);
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(BulletDuration());
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeadZone"))
        {
            GameManager.s_instance.ChangeCameraMode(CameraController.CameraMode.LastMode);
            gameObject.SetActive(false);
        }
    }


    IEnumerator BulletDuration()
    {
        yield return new WaitForSeconds(_boomDuration);
        //transform.Find("ExplosionArea").gameObject.SetActive(false);      // È¤½Ã ¸ô¶ó¼­ ³²°ÜµÓ´Ï´Ù
        gameObject.SetActive(false);
    }
}