using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerExampleSec2;

public class GameManager_Sec2 : MonoBehaviour
{
    public string roomName;

    private void OnGUI()
    {
        if(SocketConnection_Sec2.Instance.IsConnected() == false)
        {
            if (GUILayout.Button("Connect"))
            {
                SocketConnection_Sec2.Instance.Connect();
            }
        }
        else
        {
            if(SocketConnection_Sec2.Instance.currentRoom == null)
            {
                roomName = GUILayout.TextField(roomName);
                if (GUILayout.Button("CreateRoom"))
                {
                    MultiplayerExampleSec2.Room.RoomOption newRoomOption = new MultiplayerExampleSec2.Room.RoomOption();
                    newRoomOption.roomName = roomName;
                    newRoomOption.mapName = "A1";

                    SocketConnection_Sec2.Instance.CreateRoom(newRoomOption);
                }

                if(GUILayout.Button("JoinRoom"))
                {
                    MultiplayerExampleSec2.Room.RoomOption newRoomOption = new MultiplayerExampleSec2.Room.RoomOption();
                    newRoomOption.roomName = roomName;

                    SocketConnection_Sec2.Instance.JoinRoom(newRoomOption);
                }
            }
            else
            {
                if(GUILayout.Button("SpawnNetworkObject"))
                {
                    SocketConnection_Sec2.Instance.SpawnNetworkObject("Sphere", Vector3.zero, Quaternion.identity);
                }
            }
        }
    }
}
