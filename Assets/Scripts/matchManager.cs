using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Random = UnityEngine.Random;

public class matchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{

    public static matchManager instance;

    private void Awake()
    {
        instance = this;
    }

    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat,
        NextMatch,
        SyncTimer
    }
    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;
    private List<leaderboardScript> allPlayersLeaderBoofa = new List<leaderboardScript>();

    public enum GameSate
    {
        Waiting,
        Playing,
        Ending
    }

    public float matchLength = 120;
    private float currentMatchTimer;
    public int KillsToWin = 3;
    public Transform mapCamPoint;
    public GameSate state = GameSate.Waiting;
    public float waitAfterEnding = 5f;
    public bool perpetual;
    private float timer;
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
            state = GameSate.Playing;
            setUpTimer();
        }
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.Tab) && state != GameSate.Ending)
        {
            showLeaderBoard();
        }
        else if (state == GameSate.Ending)
        {
            showLeaderBoard();
        }
        else
        {
            UIController.instance.leaderBoard.SetActive(false);
        }
        if (currentMatchTimer > 0 && state == GameSate.Playing)
        {
            currentMatchTimer -= Time.deltaTime;
            if(currentMatchTimer <= 0)
            {
                currentMatchTimer = 0;
                state = GameSate.Ending;
                if (PhotonNetwork.IsMasterClient)
                {
                    ListPlayerSend();
                    StateCheck();
                }
            }
            updateTimer();
        }
    }
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;
            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerRecieve(data);
                    break;
                
                case EventCodes.ListPlayers:
                    ListPlayerRecieve(data);
                    break;
                
                case EventCodes.UpdateStat:
                    UpdateStatsRecieve(data);
                    break;
                
                case EventCodes.NextMatch:
                    NextMatchReceive();
                    break;
                    
                    
            }
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NewPlayerSend(string username)
    {
        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent((byte)EventCodes.NewPlayer, package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, new SendOptions { Reliability = true });
    }

    public void NewPlayerRecieve(object[] dataRecieved)
    {
        PlayerInfo player = new PlayerInfo((string)dataRecieved[0], (int)dataRecieved[1], (int)dataRecieved[2], (int)dataRecieved[3]);
        allPlayers.Add(player);
        ListPlayerSend();
    }

    public void ListPlayerSend()
    {
        object[] package = new object[allPlayers.Count + 1];
        package[0] = state;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];
            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i + 1] = piece;

        }
        
        PhotonNetwork.RaiseEvent((byte)EventCodes.ListPlayers, package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
    }

    public void ListPlayerRecieve(object[] dataRecieved)
    {
        allPlayers.Clear();
        state = (GameSate)dataRecieved[0];
        for (int i = 1; i < dataRecieved.Length; i++)
        {
            object[] piece = (object[])dataRecieved[i];
            PlayerInfo player = new PlayerInfo((string)piece[0], (int)piece[1], (int)piece[2], (int)piece[3]);
            allPlayers.Add(player);
            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i - 1;
            }
        }
        
        StateCheck();
     
    }
    public void UpdateStatsSend(int actorSending, int statToUpdate, int amountToChange)
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };
        PhotonNetwork.RaiseEvent((byte)EventCodes.UpdateStat, package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
        
    }

    public void UpdateStatsRecieve(object[] dataRecieved)
    {
        int actor = (int)dataRecieved[0];
        int statType = (int)dataRecieved[1];
        int amount = (int)dataRecieved[2];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].actor == actor)
            {
                switch (statType)
                {
                    case 0:
                        allPlayers[i].kills += amount;
                        break;
                    case 1:
                        allPlayers[i].deaths += amount;
                        
                        break;
                }

                if (i == index)
                {
                    UpdatesStatsDisplay();
                }

                if (UIController.instance.leaderBoard.activeInHierarchy)
                {
                    showLeaderBoard();
                }

                break;
            }

        }
        ScoreCheck();
    }
    public void UpdatesStatsDisplay()
    {
        if (allPlayers.Count > index)
        {
            UIController.instance.kills.text = "Kills: " + allPlayers[index].kills;
            UIController.instance.deaths.text = "Deaths: " + allPlayers[index].deaths;
        }
        else
        {
            UIController.instance.kills.text = "Kills: 0";
            UIController.instance.deaths.text = "Deaths: 0";
        }
    }
    public void showLeaderBoard()
    {
        UIController.instance.leaderBoard.SetActive(true);
        foreach(leaderboardScript lb in allPlayersLeaderBoofa)
        {
            Destroy(lb.gameObject);
        }
        allPlayersLeaderBoofa.Clear();
        UIController.instance.leaderboardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = sortPlayers(allPlayers);
        foreach (PlayerInfo player in sorted)
        {
            leaderboardScript newPlayerDispay = Instantiate(UIController.instance.leaderboardPlayerDisplay, UIController.instance.leaderboardPlayerDisplay.transform.parent);
            newPlayerDispay.setDetails(player.name,player.kills,player.deaths);
            newPlayerDispay.gameObject.SetActive(true);
            allPlayersLeaderBoofa.Add(newPlayerDispay);
        }
    }

    private List<PlayerInfo> sortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        while (sorted.Count < players.Count)
        {
            int highest = -1;
            PlayerInfo selection = players[0];

            foreach (PlayerInfo player in players)
            {
                if (!sorted.Contains(player))
                {
                    if (player.kills > highest)
                                    {
                                        selection = player;
                                        highest = player.kills;
                                    }
                }
                
            }
            
            sorted.Add(selection);
            
            
        }
        
        return sorted;
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(0);
    }

    void ScoreCheck()
    {
        bool winnerFound = false;
        foreach (PlayerInfo player in allPlayers)
        {
            if (player.kills >= KillsToWin && KillsToWin > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if (winnerFound)
        {
            if (PhotonNetwork.IsMasterClient && state != GameSate.Ending)
            {
                state = GameSate.Ending;
                ListPlayerSend();
            }
        }
    }

    void StateCheck()
    {
        if (state == GameSate.Ending)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        state = GameSate.Ending;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }
        
        UIController.instance.endscreen.SetActive(true);
        showLeaderBoard();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Camera.main.transform.position = mapCamPoint.position;
        Camera.main.transform.rotation = mapCamPoint.rotation;
        StartCoroutine(EndCo());

    }

    private IEnumerator EndCo()
    {
        yield return new WaitForSeconds(waitAfterEnding);
        if (!perpetual)
        {
           PhotonNetwork.AutomaticallySyncScene = false;
                   PhotonNetwork.LeaveRoom(); 
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if(!Laucher.instance.changeMapBetweenRounds)
                {
                    NextMatchSend();
                }
                else
                {
                    int newLevel = Random.Range(0, Laucher.instance.levelToLoad.Length);
                    if(Laucher.instance.levelToLoad[newLevel] == SceneManager.GetActiveScene().name)
                    {
                        NextMatchSend();
                    }
                    else
                    {
                        PhotonNetwork.LoadLevel(Laucher.instance.levelToLoad[newLevel]);
                    }
                }
            }
        }
        
    }

    public void NextMatchSend()
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NextMatch,
            null,
            new RaiseEventOptions{Receivers = ReceiverGroup.All},
            new SendOptions{Reliability = true}
        );

    }
    
    public void NextMatchReceive()
    {
        state = GameSate.Playing;
        UIController.instance.endscreen.SetActive(false);
        UIController.instance.leaderBoard.SetActive(true);
        foreach (PlayerInfo player in allPlayers)
        {
            player.kills = 0;
            player.deaths = 0;
        }
        
        UpdatesStatsDisplay();
        
        playerSpawner.instance.spawnPlayer();
        setUpTimer();
    }

    public void setUpTimer()
    {
        if (matchLength > 0)
        {
            currentMatchTimer = matchLength;
            updateTimer();
        }
    }

    public void updateTimer()
    {
        var TimeToDisplay = System.TimeSpan.FromSeconds(currentMatchTimer);
        UIController.instance.time.text = TimeToDisplay.Minutes.ToString("00") + ":" + TimeToDisplay.Seconds.ToString("00");
    }
    public void timerSend()
    {
        //timer network coding, master client send timer
    }
    public void timerRecieve()
    {
        //timer network coding, normal client recieve master client timer
    }
}


[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor, kills, deaths;

    public PlayerInfo(string _name, int _actor, int _kills, int _deaths)
    {
        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _deaths;
    }
}
