using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class playerSpawner : MonoBehaviour
{
    public static playerSpawner instance;
    private void Awake()
    {
        instance = this;
    }
    public GameObject playerPrefab;
    private GameObject player;
    public GameObject bood;
    public float toot = 5;
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            spawnPlayer();
        }
    }

    public void spawnPlayer()
    {
        Transform spawnpoint = SpawnManager.instance.GetSpawnPoints();
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnpoint.position, spawnpoint.rotation);
    }
    public void die(string damager)
    {
        UIController.instance.IthinkUbrokeSth.text = "I think " + damager + " added a hole somewhere on you.";
        matchManager.instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber,1,1);
        if (player != null)
        {
            StartCoroutine(dei());
        }
    }
    public IEnumerator dei()
    {
        PhotonNetwork.Instantiate(bood.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        player = null;
        UIController.instance.deathScreen.SetActive(true);
        yield return new WaitForSeconds(toot);
        UIController.instance.deathScreen.SetActive(false);
        if (matchManager.instance.state == matchManager.GameSate.Playing && player == null)
        {
            spawnPlayer();
        }
        
    }
}
