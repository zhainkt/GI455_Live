using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerExampleSec1;

public class GameManager_Sec1 : MonoBehaviour
{
    public string roomName;

    public void OnGUI()
    {
        if(SocketConnection_Sec1.instance.IsConnected() == false)
        {
            if (GUILayout.Button("Connect"))
            {
                SocketConnection_Sec1.instance.Connect();
            }
        }
        else
        {
            if(SocketConnection_Sec1.instance.currentRoom == null)
            {
                roomName = GUILayout.TextField(roomName);

                if(GUILayout.Button("CreateRoom"))
                {
                    MultiplayerExampleSec1.Room.RoomOption roomOption = new MultiplayerExampleSec1.Room.RoomOption();
                    roomOption.roomName = roomName;

                    SocketConnection_Sec1.instance.CreateRoom(roomOption);
                }
            }
        }
    }
}
