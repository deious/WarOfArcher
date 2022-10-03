using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class DevManager : MonoBehaviourPun
{
    [Header("NextTurn Button")]
    public Button _nextTurnButton = null;

    public void OnNextTurnButton()
    {
        photonView.RPC("MoveNextTurn", RpcTarget.All);
    }

    [PunRPC]
    public void MoveNextTurn()
    {
        //TurnManager.s_instance.EndTurn();
    }
}
