using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class leaderboardScript : MonoBehaviour
{
    public Text playerName;
    public Text kills, deaths;
    public void setDetails(string name, int kill, int death)
    {
        playerName.text = name;
        kills.text = kill.ToString();
        deaths.text = death.ToString();
    }
}
