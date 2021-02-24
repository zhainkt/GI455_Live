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
        public class EventServer
        {
            public string eventName;
        }

        [System.Serializable]
        public class EventStudent : EventServer
        {
            public string studentID;
        }

        [System.Serializable]
        public class EventToken : EventServer
        {
            public string token;
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
        public class EventAddMoney
        {
            public string eventName;
            public string userID;
            public int addMoney;
        }

        [System.Serializable]
        public class EventCallbackAddMoney
        {
            //public string eventName;
            public string status;
            public int data;
        }

        [System.Serializable]
        public class EventCallbackGeneral
        {
            public string eventName;
            public string data;
        }

        

        private WebSocket ws;

        public delegate void DelegateHandler(string msg);
        public delegate void DelegateHandlerAddMoney(string status, int money);

        public event DelegateHandler OnConnectionSuccess;
        public event DelegateHandler OnConnectionFail;
        public event DelegateHandler OnReceiveMessage;
        public event DelegateHandler OnCreateRoom;
        public event DelegateHandler OnJoinRoom;
        public event DelegateHandler OnLeaveRoom;
        public event DelegateHandler OnLogin;
        public event DelegateHandler OnRegister;

        public event DelegateHandlerAddMoney OnAddMoney;

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
            EventCallbackGeneral eventData = new EventCallbackGeneral();

            eventData.eventName = "CreateRoom";
            eventData.data = roomName;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void JoinRoom(string roomName)
        {
            EventCallbackGeneral eventData = new EventCallbackGeneral();

            eventData.eventName = "JoinRoom";
            eventData.data = roomName;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void LeaveRoom()
        {
            EventCallbackGeneral eventData = new EventCallbackGeneral(); ;

            eventData.eventName = "LeaveRoom";
            eventData.data = "";

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void RequestToken(string studentID)
        {
            EventStudent eventData = new EventStudent();
            eventData.eventName = "RequestToken";
            eventData.studentID = studentID;

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);
        }

        public void GetStudentData(string studentID)
        {
            EventStudent eventData = new EventStudent();
            eventData.eventName = "GetStudentData";
            eventData.studentID = studentID;

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);
        }

        public void RequestExamInfo(string token)
        {
            EventToken eventData = new EventToken();
            eventData.eventName = "RequestExamInfo";
            eventData.token = token;

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);
        }

        public void Login(string userId, string password)
        {
            EventCallbackGeneral eventData = new EventCallbackGeneral();
            eventData.eventName = "Login";
            eventData.data = userId + "#" + password;

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);
        }

        public void Register(string userId, string password, string name)
        {
            EventCallbackGeneral eventData = new EventCallbackGeneral();
            eventData.eventName = "Register";
            eventData.data = userId + "#" + password+"#"+name;

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);
        }

        public void AddMoney()
        {
            EventAddMoney eventData = new EventAddMoney(); ;

            eventData.eventName = "AddMoney";
            eventData.userID = "test0005";
            eventData.addMoney = 100;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void SendMessage(string data)
        {
            if (!IsConnected())
                return;

            EventCallbackGeneral msgEventData = new EventCallbackGeneral();
            msgEventData.eventName = "SendMessage";
            msgEventData.data = data;

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

            Debug.Log(recieveEvent.eventName);

            switch (recieveEvent.eventName)
            {
                case "CreateRoom":
                    {
                        EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<EventCallbackGeneral>(callbackData);
                        if (OnCreateRoom != null)
                            OnCreateRoom(receiveEventGeneral.data);
                        break;
                    }
                case "JoinRoom":
                    {
                        EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<EventCallbackGeneral>(callbackData);
                        if (OnJoinRoom != null)
                            OnJoinRoom(receiveEventGeneral.data);
                        break;
                    }
                case "LeaveRoom":
                    {
                        EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<EventCallbackGeneral>(callbackData);
                        if (OnLeaveRoom != null)
                            OnLeaveRoom(receiveEventGeneral.data);
                        break;
                    }
                case "SendMessage":
                    {
                        EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<EventCallbackGeneral>(callbackData);
                        if (OnReceiveMessage != null)
                            OnReceiveMessage(receiveEventGeneral.data);
                        break;
                    }
                case "RequestToken":
                    {
                        //Debug.Log("message : " + (string)recieveEvent.data);
                        break;
                    }
                case "Login":
                    {
                        EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<EventCallbackGeneral>(callbackData);
                        if (OnLogin != null)
                            OnLogin(receiveEventGeneral.data);
                        break;
                    }
                case "Register":
                    {
                        EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<EventCallbackGeneral>(callbackData);
                        if (OnRegister != null)
                            OnRegister(receiveEventGeneral.data);
                        break;
                    }
                case "AddMoney":
                    {
                        EventCallbackAddMoney receiveAddMoney = JsonUtility.FromJson<EventCallbackAddMoney>(callbackData);
                        if (OnAddMoney != null)
                            OnAddMoney(receiveAddMoney.status, receiveAddMoney.data);
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


