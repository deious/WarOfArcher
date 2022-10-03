using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DefaultBullet : MonoBehaviourPun
{
    [SerializeField] private Rigidbody2D _bulletRigidBody = null;
    private void Awake()
    {
        _bulletRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _bulletRigidBody.AddForce(Vector2.right * MapManager.s_instance.CurrWindForce * Time.deltaTime);
    }
}
