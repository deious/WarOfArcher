using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviourPun
{
    public GameObject _playerPrefab = null;
    private static GameObject _localPlayerCharacter = null;
    public GameObject _aiPrefab = null;
    public List<Transform> _spawnPoints = null;
    private int _respawnSeedIndex = -1;
    public int RespawnSeedIndex
    {
        get { return _respawnSeedIndex; }
        set { _respawnSeedIndex = value; }
    }
    public List<List<int>> _respawnSeeds = new List<List<int>>
    {
        new List<int>{ 3, 8, 5, 6, 7, 2, 4, 0 },
        new List<int>{ 5, 1, 2, 3, 4, 0, 7, 6 },
        new List<int>{ 7, 6, 5, 2, 0, 3, 4, 1 },
        new List<int>{ 1, 0, 8, 7, 3, 2, 6, 4 },
        new List<int>{ 4, 6, 2, 3, 7, 5, 0, 1 }
    };


    public void SpawnPlayer()
    {
        if (_localPlayerCharacter)
        {
            return;
        }

        if (ActNumber() < 0)
        {
            Debug.LogError("Act number를 찾지 못했습니다");
        }
        if (ActNumber() >= _respawnSeeds[0].Count)
        {
            Debug.LogError("Act number가 시드의 길이보다 큽니다");
        }
        Debug.Log($"플레이어 스폰 시드: {_respawnSeedIndex}");
        Transform spawnPos = _spawnPoints[_respawnSeeds[_respawnSeedIndex / _respawnSeeds[0].Count][(ActNumber() + _respawnSeedIndex) % _respawnSeeds[0].Count]];
        _localPlayerCharacter = PhotonNetwork.Instantiate(_playerPrefab.name, spawnPos.position, Quaternion.identity);
        int viewID = _localPlayerCharacter.GetPhotonView().ViewID;
        photonView.RPC("AnnounceCharacterViewID", RpcTarget.All, viewID);
    }

    private int ActNumber()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                return i;
            }
        }
        return -1;
    }

    public void SpawnAI(bool isMasterClient)
    {
        if (isMasterClient)
        {
            Transform spawnPos = _spawnPoints[_respawnSeeds[_respawnSeedIndex / _respawnSeeds[0].Count][(_respawnSeedIndex + _respawnSeeds[0].Count - 1) % _respawnSeeds[0].Count]];
            GameObject spawnedObject = PhotonNetwork.Instantiate(_aiPrefab.name, spawnPos.position, Quaternion.identity);
            int viewID = spawnedObject.GetPhotonView().ViewID;
            photonView.RPC("AnnounceAIViewID", RpcTarget.All, viewID);
        }
    }

    [PunRPC]
    private void AnnounceCharacterViewID(int viewID)
    {
        TurnManager.s_instance.AddToViewIDList(viewID);
    }
    [PunRPC]
    private void AnnounceAIViewID(int viewID)
    {
        TurnManager.s_instance.SetAIViewID(viewID);
    }
}