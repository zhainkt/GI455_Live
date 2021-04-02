using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerExampleSec1;

public class GameManager_Sec1 : MonoBehaviour
{
    [SerializeField]
    public class TestSendOnce
    {
        public int hp;
    }

    public string roomName;

    public int currentScore;

    public static GameManager_Sec1 instance;

    public void Awake()
    {
        instance = this;
    }

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

                if(GUILayout.Button("JoinRoom"))
                {
                    MultiplayerExampleSec1.Room.RoomOption roomOption = new MultiplayerExampleSec1.Room.RoomOption();
                    roomOption.roomName = roomName;
                    SocketConnection_Sec1.instance.JoinRoom(roomOption);
                }
            }
            else
            {
                if(GUILayout.Button("SpawnNetworkObject"))
                {
                    SocketConnection_Sec1.instance.SpawnNetworkObject("Sphere_1", Vector3.zero, Quaternion.identity);
                }
            }
        }
    }

    public void AddScore(int addedScore)
    {
        currentScore += addedScore;
    }
}
