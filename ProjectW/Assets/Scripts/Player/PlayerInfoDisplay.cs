using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerInfoDisplay : MonoBehaviour
{
    [SerializeField] private PhotonView _playerPV = null;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TMP_Text _playerName = null;
    [SerializeField] private GameObject _accumulateDamageViewer = null;
    [SerializeField] private DefaultCharacter _defaultCharacter = null;
    [SerializeField] private DefaultCharacterAI _defaultCharacterAI = null;

    //private int _accumulateDamage = 0;   // 추후 누적 데미지 저장용
    private float _displayTime = 2.0f;

    public void Init()
    {
        if (GetComponent<DefaultCharacterAI>())
        {
            _defaultCharacterAI = GetComponent<DefaultCharacterAI>();
            _playerName.text = "<color=blue>Alpha</color>";
        }
        else
        {
            _defaultCharacter = GetComponent<DefaultCharacter>();
            _playerName.text = _playerPV.Owner.NickName;

        }
    }
    
    public void ShowPlayerInfoUI()
    {
        if (GetComponent<DefaultCharacterAI>())
        {
            SetHealth(_defaultCharacterAI.InfoData._hp);
            StartCoroutine(ShowDamageUI(_displayTime));
            _accumulateDamageViewer.GetComponent<TMP_Text>().text = "-" + _defaultCharacterAI._accumulateDamage.ToString();
        }
        else
        {
            SetHealth(_defaultCharacter.InfoData._hp);
            StartCoroutine(ShowDamageUI(_displayTime));
            _accumulateDamageViewer.GetComponent<TMP_Text>().text = "-" + _defaultCharacter._accumulateDamage.ToString();
        }
    }

    IEnumerator ShowDamageUI(float time)
    {
        _accumulateDamageViewer.SetActive(true);
        yield return new WaitForSeconds(time);
        _accumulateDamageViewer.SetActive(false);
    }

    public void SetHealth(int health)
    {
        _healthSlider.value = health;
    }
}