using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System;

namespace MultiplyerExample
{
    public class SocketConnection : MonoBehaviour
    {
        public float intervalSendTime = 8;

        private WebSocket ws;

        public delegate void DelegateHandler(string msg);
        public delegate void DelegateHandler_NoParam();

        public event DelegateHandler OnConnectionSuccess;
        public event DelegateHandler OnConnectionFail;
        public event DelegateHandler OnReceiveMessage;
        public event DelegateHandler_NoParam OnCreateRoomSuccess;
        public event DelegateHandler_NoParam OnCreateRoomFail;
        public event DelegateHandler_NoParam OnJoinRoomSuccess;
        public event DelegateHandler_NoParam OnJoinRoomFail;
        public event DelegateHandler OnLeaveRoom;
        public event DelegateHandler OnLogin;
        public event DelegateHandler OnRegister;
        public event DelegateHandler OnDisconnect;
        public event DelegateHandler OnReplicateData;

        private bool isConnection;

        public string clientID;

        private List<string> messageQueue = new List<string>();

        private Room currentRoom;

        public ReplicateList replicateSend = new ReplicateList();

        private List<ReplicateObject> replicateObjectList = new List<ReplicateObject>();

        private ReplicateObject tempSpawnNetworkObj;

        public static SocketConnection instance;

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

                StartCoroutine(IEUpdateReplicateObject());
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

        public void CreateRoom(Room.RoomOption roomOption)
        {
            CreateRoomData data = new CreateRoomData();

            data.eventName = "CreateRoom";
            data.roomOption = roomOption;

            string toJson = JsonUtility.ToJson(data);

            ws.Send(toJson);
        }

        public void JoinRoom(Room.RoomOption roomOption)
        {
            CreateRoomData data = new CreateRoomData();

            data.eventName = "JoinRoom";
            data.roomOption = roomOption;

            string toJson = JsonUtility.ToJson(data);

            ws.Send(toJson);
        }

        public void LeaveRoom()
        {
            ServerData data = new ServerData(); ;

            data.eventName = "LeaveRoom";

            string toJson = JsonUtility.ToJson(data);

            ws.Send(toJson);
        }

        public void Login(LoginOption loginOption)
        {
            LoginData data = new LoginData();
            data.eventName = "Login";
            data.loginOption = loginOption;

            string toJson = JsonUtility.ToJson(data);
            ws.Send(toJson);
        }

        public void Register(RegisterOption registerOption)
        {
            RegisterData data = new RegisterData();
            data.eventName = "Register";
            data.registerOption = registerOption;

            string toJson = JsonUtility.ToJson(data);
            ws.Send(toJson);
        }

        public void SpawnNetworkObject(string prefName, Vector3 position, Quaternion rotation)
        {
            ServerData data = new ServerData();
            data.eventName = "RequestUIDObject";

            string toJson = JsonUtility.ToJson(data);
            ws.Send(toJson);

            if(tempSpawnNetworkObj == null)
            {
                tempSpawnNetworkObj = new ReplicateObject();
            }

            tempSpawnNetworkObj.prefName = prefName;
            tempSpawnNetworkObj.position = position;
            tempSpawnNetworkObj.rotation = rotation;
        }

        public void DestroyNetworkObject(string objectID)
        {
            for (int i = 0; i < replicateSend.replicateObjectList.Count; i++)
            {
                if (replicateSend.replicateObjectList[i].objectID == objectID)
                {
                    replicateSend.replicateObjectList[i].isMarkRemove = true;
                    break;
                }
            }
        }

        public Room GetCurrentRoom()
        {
            return currentRoom;
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (ws != null)
                ws.Close();
        }

        private void Update()
        {
            if (messageQueue.Count > 0)
            {
                NotifyCallback(messageQueue[0]);
                messageQueue.RemoveAt(0);
            }
        }

        private IEnumerator IEUpdateReplicateObject()
        {
            float sendPerSecond = 1 / intervalSendTime;

            while (true)
            {
                if (currentRoom != null)
                {
                    for (int i = 0; i < replicateSend.replicateObjectList.Count; i++)
                    {
                        if(replicateSend.replicateObjectList[i].netObject != null && replicateSend.replicateObjectList[i].isMarkRemove == false)
                        {
                            replicateSend.replicateObjectList[i].position = replicateSend.replicateObjectList[i].netObject.transform.position;
                            replicateSend.replicateObjectList[i].rotation = replicateSend.replicateObjectList[i].netObject.transform.rotation;
                        }
                        else if(replicateSend.replicateObjectList[i].netObject != null && replicateSend.replicateObjectList[i].isMarkRemove == true)
                        {
                            Destroy(replicateSend.replicateObjectList[i].netObject.gameObject);
                        }
                    }

                    string toJson = JsonUtility.ToJson(replicateSend);
                    //Debug.Log(toJson);
                    ReplicateData(toJson);
                }

                yield return new WaitForSeconds(sendPerSecond);
            }

            
        }

        private void NotifyCallback(string callbackData)
        {
            //Debug.Log("OnMessage : " + callbackData);

            ServerData recieveEvent = JsonUtility.FromJson<ServerData>(callbackData);

            //Debug.Log(recieveEvent.eventName);
            if(recieveEvent == null)
            {
                return;
            }

            switch (recieveEvent.eventName)
            {
                case "Connect":
                    {
                        clientID = recieveEvent.data;
                        break;
                    }
                case "CreateRoom":
                    {
                        if(recieveEvent.status == true)
                        {
                            Internal_OnCreateRoom(recieveEvent.data);

                            if (OnCreateRoomSuccess != null)
                                OnCreateRoomSuccess();

                        }
                        else
                        {
                            if (OnCreateRoomFail != null)
                                OnCreateRoomFail();
                        }
                        
                        break;
                    }
                case "JoinRoom":
                    {
                        if(recieveEvent.status == true)
                        {
                            Internal_OnJoinRoom(callbackData);

                            if (OnJoinRoomSuccess != null)
                                OnJoinRoomSuccess();
                        }
                        else
                        {
                            if (OnJoinRoomFail != null)
                                OnJoinRoomFail();
                        }

                        
                        break;
                    }
                case "LeaveRoom":
                    {
                        if(recieveEvent.status == true)
                        {
                            Internal_OnLeaveRoom();

                            if (OnLeaveRoom != null)
                                OnLeaveRoom(recieveEvent.data);
                        }

                        break;
                    }
                case "Login":
                    {
                        if (OnLogin != null)
                            OnLogin(recieveEvent.data);
                        break;
                    }
                case "Register":
                    {
                        if (OnRegister != null)
                            OnRegister(recieveEvent.data);
                        break;
                    }
                case "ReplicateData":
                    {
                        Internal_ReplicateData(callbackData);
                        break;
                    }
                default:
                    {
                        Internal_SpawnNetworkObject(recieveEvent.data);
                        break;
                    }
            }
        }

        private void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            messageQueue.Add(messageEventArgs.Data);
        }

        private void ReplicateData(string dataStr)
        {
            ServerData data = new ServerData();
            data.eventName = "ReplicateData";
            data.data = dataStr;

            string toJson = JsonUtility.ToJson(data);
            ws.Send(toJson);
        }

        private void Internal_OnCreateRoom(string callbackData)
        {
            CreateRoomData createRoomData = JsonUtility.FromJson<CreateRoomData>(callbackData);
            Room newRoom = new Room();
            newRoom.roomOption = createRoomData.roomOption;
            currentRoom = newRoom;
        }

        private void Internal_OnJoinRoom(string callbackData)
        {
            Debug.Log(callbackData);
            CreateRoomData createRoomData = JsonUtility.FromJson<CreateRoomData>(callbackData);
            Room newRoom = new Room();
            newRoom.roomOption = createRoomData.roomOption;
            currentRoom = newRoom;
        }

        private void Internal_OnLeaveRoom()
        {
            currentRoom = null;
        }

        private void Internal_ReplicateData(string jsonStr)
        {
            ReplicateData replicateData = JsonUtility.FromJson<ReplicateData>(jsonStr);

            if (OnReplicateData != null)
            {
                OnReplicateData(replicateData.replicateData);
            }

            ReplicateList toReplicateList = JsonUtility.FromJson<ReplicateList>(replicateData.replicateData);

            for (int i = 0; i < toReplicateList.replicateObjectList.Count; i++)
            {
                ReplicateObject replicateObj = toReplicateList.replicateObjectList[i];
                bool isNewObject = true;
                bool isNotExist = true;

                for (int j = 0; j < replicateObjectList.Count; j++)
                {
                    if (replicateObj.objectID == replicateObjectList[j].objectID)
                    {
                        if (replicateObj.isMarkRemove == false)
                        {
                            replicateObjectList[j].position = replicateObj.position;
                            replicateObjectList[j].rotation = replicateObj.rotation;

                            if (replicateObjectList[j].netObject != null)
                            {
                                replicateObjectList[j].netObject.position = replicateObjectList[j].position;
                                replicateObjectList[j].netObject.rotation = replicateObjectList[j].rotation;
                            }

                            
                        }
                        else if (replicateObj.isMarkRemove == true && replicateObjectList[j].netObject != null)
                        {
                            Destroy(replicateObjectList[j].netObject.gameObject);
                            replicateObjectList.RemoveAt(j);
                            break;
                        }

                        isNewObject = false;
                    }
                }

                for (int j = 0; j < replicateSend.replicateObjectList.Count; j++)
                {
                    if (replicateObj.objectID == replicateSend.replicateObjectList[j].objectID)
                    {
                        isNotExist = false;

                        if(replicateObj.isMarkRemove)
                        {
                            replicateSend.replicateObjectList.RemoveAt(j);
                            break;
                        }
                    }
                }

                if (isNewObject && isNotExist && replicateObj.isMarkRemove == false)
                {
                    GameObject newGameObject = Instantiate(Resources.Load(replicateObj.prefName)) as GameObject;
                    Debug.LogError("CreateNetObject");
                    NetObject newNetObject = newGameObject.GetComponent<NetObject>();
                    newNetObject.ownerID = replicateObj.ownerID;
                    newNetObject.objectID = replicateObj.objectID;
                    newNetObject.position = replicateObj.position;
                    newNetObject.rotation = replicateObj.rotation;
                    newNetObject.transform.position = replicateObj.position;
                    newNetObject.transform.rotation = replicateObj.rotation;
                    replicateObj.netObject = newNetObject;
                    replicateObjectList.Add(replicateObj);
                }
            }

            //Debug.Log(toReplicateList.replicateObjectList.Count);
        }

        private void Internal_SpawnNetworkObject(string uid)
        {
            if(tempSpawnNetworkObj == null || tempSpawnNetworkObj.prefName == "")
            {
                return;
            }

            ReplicateObject newReplicateObject = new ReplicateObject();
            newReplicateObject.ownerID = clientID;
            newReplicateObject.objectID = uid;
            newReplicateObject.prefName = tempSpawnNetworkObj.prefName;
            newReplicateObject.position = tempSpawnNetworkObj.position;
            newReplicateObject.rotation = tempSpawnNetworkObj.rotation;

            GameObject newGameObject = Instantiate(Resources.Load(newReplicateObject.prefName)) as GameObject;
            NetObject newNetObject = newGameObject.GetComponent<NetObject>();
            newNetObject.ownerID = clientID;
            newNetObject.objectID = uid;
            newNetObject.position = newReplicateObject.position;
            newNetObject.rotation = newReplicateObject.rotation;
            newNetObject.transform.position = newReplicateObject.position;
            newNetObject.transform.rotation = newReplicateObject.rotation;
            newReplicateObject.netObject = newNetObject;

            Debug.LogError("Add new replicate");
            replicateSend.replicateObjectList.Add(newReplicateObject);

            tempSpawnNetworkObj.prefName = "";
        }
    }
}


