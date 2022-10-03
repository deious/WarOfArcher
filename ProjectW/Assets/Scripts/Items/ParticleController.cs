using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public GameObject _particleObject;
    [SerializeField] private float _boomWait = 0f;

    private void OnEnable()
    {
        Invoke(nameof(StartExplosion), _boomWait);
    }

    private void OnDisable()
    {
        EndExplosion();
    }
    public void StartExplosion()
    {
        _particleObject.SetActive(true);
        _particleObject.GetComponent<ParticleSystem>().Play();
    }

    public void EndExplosion()
    {
        _particleObject.GetComponent<ParticleSystem>().Stop();
        _particleObject.SetActive(false);
    }
}
