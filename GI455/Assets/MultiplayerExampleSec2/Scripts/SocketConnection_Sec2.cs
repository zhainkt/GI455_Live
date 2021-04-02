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

        private string clientID;

        public string ClientID
        {
            get
            {
                return clientID;
            }
        }

        private List<string> messageQueue = new List<string>();

        private List<byte[]> binaryQueue = new List<byte[]>();

        private NetworkDataOption.ReplicateObjectList replicateObjectList = new NetworkDataOption.ReplicateObjectList();

        private NetworkDataOption.ReplicateObjectList replicateObjectRecvList = new NetworkDataOption.ReplicateObjectList();

        private NetworkDataOption.ReplicateObject tempSpawnReplicateObj = new NetworkDataOption.ReplicateObject();

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
            float duration = 0.05f;

            WaitForSeconds waitForSec = new WaitForSeconds(duration);

            while(true)
            {
                if(currentRoom != null)
                {
                    string toJson = JsonUtility.ToJson(replicateObjectList);

                    for(int i = 0; i < replicateObjectList.replicateObjectList.Count; i++)
                    {
                        if(replicateObjectList.replicateObjectList[i].netObj != null)
                        {
                            replicateObjectList.replicateObjectList[i].position = replicateObjectList.replicateObjectList[i].netObj.transform.position;
                            replicateObjectList.replicateObjectList[i].rotation = replicateObjectList.replicateObjectList[i].netObj.transform.rotation;
                        }
                    }

                    Debug.Log(toJson);
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
            NetworkDataOption.EventSendReplicateData eventData = new NetworkDataOption.EventSendReplicateData();
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

            if(tempSpawnReplicateObj == null)
            {
                tempSpawnReplicateObj = new NetworkDataOption.ReplicateObject();
            }

            tempSpawnReplicateObj.prefName = prefName;
            tempSpawnReplicateObj.position = position;
            tempSpawnReplicateObj.rotation = rotation;
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

        private void NotifyBinaryCallback(byte[] byteArr)
        {
            //NetworkDataOption.ReplicateObjectList newReplicateObjList = NetworkDataOption.ReplicateObjectList.FromByteArr(byteArr);
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

                        if (receiveEventGeneral.status == true)
                        {
                            Internal_CreateRoom(receiveEventGeneral.roomOption);
                        }

                        if (OnCreateRoom != null)
                            OnCreateRoom(receiveEventGeneral.data);
                        break;
                    }
                case "JoinRoom":
                    {
                        NetworkDataOption.EventSendCreateRoom receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventSendCreateRoom>(callbackData);

                        if(receiveEventGeneral.status == true)
                        {
                            Internal_JoinRoom(receiveEventGeneral.roomOption);
                        }
                        
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
                case "RequestUIDObject":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);

                        Internal_SpawnNetworkObject(receiveEventGeneral.data);

                        break;
                    }
                case "ReplicateData":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);

                        Internal_ReplicateData(receiveEventGeneral.data);
                        break;
                    }
                case "Connect":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);

                        clientID = receiveEventGeneral.data;

                        break;
                    }
                default:
                    break;
            }
        }

        private void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            if(messageEventArgs.IsText)
                messageQueue.Add(messageEventArgs.Data);

            if (messageEventArgs.IsBinary)
                binaryQueue.Add(messageEventArgs.RawData);
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

        private void Internal_JoinRoom(Room.RoomOption roomOption)
        {
            Internal_CreateRoom(roomOption);
        }

        private void Internal_SpawnNetworkObject(string uid)
        {
            if (tempSpawnReplicateObj == null || tempSpawnReplicateObj.prefName == "")
                return;

            NetworkDataOption.ReplicateObject newReplicateData = new NetworkDataOption.ReplicateObject();
            newReplicateData.ownerID = clientID;
            newReplicateData.objectID = uid;
            newReplicateData.prefName = tempSpawnReplicateObj.prefName;
            newReplicateData.position = tempSpawnReplicateObj.position;
            newReplicateData.rotation = tempSpawnReplicateObj.rotation;

            GameObject prefObj = Resources.Load(newReplicateData.prefName) as GameObject;
            GameObject newGameObject = Instantiate(prefObj, newReplicateData.position, newReplicateData.rotation);

            newReplicateData.netObj = newGameObject.GetComponent<NetworkObject_Sec2>();
            newReplicateData.netObj.replicateData = newReplicateData;

            replicateObjectList.replicateObjectList.Add(newReplicateData);

            tempSpawnReplicateObj = null;
        }

        public int countData = 0;

        private void Internal_ReplicateData(string replicateData)
        {
            //Debug.Log("Internal_ReplicateData : " + replicateData);

            NetworkDataOption.ReplicateObjectList toReplicateList = JsonUtility.FromJson<NetworkDataOption.ReplicateObjectList>(replicateData);

            countData++;

            Debug.Log("countData : " + countData + " / fromServer=[" + toReplicateList.replicateObjectList.Count + "] / fromClient=[" + replicateObjectRecvList.replicateObjectList.Count + "]");

            //Create and Update
            if (toReplicateList.replicateObjectList.Count >= replicateObjectRecvList.replicateObjectList.Count)
            {
                for (int i = 0; i < toReplicateList.replicateObjectList.Count; i++)
                {
                    NetworkDataOption.ReplicateObject replicateObj = toReplicateList.replicateObjectList[i];
                    bool isNewObject = true;
                    bool isNotSendObject = true;


                    for (int j = 0; j < replicateObjectRecvList.replicateObjectList.Count; j++)
                    {
                        NetworkDataOption.ReplicateObject replicateLocalObj = replicateObjectRecvList.replicateObjectList[j];
                        if (replicateLocalObj.objectID == replicateObj.objectID)
                        {
                            replicateLocalObj.netObj.replicateData = replicateObj;

                            isNewObject = false;
                        }
                    }

                    for (int j = 0; j < replicateObjectList.replicateObjectList.Count; j++)
                    {
                        NetworkDataOption.ReplicateObject replicateLocalObj = replicateObjectList.replicateObjectList[j];
                        if (replicateLocalObj.objectID == replicateObj.objectID)
                        {
                            isNotSendObject = false;
                        }
                    }

                    if (isNewObject && isNotSendObject)
                    {
                        GameObject prefObj = Resources.Load(replicateObj.prefName) as GameObject;
                        GameObject newGameObject = Instantiate(prefObj, replicateObj.position, replicateObj.rotation);
                        NetworkObject_Sec2 newNetObj = newGameObject.GetComponent<NetworkObject_Sec2>();
                        replicateObj.netObj = newNetObj;
                        newNetObj.replicateData = replicateObj;
                        replicateObjectRecvList.replicateObjectList.Add(replicateObj);
                    }
                }
            }
            //Remove
            else if(toReplicateList.replicateObjectList.Count < replicateObjectRecvList.replicateObjectList.Count)
            {
                for(int i = 0; i < replicateObjectRecvList.replicateObjectList.Count; i++)
                {
                    NetworkDataOption.ReplicateObject replicateObj = replicateObjectRecvList.replicateObjectList[i];
                    bool isRemoveObj = true;

                    for(int j = 0; j < toReplicateList.replicateObjectList.Count; j++)
                    {
                        if(replicateObj.objectID == toReplicateList.replicateObjectList[j].objectID)
                        {
                            isRemoveObj = false;
                            break;
                        }
                    }

                    if(isRemoveObj == true)
                    {
                        Destroy(replicateObj.netObj.gameObject);
                        replicateObjectRecvList.replicateObjectList.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}


