using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System;

namespace MultiplayerExampleSec2
{
    public class SocketConnection_Sec2 : MonoBehaviour
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
        public class EventSendAnswer : EventServer
        {
            public string token;
            public string answer;
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
        public event DelegateHandler OnDisconnect;

        public event DelegateHandlerAddMoney OnAddMoney;

        private bool isConnection;

        public Room currentRoom;

        private List<string> messageQueue = new List<string>();

        private NetworkDataOption.ReplicateObjectList replicateObjectList = new NetworkDataOption.ReplicateObjectList();

        private static SocketConnection_Sec2 instance;

        public static SocketConnection_Sec2 Instance
        {
            get
            {
                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        public void Connect(string ip, int port)
        {
            string url = $"ws://{ip}:{port}/";

            InternalConnect(url);
        }

        public void Connect()
        {
            string url = "ws://127.0.0.1:8080/";
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

        private IEnumerator IEUpdateReplicateObject()
        {
            float duration = 1.0f;

            WaitForSeconds waitForSec = new WaitForSeconds(duration);

            while(true)
            {
                if(currentRoom != null)
                {
                    string toJson = JsonUtility.ToJson(replicateObjectList);

                    SendReplicateData(toJson);

                }
                yield return waitForSec;
            }
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

        public void SendReplicateData(string jsonStr)
        {
            NetworkDataOption.EventCallbackGeneral eventData = new NetworkDataOption.EventCallbackGeneral();
            eventData.eventName = "ReplicateData";
            eventData.data = jsonStr;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void CreateRoom(Room.RoomOption roomOption)
        {
            NetworkDataOption.EventSendCreateRoom eventData = new NetworkDataOption.EventSendCreateRoom();

            eventData.eventName = "CreateRoom";
            eventData.roomOption = roomOption;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void JoinRoom(string roomName)
        {
            NetworkDataOption.EventCallbackGeneral eventData = new NetworkDataOption.EventCallbackGeneral();

            eventData.eventName = "JoinRoom";
            eventData.data = roomName;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void LeaveRoom()
        {
            NetworkDataOption.EventCallbackGeneral eventData = new NetworkDataOption.EventCallbackGeneral(); ;

            eventData.eventName = "LeaveRoom";
            eventData.data = "";

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void RequestToken(string studentID)
        {
            EventStudent eventData = new EventStudent();
            eventData.eventName = "RequestToken";
            //eventData.eventName = "StartExam";
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

        public void SendAnswer(string token, string answer)
        {
            EventSendAnswer eventData = new EventSendAnswer();
            eventData.eventName = "SendAnswer";
            eventData.token = token;
            eventData.answer = answer;

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);
        }

        public void Login(string userId, string password)
        {
            NetworkDataOption.EventCallbackGeneral eventData = new NetworkDataOption.EventCallbackGeneral();
            eventData.eventName = "Login";
            eventData.data = userId + "#" + password;

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);
        }

        public void Register(string userId, string password, string name)
        {
            NetworkDataOption.EventCallbackGeneral eventData = new NetworkDataOption.EventCallbackGeneral();
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

            NetworkDataOption.EventCallbackGeneral msgEventData = new NetworkDataOption.EventCallbackGeneral();
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
                        NetworkDataOption.EventSendCreateRoom receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventSendCreateRoom>(callbackData);

                        Internal_CreateRoom(receiveEventGeneral.roomOption);

                        if (OnCreateRoom != null)
                            OnCreateRoom(receiveEventGeneral.data);
                        break;
                    }
                case "JoinRoom":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        if (OnJoinRoom != null)
                            OnJoinRoom(receiveEventGeneral.data);
                        break;
                    }
                case "LeaveRoom":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        if (OnLeaveRoom != null)
                            OnLeaveRoom(receiveEventGeneral.data);
                        break;
                    }
                case "SendMessage":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
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
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        if (OnLogin != null)
                            OnLogin(receiveEventGeneral.data);
                        break;
                    }
                case "Register":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
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
                case " SendAnswer":
                    {
                        break;
                    }
            }
        }

        private void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            messageQueue.Add(messageEventArgs.Data);
        }

        private void Internal_CreateRoom(Room.RoomOption roomOption)
        {
            if(roomOption != null && currentRoom == null)
            {
                currentRoom = new Room();
                currentRoom.roomOption = roomOption;

                StartCoroutine(IEUpdateReplicateObject());
            }
        }
    }
}


