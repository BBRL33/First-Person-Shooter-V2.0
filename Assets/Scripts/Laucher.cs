using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class Laucher : MonoBehaviourPunCallbacks
{
    public static Laucher instance;
    private void Awake()
    {
        instance = this;
    }
    public Text loading;
    public Text connecting;
    private float timer;
    public GameObject menubutton;
    public GameObject loadingScreen;
    public GameObject createRoomScreen;
    public InputField roomNameInput;
    private bool creatingRoom;
    private bool leavingRoom;
    public GameObject lobbyScreen;
    public Text lobbyName;
    public GameObject errScreen;
    public Text errText;
    public GameObject roomBrowserScreen;
    public RoomButton roomButton;
    private List<RoomButton> roomButtonList = new List<RoomButton>();
    public Text playerName;
    private List<Text> nickonames = new List<Text>();
    public GameObject nameInputScreen;
    public Text nameInput;
    public static bool hasSetNick;
    public string[] levelToLoad;
    public GameObject startButton;
    public bool changeMapBetweenRounds = true;
    void Start()
    {
        CloseMenus();
        loadingScreen.SetActive(true);
        connecting.gameObject.SetActive(true);
        PhotonNetwork.ConnectUsingSettings();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void CloseMenus()
    {
        errScreen.SetActive(false);
        menubutton.SetActive(false);
        lobbyScreen.SetActive(false);
        loadingScreen.SetActive(false);
        nameInputScreen.SetActive(false);
        loading.gameObject.SetActive(false);
        connecting.gameObject.SetActive(false);
        createRoomScreen.gameObject.SetActive(false);
        roomBrowserScreen.gameObject.SetActive(false);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errText.text = "Err 404 type: "+message;
        CloseMenus();
        errScreen.SetActive(true);
    }
    public void LeaveRoom()
    {
        CloseMenus();
        creatingRoom = false;
        leavingRoom = true;
        loadingScreen.SetActive(true);
        loading.gameObject.SetActive(true);
        StartCoroutine(waitForSecLeaveRoom());
    }
    public override void OnLeftRoom()
    {
        CloseMenus();
        menubutton.SetActive(true);
    }
    public void CloseErrScreen()
    {
        CloseMenus();
        menubutton.SetActive(true);
    }
    public void OpenRoomCreate()
    {
        CloseMenus();
        createRoomScreen.gameObject.SetActive(true);
    }
    public void ActivateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameInput.text))
        {
            RoomOptions opts = new RoomOptions();
            opts.MaxPlayers = 10;
            PhotonNetwork.CreateRoom(roomNameInput.text, opts);
            CloseMenus();
            leavingRoom = false;
            creatingRoom = true;
            loading.gameObject.SetActive(true);
            loadingScreen.SetActive(true);
            StartCoroutine(waitForSec(3));
        }
    }
    public override void OnJoinedRoom()
    {
        CloseMenus();
        lobbyScreen.SetActive(true);
        lobbyName.text = "Room ID: " + PhotonNetwork.CurrentRoom.Name;
        listAllPlayers();
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }
    private void listAllPlayers()
    {
        foreach (Text boof in nickonames)
        {
            Destroy(boof.gameObject);
        }
        nickonames.Clear();
        Player[] playerListomato = PhotonNetwork.PlayerList;
        for (int boofa = 0; boofa < playerListomato.Length; boofa++)
        {
            Text newPlayerName = Instantiate(playerName, playerName.transform.parent);
            newPlayerName.text = playerListomato[boofa].NickName;
            newPlayerName.gameObject.SetActive(true);
            nickonames.Add(newPlayerName);
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Text newPlayerName = Instantiate(playerName, playerName.transform.parent);
        newPlayerName.text = newPlayer.NickName;
        newPlayerName.gameObject.SetActive(true);
        nickonames.Add(newPlayerName);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        listAllPlayers();
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        loading.gameObject.SetActive(true);
        connecting.gameObject.SetActive(false);
        StartCoroutine(waitForSecJoinLobby());
    }
    public override void OnJoinedLobby()
    {
        CloseMenus();
        menubutton.SetActive(true);
        if (!hasSetNick)
        {
            CloseMenus();
            nameInputScreen.SetActive(true);
            if(PlayerPrefs.HasKey("playerName"))
            {
                nameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer <= 0.25f)
        {
            if (creatingRoom == true)
            {
                loading.text = "Creating Room";
            }
            else if (leavingRoom == true)
            {
                loading.text = "Leaving Room";
            }
            else
            {
                loading.text = "Loading";
            }
            connecting.text = "Connecting";
        }
        else if (timer <= 0.5f)
        {
            if (creatingRoom == true)
            {
                loading.text = "Creating Room.";
            }
            else if (leavingRoom == true)
            {
                loading.text = "Leaving Room.";
            }
            else
            {
                loading.text = "Loading.";
            }
            connecting.text = "Connecting.";
        }
        else if (timer <= 0.75f)
        {
            if (creatingRoom == true)
            {
                loading.text = "Creating Room..";
            }
            else if (leavingRoom == true)
            {
                loading.text = "Leaving Room..";
            }
            else
            {
                loading.text = "Loading..";
            }
            connecting.text = "Connecting..";
        }
        else if (timer <= 1)
        {
            if (creatingRoom == true)
            {
                loading.text = "Creating Room...";
            }
            else if (leavingRoom == true)
            {
                loading.text = "Leaving Room...";
            }
            else
            {
                loading.text = "Loading...";
            }
            connecting.text = "Connecting...";
        }
        else if (timer > 1)
        {
            timer = 0;
        }
    }
    public void openRoomBrowser()
    {
        CloseMenus();
        roomBrowserScreen.gameObject.SetActive(true);
    }
    public void closeRoomBrowser()
    {
        CloseMenus();
        menubutton.SetActive(true);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton rb in roomButtonList)
        {
            Destroy(rb.gameObject);
        }

        roomButtonList.Clear();
        roomButton.gameObject.SetActive(false);
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(roomButton, roomButton.transform.parent);
                newButton.setButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);
                roomButtonList.Add(newButton);
            }
        }
    }
    public void joinRoom(RoomInfo roominfoo)
    {
        PhotonNetwork.JoinRoom(roominfoo.Name);
        CloseMenus();
        creatingRoom = false;
        leavingRoom = false;
        loadingScreen.SetActive(true);
        loading.gameObject.SetActive(true);
    }
    public void setNickname()
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text;
            PlayerPrefs.SetString("playerName",nameInput.text);
            CloseMenus();
            menubutton.SetActive(true);
            hasSetNick = true;
        }
    }
    public void startGame()
    {
        PhotonNetwork.LoadLevel(levelToLoad[Random.Range(0,levelToLoad.Length)]);
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }
    public void quitGame()
    {
        Application.Quit();
    }
    IEnumerator waitForSecJoinLobby()
    {
        yield return new WaitForSeconds(3);
        PhotonNetwork.JoinLobby();
    }
    IEnumerator waitForSec(float time)
    {
        yield return new WaitForSeconds(time);
    }
    IEnumerator waitForSecLeaveRoom()
    {
        yield return new WaitForSeconds(3);
        PhotonNetwork.LeaveRoom();
    }
}
