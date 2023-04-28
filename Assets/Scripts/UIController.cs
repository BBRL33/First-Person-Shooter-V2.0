using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class UIController : MonoBehaviour
{
    public static UIController instance;
    private void Awake()
    {
        instance = this;
    }
    public Slider heatCap;
    public GameObject deathScreen;
    public Text IthinkUbrokeSth;
    public Slider health;
    public Text kills, deaths;
    public GameObject leaderBoard;
    public leaderboardScript leaderboardPlayerDisplay;
    public GameObject endscreen;
    public Text time;
    public Text timeTitleUnfunc;
    public GameObject pauseScreen;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            showOrQuitOptions();
        }
        if (Cursor.lockState != CursorLockMode.None && pauseScreen.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    public void showOrQuitOptions()
    {
        if (pauseScreen.activeInHierarchy)
        {
            pauseScreen.SetActive(false);
        }
        else
        {
            pauseScreen.SetActive(true);
        }
    }
    public void returnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }
    public void quit()
    {
        Application.Quit();
    }
}
