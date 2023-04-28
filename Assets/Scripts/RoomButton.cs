using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour
{
    public Text buttonText;
    private RoomInfo whateverInfo;
    public void setButtonDetails(RoomInfo wateverInfo)
    {
        whateverInfo = wateverInfo;
        buttonText.text = whateverInfo.Name;
    }
    public void openRoom()
    {
        Laucher.instance.joinRoom(whateverInfo);
    }
}
