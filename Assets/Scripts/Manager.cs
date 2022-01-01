using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Manager : MonoBehaviourPunCallbacks
{
    public string Player_prefab;
    public Transform[] SpawnPoint;
    

    private void Start()
    {
        Spawn();
    }
    
    public void Spawn()
    {
        Transform t_spawn = SpawnPoint[Random.Range(0, SpawnPoint.Length)];
        PhotonNetwork.Instantiate(Player_prefab, t_spawn.position, t_spawn.rotation);
    }
}
