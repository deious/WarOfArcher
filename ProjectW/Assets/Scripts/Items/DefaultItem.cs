using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DefaultItem : MonoBehaviourPun
{
    [SerializeField] private string _itemName;
    public string ItemName { get { return _itemName; } }
    [SerializeField] private Sprite _itemImage;
    public Sprite ItemImage { get { return _itemImage; } }
    [SerializeField] private int _remainCnt;
    public int RemainCnt { get { return _remainCnt; } }

    public string _itemInfo;

    public bool _isUsed = false;
    public bool _isAimType = false;
    public bool _isUtility = false;
}
