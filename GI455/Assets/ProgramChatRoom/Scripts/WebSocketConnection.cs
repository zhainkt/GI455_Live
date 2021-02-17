using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System;

namespace ChatWebSocket_Room
{
    public class WebSocketConnection : MonoBehaviour
    {
        [System.Serializable]
        public struct RoomData
        {
            public string roomName;
        }

        [System.Serializable]
        public struct MessageEventData
        {
            public string Event;
            public string Msg;
        }

        [System.Serializable]
        public struct StudentData
        {
            public string studentID;

            public StudentData(string studentID)
            {
                this.studentID = studentID;
            }
        }

        [System.Serializable]
        public class WSEvent
        {
            public string eventName;
        }

        [System.Serializable]
        public class EventServer : WSEvent
        {
            public string data;
        }

        [System.Serializable]
        public class EventStudent : WSEvent
        {
            public StudentData data;
        }

        private WebSocket ws;

        public delegate void DelegateHandler(string msg);

        public event DelegateHandler OnConnectionSuccess;
        public event DelegateHandler OnConnectionFail;
        public event DelegateHandler OnReceiveMessage;
        public event DelegateHandler OnCreateRoom;
        public event DelegateHandler OnJoinRoom;
        public event DelegateHandler OnLeaveRoom;

        private bool isConnection;

        private string callbackData;
        private float countDataTime;
        private float currentDataTime;

        private List<string> messageQueue = new List<string>();

        public void Connect(string ip, int port)
        {
            string url = $"ws://{ip}:{port}/";

            InternalConnect(url);
        }

        public void Connect()
        {
            string url = "ws://gi455-305013.an.r.appspot.com/";
            //string url = "ws://127.0.0.1:8080/";
            InternalConnect(url);
        }

        private void InternalConnect(string url)
        {
            if (isConnection)
                return;

            isConnection = true;

            ws = new WebSocket(url);

            ws.OnMessage += OnMessage;

            ws.Connect();

            StartCoroutine(WaitingConnectionState());
        }

        private IEnumerator WaitingConnectionState()
        {
            yield return new WaitForSeconds(1.0f);

            if(ws.ReadyState == WebSocketState.Open)
            {
                if (OnConnectionSuccess != null)
                    OnConnectionSuccess("Success");
            }
            else
            {
                if (OnConnectionFail != null)
                    OnConnectionFail("Fail");
            }

            isConnection = false;
        }

        public void Disconnect()
        {
            if (ws != null)
                ws.Close();
        }

        public bool IsConnected()
        {
            if (ws == null)
                return false;

            return ws.ReadyState == WebSocketState.Open;
        }

        public void CreateRoom(string roomName)
        {
            var eventData = new EventServer();

            eventData.eventName = "CreateRoom";
            eventData.data = roomName;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void JoinRoom(string roomName)
        {
            EventServer eventData = new EventServer();

            eventData.eventName = "JoinRoom";
            eventData.data = roomName;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void LeaveRoom()
        {
            EventServer eventData = new EventServer(); ;

            eventData.eventName = "LeaveRoom";
            eventData.data = "";

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void RequestToken(string studentID)
        {
            EventStudent eventData = new EventStudent();
            eventData.eventName = "RequestToken";
            eventData.data = new StudentData(studentID);

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);
        }

        public void GetStudentData(string studentID)
        {
            EventStudent eventData = new EventStudent();
            eventData.eventName = "GetStudentData";
            eventData.data = new StudentData(studentID);

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);
        }

        public void Send(string data)
        {
            if (!IsConnected())
                return;

            MessageEventData msgEventData;
            msgEventData.Event = "message";
            msgEventData.Msg = data;

            string toJson = JsonUtility.ToJson(msgEventData);

            ws.Send(toJson);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (ws != null)
                ws.Close();
        }

        private void Update()
        {
            if(messageQueue.Count > 0)
            {
                NotifyCallback(messageQueue[0]);
                messageQueue.RemoveAt(0);
            }
        }

        private void NotifyCallback(string callbackData)
        {
            Debug.Log("OnMessage : " + callbackData);

            EventServer recieveEvent = JsonUtility.FromJson<EventServer>(callbackData);

            switch (recieveEvent.eventName)
            {
                case "CreateRoom":
                    {
                        if (OnCreateRoom != null)
                            OnCreateRoom(recieveEvent.data);
                        break;
                    }
                case "JoinRoom":
                    {
                        if (OnJoinRoom != null)
                            OnJoinRoom(recieveEvent.data);
                        break;
                    }
                case "LeaveRoom":
                    {
                        if (OnLeaveRoom != null)
                            OnLeaveRoom(recieveEvent.data);
                        break;
                    }
                case "Message":
                    {
                        Debug.Log("message : "+ recieveEvent.data);
                        if (OnReceiveMessage != null)
                            OnReceiveMessage(recieveEvent.data);
                        break;
                    }
                case "RequestToken":
                    {
                        Debug.Log("message : " + recieveEvent.data);
                        break;
                    }
            }
        }

        private void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            messageQueue.Add(messageEventArgs.Data);
        }
    }
}


