using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ItemStorage : MonoBehaviourPun
{
    public static ItemStorage s_instance = null;
    [SerializeField] private Transform _bulletStorage = null;
    [SerializeField] private Transform _weaponStorage = null;
    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError("아이템 매니저 생성 에러");
            Destroy(this.gameObject);
            return;
        }
        s_instance = this;
    }
    public Transform WeaponStorage
    {
        get
        {
            return _weaponStorage;
        }
    }
    public Transform BulletStorage
    {
        get
        {
            return _bulletStorage;
        }
    }

    public void PreProcess(bool isMasterClient)
    {
        if (isMasterClient)
        {
            TransferWeaponOwnership();
        }

        JetPackInit();
    }

    private void JetPackInit()
    {
        foreach (Transform weapon in _weaponStorage)
        {
            if (weapon.GetComponent<JetPack>())
            {
                weapon.GetComponent<JetPack>().FillFuel();
                break;
            }
        }
    }

    public void PostProcess()
    {
        foreach (Transform bullet in BulletStorage)
        {
            bullet.position = ItemStorage.s_instance.transform.position;
        }
    }

    private void TransferWeaponOwnership()
    {
        foreach (Transform weapon in _weaponStorage)
        {
            weapon.gameObject.GetPhotonView().TransferOwnership(TurnManager.s_instance.CurrentPlayer);
        }
    }
}