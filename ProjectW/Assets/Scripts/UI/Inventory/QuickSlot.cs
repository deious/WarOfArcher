using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class QuickSlot : MonoBehaviour
{
    [SerializeField] private List<DefaultItem> _itemList = null;
    [SerializeField] private Transform _slotParent = null;
    [SerializeField] private Slot[] _slots = null;
    [SerializeField] private int _levelMax = 3;

    public GameObject _weaponItemInfo = null;
    public GameObject _itemInfoText = null;
    public Image _itemImage = null;
    public List<Image> _rangeList = null;
    public List<Image> _damageList = null;
    public List<Image> _ExplosionList = null;

    public Slot[] Slots { get { return _slots; } }
    public void Init()
    {
        _itemList = GetItemList();
        InitSlot();
        _itemImage.enabled = false;
        _itemInfoText.SetActive(false);
        _weaponItemInfo.SetActive(false);

    }

    private void OnValidate()
    {
        _slots = _slotParent.GetComponentsInChildren<Slot>();
    }

    public void InitSlot()
    {
        int i = 0;
        for (; ((i < _itemList.Count) && i < (_slots.Length)); i++)
        {
            _slots[i].Item = _itemList[i];
        }
        for (; i < _slots.Length; i++)
        {
            _slots[i].Item = null;
        }
    }
    public void RefreshSelection()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i].ItemSlotImage.color = new Color(1, 1, 1, 1);
            _slots[i].SlotBoardImage.color = new Color(1, 1, 1, 1);
        }
        ClearInfo();
    }
    public void SetItemInfo(Sprite slotItemImage, string itemInfo)
    {
        _itemImage.enabled = true;
        _itemInfoText.SetActive(true);
        _weaponItemInfo.SetActive(false);

        _itemImage.sprite = slotItemImage;
        _itemInfoText.GetComponentInChildren<TextMeshProUGUI>().text = itemInfo;

    }
    public void SetItemInfo(Sprite slotItemImage, int rangeLevel, int damageLevel, int explosionLevel)
    {
        _itemImage.enabled = true;
        _itemInfoText.SetActive(false);
        _weaponItemInfo.SetActive(true);

        InitItemLevel();
        _itemImage.sprite = slotItemImage;

        if (rangeLevel > _levelMax || damageLevel > _levelMax || explosionLevel > _levelMax)
        {
            Debug.Log("Level로 들어온 인자 중에 Level 한계를 넘은 인자가 있습니다");
            return;
        }
        int sumLevel = rangeLevel + damageLevel + explosionLevel;
        int levelCount = 0, levelCount2 = 0, levelCount3 = 0;
        while (sumLevel > 0)
        {
            if (rangeLevel > 0)
            {
                --rangeLevel;
                _rangeList[levelCount].color = new Vector4(1, 0, 0, 1);
                levelCount++;
                sumLevel--;
            }
            if (damageLevel > 0)
            {
                --damageLevel;
                _damageList[levelCount2].color = new Vector4(1, 0, 0, 1);
                levelCount2++;
                sumLevel--;
            }
            if (explosionLevel > 0)
            {
                --explosionLevel;
                _ExplosionList[levelCount3].color = new Vector4(1, 0, 0, 1);
                levelCount3++;
                sumLevel--;
            }

        }
    }
    public void ClearInfo()
    {
        if ((TurnManager.s_instance.IsAITurn || !TurnManager.s_instance.CurrentPlayer.IsLocal))
        {
            _weaponItemInfo.SetActive(false);
            _itemInfoText.SetActive(true);
            _itemImage.enabled = false;
            _itemInfoText.GetComponentInChildren<TextMeshProUGUI>().text =
                "";
        }

    }
    private void InitItemLevel()
    {
        int i = 0;
        while (i < _levelMax)
        {
            _rangeList[i].color = new Vector4(1, 1, 1, 1);
            _damageList[i].color = new Vector4(1, 1, 1, 1);
            _ExplosionList[i].color = new Vector4(1, 1, 1, 1);
            i++;
        }
    }


    private List<DefaultItem> GetItemList()
    {
        List<DefaultItem> items = new List<DefaultItem>();
        for (int i = 0; i < ItemStorage.s_instance.WeaponStorage.childCount; i++)
        {
            items.Add(ItemStorage.s_instance.WeaponStorage.GetChild(i).GetComponent<DefaultItem>());
        }
        return items;
    }

}
