using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MultiplayerExampleSec1
{
    public class SocketConnection_Sec1 : MonoBehaviour
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

        private List<byte[]> binaryQueue = new List<byte[]>();

        private NetworkDataOption.ReplicateObjectList replicateListSend = new NetworkDataOption.ReplicateObjectList();

        private NetworkDataOption.ReplicateObjectList replicateListRecv = new NetworkDataOption.ReplicateObjectList();

        private NetworkDataOption.ReplicateObject tempSpawnNetworkObj;

        private string clientID;

        public string ClientID
        {
            get
            {
                return clientID;
            }
        }

        public static SocketConnection_Sec1 instance;

        public void Awake()
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

            if (ws.ReadyState == WebSocketState.Open)
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
            float duration = 0.1f;

            WaitForSeconds waitForSec = new WaitForSeconds(duration);

            while (true)
            {
                for(int i = 0; i < replicateListSend.replicateObjectList.Count; i++)
                {
                    if (replicateListSend.replicateObjectList[i].netObj != null)
                    {
                        replicateListSend.replicateObjectList[i].position = replicateListSend.replicateObjectList[i].netObj.transform.position;
                        replicateListSend.replicateObjectList[i].rotation = replicateListSend.replicateObjectList[i].netObj.transform.rotation;
                    }
                }

                string toJson = JsonUtility.ToJson(replicateListSend);

                //Debug.Log(toJson);

                SendReplicateData(toJson);

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

        public void CreateRoom(Room.RoomOption roomOption)
        {
            NetworkDataOption.EventSendCreateRoom eventData = new NetworkDataOption.EventSendCreateRoom();

            eventData.eventName = "CreateRoom";
            eventData.roomOption = roomOption;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void JoinRoom(Room.RoomOption roomOption)
        {
            NetworkDataOption.EventSendCreateRoom eventData = new NetworkDataOption.EventSendCreateRoom();

            eventData.eventName = "JoinRoom";
            eventData.roomOption = roomOption;

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

        public void SendReplicateData(string jsonStr)
        {
            NetworkDataOption.EventSendReplicate eventData = new NetworkDataOption.EventSendReplicate();
            eventData.eventName = "ReplicateData";
            eventData.data = jsonStr;
            eventData.roomName = currentRoom.roomOption.roomName;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void SpawnNetworkObject(string prefName, Vector3 position, Quaternion rotation)
        {
            NetworkDataOption.EventCallbackGeneral eventData = new NetworkDataOption.EventCallbackGeneral();
            eventData.eventName = "RequestUIDObject";

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);

            if(tempSpawnNetworkObj == null)
            {
                tempSpawnNetworkObj = new NetworkDataOption.ReplicateObject();
            }

            tempSpawnNetworkObj.prefName = prefName;
            tempSpawnNetworkObj.position = position;
            tempSpawnNetworkObj.rotation = rotation;
        }

        public void DestroyNetworkObject(string objectID)
        {
            for(int i = 0; i < replicateListSend.replicateObjectList.Count; i++)
            {
                if(replicateListSend.replicateObjectList[i].objectID == objectID)
                {
                    Destroy(replicateListSend.replicateObjectList[i].netObj.gameObject);
                    replicateListSend.replicateObjectList.RemoveAt(i);
                    break;
                }
            }
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

            if (binaryQueue.Count > 0)
            {
                NotifyBinaryCallback(binaryQueue[0]);
                binaryQueue.RemoveAt(0);
            }
        }

        private void NotifyCallback(string callbackData)
        {
            //Debug.Log("OnMessage : " + callbackData);

            EventServer recieveEvent = JsonUtility.FromJson<EventServer>(callbackData);

            Debug.Log(recieveEvent.eventName);

            switch (recieveEvent.eventName)
            {
                case "Connect":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        clientID = receiveEventGeneral.data;
                        break;
                    }
                case "CreateRoom":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);

                        Internal_CreateRoom(receiveEventGeneral.data);

                        if (OnCreateRoom != null)
                            OnCreateRoom(receiveEventGeneral.data);
                        break;
                    }
                case "JoinRoom":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);

                        Internal_JoinRoom(receiveEventGeneral.data);

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
                case "ReplicateData":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        Internal_ReplicateData(receiveEventGeneral.data);
                        break;
                    }
                case "RequestUIDObject":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        Internal_SpawnNetworkObject(receiveEventGeneral.data);
                        break;
                    }
                default:
                    break;
            }
        }

        private void NotifyBinaryCallback(byte[] byteArr)
        {
            NetworkDataOption.ReplicateObjectList toReplicateList = new NetworkDataOption.ReplicateObjectList();
            toReplicateList = toReplicateList.FromByteArr(byteArr);

            toReplicateList.replicateObjectList[0].position = Vector3.zero;
        }

        private void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            if(messageEventArgs.IsText)
                messageQueue.Add(messageEventArgs.Data);

            if(messageEventArgs.IsBinary)
                binaryQueue.Add(messageEventArgs.RawData);
        }

        private void Internal_CreateRoom(string data)
        {
            Room.RoomOption roomOption = JsonUtility.FromJson<Room.RoomOption>(data);

            if(roomOption != null && currentRoom == null)
            {
                currentRoom = new Room();
                currentRoom.roomOption = roomOption;

                StartCoroutine(IEUpdateReplicateObject());
            }
        }

        private void Internal_JoinRoom(string data)
        {
            Internal_CreateRoom(data);
        }

        private void Internal_ReplicateData(string data)
        {
            NetworkDataOption.ReplicateObjectList toReplicateList = JsonUtility.FromJson<NetworkDataOption.ReplicateObjectList>(data);

            //Remove
            if(toReplicateList.replicateObjectList.Count < replicateListRecv.replicateObjectList.Count)
            {
                for (int i = 0; i < replicateListRecv.replicateObjectList.Count; i++)
                {
                    bool isRemoveObject = true;
                    NetworkDataOption.ReplicateObject replicateObjClient = replicateListRecv.replicateObjectList[i];

                    for (int j = 0; j < toReplicateList.replicateObjectList.Count; j++)
                    {
                        NetworkDataOption.ReplicateObject replicateObjServer = toReplicateList.replicateObjectList[j];
                        if (replicateObjServer.objectID == replicateObjClient.objectID)
                        {
                            isRemoveObject = false;
                            break;
                        }
                    }

                    if (isRemoveObject == true)
                    {
                        Destroy(replicateObjClient.netObj.gameObject);
                        replicateListRecv.replicateObjectList.RemoveAt(i);
                        break;
                    }
                }
            }
            //For Create
            else if(toReplicateList.replicateObjectList.Count >= replicateListRecv.replicateObjectList.Count)
            {
                for (int i = 0; i < toReplicateList.replicateObjectList.Count; i++)
                {
                    NetworkDataOption.ReplicateObject replicateObj = toReplicateList.replicateObjectList[i];
                    bool isNewObject = true;
                    bool isNotSendObj = true;

                    string objectID = replicateObj.objectID;

                    for (int j = 0; j < replicateListRecv.replicateObjectList.Count; j++)
                    {
                        if (replicateListRecv.replicateObjectList[j].objectID == objectID)
                        {
                            replicateListRecv.replicateObjectList[j].netObj.replicateData = replicateObj;
                            isNewObject = false;
                        }
                    }

                    for (int j = 0; j < replicateListSend.replicateObjectList.Count; j++)
                    {
                        if (replicateListSend.replicateObjectList[j].objectID == objectID)
                        {
                            isNotSendObj = false;
                            break;
                        }
                    }

                    if (isNewObject == true && isNotSendObj == true)
                    {
                        GameObject prefObj = Resources.Load(replicateObj.prefName) as GameObject;
                        GameObject newGameObject = Instantiate(prefObj, replicateObj.position, replicateObj.rotation);
                        NetworkObject newNetObj = newGameObject.GetComponent<NetworkObject>();
                        replicateObj.netObj = newNetObj;
                        newNetObj.replicateData = replicateObj;
                        replicateListRecv.replicateObjectList.Add(replicateObj);
                    }
                }
            }
        }

        private void Internal_SpawnNetworkObject(string data)
        {
            if (tempSpawnNetworkObj == null || tempSpawnNetworkObj.prefName == "")
                return;

            NetworkDataOption.ReplicateObject newReplicateData = new NetworkDataOption.ReplicateObject();
            newReplicateData.ownerID = clientID;
            newReplicateData.objectID = data;
            newReplicateData.prefName = tempSpawnNetworkObj.prefName;
            newReplicateData.position = tempSpawnNetworkObj.position;
            newReplicateData.rotation = tempSpawnNetworkObj.rotation;

            GameObject prefObj = Resources.Load(newReplicateData.prefName) as GameObject;
            GameObject newGameObject = Instantiate(prefObj, newReplicateData.position, newReplicateData.rotation);

            newReplicateData.netObj = newGameObject.GetComponent<NetworkObject>();
            newReplicateData.netObj.replicateData = newReplicateData;

            replicateListSend.replicateObjectList.Add(newReplicateData);

            tempSpawnNetworkObj = null;
        }
    }
}


