using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float _radius = 0f;
    [SerializeField] private int _damage = 0;
    [SerializeField] private int _index = 0;

    private void Start()
    {
        _radius = gameObject.GetComponent<CircleCollider2D>().radius;
    }

    private void OnEnable()
    {
        //SoundController.s_instance.transform.Find("SmallExplosionSound").GetComponent<AudioSource>().Play();
        SoundController.s_instance._audio[_index].GetComponent<AudioSource>().Play();
        RaycastHit2D[] rayHits = Physics2D.CircleCastAll(transform.position, _radius, Vector2.zero, LayerMask.GetMask("Player"));

        int tempDamage = 0;

        foreach (RaycastHit2D player in rayHits)
        {
            float distance = Vector2.Distance(player.transform.position, transform.position);
            if (player.transform.CompareTag("Player") && distance < _radius)
            {
                if (TurnManager.s_instance.CurrentPlayer.IsLocal)
                {
                    tempDamage = (int)(_damage * (1 - (distance / _radius)));
                    if (tempDamage == 0)
                    {
                        tempDamage = 1;
                    }

                    if (player.transform.gameObject.GetComponent<DefaultCharacterAI>())
                    {
                        tempDamage = (int)(_damage * (1 - (distance / _radius)));
                        player.transform.root.GetComponent<DefaultCharacterAI>().CallHit(tempDamage);
                    }
                    else
                    {
                        player.transform.root.GetComponent<DefaultCharacter>().CallHit(tempDamage);
                    }
                }
            }
        }
    }
}