using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private static Transform s_respawnStartPoint = null, s_respawnEndPoint = null;
    [SerializeField] private static Transform s_respawnAIPoints;
    public static Transform RespawnStartPoint { get { return s_respawnStartPoint; } }
    public static Transform RespawnEndPoint { get { return s_respawnEndPoint; } }
    public static Transform RespawnAIPoints { get { return s_respawnAIPoints; } }

    private void Start()
    {
        s_respawnStartPoint = transform.Find("StartPoint");
        s_respawnEndPoint = transform.Find("EndPoint");
        s_respawnAIPoints = transform.Find("AIRespawnPoints");
    }
}