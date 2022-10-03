using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using TMPro;

public class Slot : MonoBehaviourPun
{
    [SerializeField] private Image _itemSlotImage = null;
    public Image ItemSlotImage { get { return _itemSlotImage; } }
    [SerializeField] private Image _SlotBoardImage = null;
    public Image SlotBoardImage { get { return _SlotBoardImage; } }
    [SerializeField] private DefaultItem _currentSlotItem = null;
    public DefaultItem CurrentSlotItem { get { return _currentSlotItem; } }

    private QuickSlot _itemQuickSlot = null;
    private Color _nonSlotActiveColor = new Color(1, 1, 1, 1);
    private Color _slotActiveColor = new Color(1f, 0f, 0f, 1f);

    public DefaultItem Item
    {
        get { return _currentSlotItem; }
        set
        {
            _currentSlotItem = value;
            if (_currentSlotItem != null)
            {
                _itemSlotImage.sprite = _currentSlotItem.ItemImage;
                _itemSlotImage.color = new Color(1, 1, 1, 1);
            }
            else
            {
                _itemSlotImage.color = new Color(1, 1, 1, 0.5f);
            }
        }
    }

    private void Start()
    {
        _itemQuickSlot = transform.parent.parent.GetComponent<QuickSlot>();
    }

    public void ShowItemInfo()
    {
        if ((TurnManager.s_instance.IsAITurn || !TurnManager.s_instance.CurrentPlayer.IsLocal))
        {
            DefaultItem itemInfo = _currentSlotItem.GetComponent<DefaultItem>();
            _itemQuickSlot.SetItemInfo(itemInfo.ItemImage, itemInfo._itemInfo);
        }
    }

    public void ItemClick()
    {
        if ((!TurnManager.s_instance.IsAITurn && TurnManager.s_instance.CurrentPlayer.IsLocal))
        {
            photonView.RPC("SwitchItem", RpcTarget.All);
            GameManager.s_instance.ChangeCameraMode(CameraController.CameraMode.PlayerMode);
        }
    }

    public void ChangeSelect()
    {
        if (_SlotBoardImage.color == _slotActiveColor)
        {
            _SlotBoardImage.color = _nonSlotActiveColor;
            _itemSlotImage.color = _nonSlotActiveColor;
            return;
        }

        Slot[] slots = transform.parent.GetComponentsInChildren<Slot>();
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i]._SlotBoardImage.color = _nonSlotActiveColor;
            slots[i]._itemSlotImage.color = _nonSlotActiveColor;
        }
        _SlotBoardImage.color = _slotActiveColor;
        _itemSlotImage.color = _nonSlotActiveColor / 2;
    }
    [PunRPC]
    private void SwitchItem()
    {
        DefaultCharacter avatarClass = TurnManager.s_instance.CurrentCharacter.GetComponent<DefaultCharacter>();
        if (!GameManager.s_instance.GetPlayerControllerState(DefaultCharacter.PlayerState.IsAttack) && _currentSlotItem && !TurnManager.s_instance.IsAITurn)
        {
            if (avatarClass._equipItem && avatarClass._equipItem == _currentSlotItem)
            {
                return;
            }

            avatarClass.SwitchItem(_currentSlotItem.name);
            if (_currentSlotItem._isUtility)
            {
                UtilityItem utility = _currentSlotItem.GetComponent<UtilityItem>();
                _itemQuickSlot.SetItemInfo(utility.ItemImage, utility._itemInfo);
            }
            else
            {
                WeaponItem weapon = _currentSlotItem.GetComponent<WeaponItem>();
                _itemQuickSlot.SetItemInfo(weapon.ItemImage, weapon._rangeLevel, weapon._damageLevel, weapon._explosionLevel);
            }
            ChangeSelect();
        }
        EventSystem.current.SetSelectedGameObject(null);
    }
}