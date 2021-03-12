using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChatWebSocket_Room;

[RequireComponent(typeof(WebSocketConnection))]
public class MidtermUI : MonoBehaviour
{
    private WebSocketConnection webSocket;

    private string studentID;
    private string answer;


    private void OnGUI()
    {
        if(webSocket.IsConnected())
        {
            studentID = GUILayout.TextField(studentID);
            answer = GUILayout.TextField(answer);

            if (GUILayout.Button("StartExam"))
            {
                webSocket.RequestToken(studentID);
            }

            if (GUILayout.Button("GetStudentData"))
            {
                webSocket.GetStudentData(studentID);
            }

            if (GUILayout.Button("RequestExamInfo"))
            {
                webSocket.RequestExamInfo(studentID);
            }

            if (GUILayout.Button("SendAnswer"))
            {
                webSocket.SendAnswer(studentID, answer);
            }
        }
    }

    public void Start()
    {
        webSocket = GetComponent<WebSocketConnection>();
        webSocket.Connect();
    }
}
