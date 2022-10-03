using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class LobbyChatSystem : MonoBehaviourPun
{
    [Header("Chat System")]
    public TMP_InputField _playerInput = null;
    public List<TextMeshProUGUI> _chatTextList = null;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            _playerInput.ActivateInputField();
        }
    }

    public void ClearChatText()
    {
        for (int i = 0; i < _chatTextList.Count; i++)
        {
            _chatTextList[i].text = "";
        }
    }

    public void OnEndEditEvent()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendInputText();
        }
    }

    private void SendInputText()
    {
        if (_playerInput.text == "")
        {
            return;
        }
        string msg = PhotonNetwork.LocalPlayer.NickName + " : " + _playerInput.text;
        photonView.RPC("UpdateChatText", RpcTarget.All, msg);
        _playerInput.text = "";
    }

    public void SendSystemMessage(string msg)
    {
        string message = "<color=red>" + "[System] " + msg + "</color>";
        photonView.RPC("UpdateChatText", RpcTarget.All, message);
    }

    [PunRPC]
    private void UpdateChatText(string msg)
    {
        for (int i = _chatTextList.Count - 1; i > 0; i--)
        {
            _chatTextList[i].text = _chatTextList[i - 1].text;
        }
        _chatTextList[0].text = msg;
    }
}
