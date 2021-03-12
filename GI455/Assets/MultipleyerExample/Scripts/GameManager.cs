using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplyerExample;

public class GameManager : MonoBehaviour
{
    public SocketConnection socket;

    private string playerName;

    private string roomName;

    private int a = 0;

    private void Start()
    {
        socket.OnReplicateData += OnReplicateData;
    }

    void OnGUI()
    {
        if(socket.IsConnected() == false)
        {
            GUILayout.TextArea("PlayerName");
            playerName = GUILayout.TextField(playerName);
            if (GUILayout.Button("Connect"))
            {
                socket.Connect();
            }
        }
        else
        {
            if (socket.GetCurrentRoom() == null)
            {
                roomName = GUILayout.TextField(roomName);

                if (GUILayout.Button("CreateRoom"))
                {
                    Room.RoomOption roomOption = new Room.RoomOption();
                    roomOption.roomName = roomName;
                    roomOption.passward = "";
                    roomOption.maxPlayer = 4;
                    roomOption.mapName = "a1";
                    socket.CreateRoom(roomOption);
                }

                if (GUILayout.Button("JoinRoom"))
                {
                    Room.RoomOption roomOption = new Room.RoomOption();
                    roomOption.roomName = roomName;
                    roomOption.passward = "";
                    socket.JoinRoom(roomOption);
                }
            }
            else
            {
                //InRoom
                if (GUILayout.Button("LeaveRoom"))
                {
                    socket.LeaveRoom();
                }

                if(GUILayout.Button("SpawnNetworkObject"))
                {
                    socket.SpawnNetworkObject("NetObjectSphere", Vector3.zero, Quaternion.identity);
                }
            }
        }
    }

    void OnReplicateData(string data)
    {
        //Debug.Log(data);
    }
}
