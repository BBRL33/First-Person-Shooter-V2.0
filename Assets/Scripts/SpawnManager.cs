using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;
    private void Awake()
    {
        instance = this;
    }
    public Transform[] spawnpoints;
    void Start()
    {
        foreach(Transform spawn in spawnpoints)
        {
            spawn.gameObject.SetActive(false);
        }    
    }
    public Transform GetSpawnPoints()
    {
        return spawnpoints[Random.Range(0, spawnpoints.Length)];
    }
}
