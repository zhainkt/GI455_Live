using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System;
using System.Linq;

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
        private List<byte[]> binaryQueue = new List<byte[]>();

        private Room currentRoom;

        public ReplicateList replicateSend = new ReplicateList();

        private Dictionary<string, ReplicateObject> replicationObjectDict = new Dictionary<string, ReplicateObject>();

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
            CreateRoomSendData data = new CreateRoomSendData();

            data.eventName = "CreateRoom";
            data.roomOption = roomOption;

            string toJson = JsonUtility.ToJson(data);

            ws.Send(toJson);
        }

        public void JoinRoom(Room.RoomOption roomOption)
        {
            CreateRoomSendData data = new CreateRoomSendData();

            data.eventName = "JoinRoom";
            data.roomOption = roomOption;

            string toJson = JsonUtility.ToJson(data);

            ws.Send(toJson);
        }

        public void LeaveRoom()
        {
            ServerSendData data = new ServerSendData(); ;

            data.eventName = "LeaveRoom";

            string toJson = JsonUtility.ToJson(data);

            ws.Send(toJson);
        }

        public void Login(LoginOption loginOption)
        {
            LoginSendData data = new LoginSendData();
            data.eventName = "Login";
            data.loginOption = loginOption;

            string toJson = JsonUtility.ToJson(data);
            ws.Send(toJson);
        }

        public void Register(RegisterOption registerOption)
        {
            RegisterSendData data = new RegisterSendData();
            data.eventName = "Register";
            data.registerOption = registerOption;

            string toJson = JsonUtility.ToJson(data);
            ws.Send(toJson);
        }

        public void SpawnNetworkObject(string prefName, Vector3 position, Quaternion rotation)
        {
            ServerSendData data = new ServerSendData();
            data.eventName = "RequestUIDObject";

            string toJson = JsonUtility.ToJson(data);
            ws.Send(toJson);

            if(tempSpawnNetworkObj == null)
            {
                tempSpawnNetworkObj = new ReplicateObject();
            }

            tempSpawnNetworkObj.prefName = prefName;
            tempSpawnNetworkObj.SetPositionData(position);
            tempSpawnNetworkObj.SetRotationData(rotation);
        }

        public void DestroyNetworkObject(string objectID)
        {
            if(replicateSend.replicationObjectDict.ContainsKey(objectID))
            {
                replicateSend.replicationObjectDict[objectID].isMarkRemove = true;
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
                NotifyTextCallback(messageQueue[0]);
                messageQueue.RemoveAt(0);
            }

            if(binaryQueue.Count > 0)
            {
                NotifyBinaryCallback(binaryQueue[0]);
                binaryQueue.RemoveAt(0);
            }
        }

        private IEnumerator IEUpdateReplicateObject()
        {
            float sendPerSecond = 1 / intervalSendTime;

            while (true)
            {
                if (currentRoom != null)
                {
                    foreach (var replicationObj in replicateSend.replicationObjectDict)
                    {
                        if(replicationObj.Value.netObject != null && replicationObj.Value.isMarkRemove == false)
                        {
                            replicationObj.Value.SetPositionData(replicationObj.Value.netObject.transform.position);
                            replicationObj.Value.SetRotationData(replicationObj.Value.netObject.transform.rotation);
                        }
                        else if(replicationObj.Value.netObject != null && replicationObj.Value.isMarkRemove == true)
                        {
                            Destroy(replicationObj.Value.netObject.gameObject);
                        }
                    }

                    replicateSend.replicationObjectList = replicateSend.replicationObjectDict.Values.ToList();

                    //string toJson = JsonUtility.ToJson(replicateSend);

                    //Debug.Log(toJson);
                    ReplicateData();
                }

                yield return new WaitForSeconds(sendPerSecond);
            }

            
        }

        private void NotifyTextCallback(string callbackData)
        {
            //Debug.Log("OnMessage : " + callbackData);

            ServerSendData recieveEvent = JsonUtility.FromJson<ServerSendData>(callbackData);

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
                        //Internal_ReplicateData(callbackData);
                        break;
                    }
                default:
                    {
                        Internal_SpawnNetworkObject(recieveEvent.data);
                        break;
                    }
            }
        }

        private void NotifyBinaryCallback(byte[] bytes)
        {
            Internal_ReplicateData(bytes);
        }

        private void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            Debug.Log("IsText : " + messageEventArgs.IsText + "/ IsBin : "+messageEventArgs.IsBinary);

            if (messageEventArgs.IsText)
            {
                messageQueue.Add(messageEventArgs.Data);
            }
            else if(messageEventArgs.IsBinary)
            {
                
                binaryQueue.Add(messageEventArgs.RawData);
            }
            
        }

        private void ReplicateData()
        {
            /*ReplicateSendData data = new ReplicateSendData();
            data.eventName = "ReplicateData";
            data.replicateByteData = replicateSend.ToByteArr();

            //Debug.Log(currentRoom.roomOption.roomName);
            data.roomName = currentRoom.roomOption.roomName;

            string toJson = JsonUtility.ToJson(data);*/
            //Debug.Log("total send byte : " + System.Text.ASCIIEncoding.ASCII.GetByteCount(toJson));

            byte[] byteArr = replicateSend.ToByteArr();
            ws.Send(byteArr);
        }

        private void Internal_OnCreateRoom(string callbackData)
        {
            Debug.Log(callbackData);
            Room.RoomOption createRoomData = JsonUtility.FromJson<Room.RoomOption>(callbackData);
            Room newRoom = new Room();
            newRoom.roomOption = createRoomData;
            currentRoom = newRoom;

            Debug.Log(createRoomData.roomName);
        }

        private void Internal_OnJoinRoom(string callbackData)
        {
            Debug.Log(callbackData);
            CreateRoomSendData createRoomData = JsonUtility.FromJson<CreateRoomSendData>(callbackData);
            Room newRoom = new Room();
            newRoom.roomOption = createRoomData.roomOption;
            currentRoom = newRoom;
        }

        private void Internal_OnLeaveRoom()
        {
            currentRoom = null;
        }

        private void Internal_ReplicateData(byte[] bytes)
        {
            ReplicateList newReplicationList = new ReplicateList();
            newReplicationList = newReplicationList.FromByteArr(bytes);

            //ReplicateSendData replicateData = JsonUtility.FromJson<ReplicateSendData>(jsonStr);

            //if (OnReplicateData != null)
            //{
            //    OnReplicateData(replicateData.replicateData);
            //}
            
            ReplicateList toReplicateList = newReplicationList; //JsonUtility.FromJson<ReplicateList>(replicateData.replicateData);

            Debug.Log(toReplicateList.replicationObjectList.Count);

            for (int i = 0; i < toReplicateList.replicationObjectList.Count; i++)
            {
                ReplicateObject replicateObj = toReplicateList.replicationObjectList[i];
                bool isNewObject = true;
                bool isNotExist = true;
                string objectID = replicateObj.objectID;

                Debug.Log(objectID);

                if (replicationObjectDict.ContainsKey(objectID))
                {
                    if(replicateObj.isMarkRemove == false)
                    {
                        replicationObjectDict[objectID].SetPositionData(replicateObj.GetPositionData());
                        replicationObjectDict[objectID].SetRotationData(replicateObj.GetRotationData());

                        if(replicationObjectDict[objectID].netObject != null)
                        {
                            replicationObjectDict[objectID].netObject.position = replicationObjectDict[objectID].GetPositionData();
                            replicationObjectDict[objectID].netObject.rotation = replicationObjectDict[objectID].GetRotationData();
                        }
                        else if(replicateObj.isMarkRemove == true && replicateObj.netObject != null)
                        {
                            Destroy(replicationObjectDict[objectID].netObject.gameObject);
                            replicationObjectDict.Remove(objectID);
                            break;
                        }

                        isNewObject = false;
                    }
                }

                if(replicateSend.replicationObjectDict.ContainsKey(objectID))
                {
                    isNotExist = false;

                    if (replicateObj.isMarkRemove)
                    {
                        replicateSend.replicationObjectDict.Remove(objectID);
                        break;
                    }
                }

                if (isNewObject && isNotExist && replicateObj.isMarkRemove == false)
                {
                    GameObject newGameObject = Instantiate(Resources.Load(replicateObj.prefName)) as GameObject;
                    Debug.LogError("CreateNetObject");
                    NetObject newNetObject = newGameObject.GetComponent<NetObject>();
                    newNetObject.ownerID = replicateObj.ownerID;
                    newNetObject.objectID = replicateObj.objectID;
                    newNetObject.position = replicateObj.GetPositionData();
                    newNetObject.rotation = replicateObj.GetRotationData();
                    newNetObject.transform.position = replicateObj.GetPositionData();
                    newNetObject.transform.rotation = replicateObj.GetRotationData();
                    replicateObj.netObject = newNetObject;
                    replicationObjectDict.Add(objectID, replicateObj);
                }
            }
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
            newReplicateObject.SetPositionData(tempSpawnNetworkObj.GetPositionData());
            newReplicateObject.SetRotationData(tempSpawnNetworkObj.GetRotationData());

            GameObject newGameObject = Instantiate(Resources.Load(newReplicateObject.prefName)) as GameObject;
            NetObject newNetObject = newGameObject.GetComponent<NetObject>();
            newNetObject.ownerID = clientID;
            newNetObject.objectID = uid;
            newNetObject.position = newReplicateObject.GetPositionData();
            newNetObject.rotation = newReplicateObject.GetRotationData();
            newNetObject.transform.position = newReplicateObject.GetPositionData();
            newNetObject.transform.rotation = newReplicateObject.GetRotationData();
            newReplicateObject.netObject = newNetObject;

            replicateSend.replicationObjectDict.Add(uid, newReplicateObject);

            replicateSend.replicationObjectList.Add(newReplicateObject);

            tempSpawnNetworkObj.prefName = "";
        }
    }
}


