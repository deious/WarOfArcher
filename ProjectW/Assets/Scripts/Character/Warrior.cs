using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Warrior : DefaultCharacter
{
    [SerializeField] private float _damageMitigation = 0.7f;

    protected override int CalcDamage(int damage)
    {
        return (int)(damage * _damageMitigation);
    }
}