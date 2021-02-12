using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace ChatWebSocket_Room
{
    public class UIManager : MonoBehaviour
    {
        public enum UIRootType
        {
            Connection,
            Lobby,
            CreateRoom,
            JoinRoom,
            Chat
        }

        public struct MessageData
        {
            public string name;
            public string message;
            public string colorCode;
        }

        public GameObject uiRootConnection;
        public GameObject uiRootChat;
        public GameObject uiRootPopUp;
        public GameObject uiRootLobby;
        public GameObject uiRootCreateRoom;
        public GameObject uiRootJoinRoom;

        public Button btnConnectToServer;
        public Button btnPopupOK;
        public Button btnSendMessage;
        public Button btnSelectCreateRoom;
        public Button btnSelectJoinRoom;
        public Button btnCreateRoom;
        public Button btnJoinRoom;
        public Button btnLeaveRoom;

        public InputField inputFieldName;
        public InputField inputMessage;
        public InputField inputCreateRoomName;
        public InputField inputJoinRoomName;

        public Text textPopUpMsg;
        public Text textReceiveMsgOwner;
        public Text textReceiveMsgOther;

        private Dictionary<UIRootType, GameObject> uiRootDict = new Dictionary<UIRootType, GameObject>();

        private WebSocketConnection webSocket;

        private string username;
        private string receiveStrOwner;
        private string receiveStrOther;

        public int id;

        MessageData messageDataSend;

        // Start is called before the first frame update
        void Start()
        {
            webSocket = GetComponent<WebSocketConnection>();
            btnConnectToServer.onClick.AddListener(BTN_ConnectToServer);
            btnSendMessage.onClick.AddListener(BTN_SendMessage);
            btnPopupOK.onClick.AddListener(BTN_PopupOK);
            btnSelectCreateRoom.onClick.AddListener(BTN_SelectCreateRoom);
            btnSelectJoinRoom.onClick.AddListener(BTN_SelectJoinRoom);
            btnCreateRoom.onClick.AddListener(BTN_CreateRoom);
            btnJoinRoom.onClick.AddListener(BTN_JoinRoom);

            webSocket.OnConnectionSuccess += OnConnectionSuccess;
            webSocket.OnConnectionFail += OnConnectionFail;
            webSocket.OnReceiveMessage += OnReceiveMessage;
            webSocket.OnCreateRoom += OnCreateRoom;
            webSocket.OnJoinRoom += OnJoinRoom;
            webSocket.OnLeaveRoom += OnLeaveRoom;

            uiRootDict.Add(UIRootType.Connection, uiRootConnection);
            uiRootDict.Add(UIRootType.Chat, uiRootChat);
            uiRootDict.Add(UIRootType.Lobby, uiRootLobby);
            uiRootDict.Add(UIRootType.CreateRoom, uiRootCreateRoom);
            uiRootDict.Add(UIRootType.JoinRoom, uiRootJoinRoom);

            OpenUIRoot(UIRootType.Connection);

            messageDataSend = new MessageData();
        }

        private void Update()
        {
            if(receiveStrOther != textReceiveMsgOther.text)
            {
                textReceiveMsgOther.text = receiveStrOther;
            }

            if(receiveStrOwner != textReceiveMsgOwner.text)
            {
                textReceiveMsgOwner.text = receiveStrOwner;
            }

            if(Input.GetKeyDown(KeyCode.Return))
            {
                BTN_SendMessage();
            }
        }

        void BTN_ConnectToServer()
        {
            username = inputFieldName.text;
            var random = new System.Random();
            var color = String.Format("#{0:X6}", random.Next(0x1000000));
            messageDataSend.colorCode = color;
            webSocket.Connect();
        }

        void BTN_SelectCreateRoom()
        {
            OpenUIRoot(UIRootType.CreateRoom);
        }

        void BTN_SelectJoinRoom()
        {
            OpenUIRoot(UIRootType.JoinRoom);
        }

        void BTN_CreateRoom()
        {
            if (!webSocket.IsConnected())
                return;

            webSocket.CreateRoom(inputCreateRoomName.text);
        }

        void BTN_JoinRoom()
        {
            if (!webSocket.IsConnected())
                return;

            webSocket.JoinRoom(inputJoinRoomName.text);
        }

        void BTN_SendMessage()
        {
            if(!string.IsNullOrEmpty(inputMessage.text))
            {
                SendMessageData(inputMessage.text);
            }

            inputMessage.text = "";
        }

        void BTN_PopupOK()
        {
            uiRootPopUp.SetActive(false);
        }

        public void ShowPopup(string msg)
        {
            Debug.Log("Show popup");
            uiRootPopUp.SetActive(true);
            textPopUpMsg.text = msg;
        }

        private void OpenUIRoot(UIRootType uiRootType)
        {
            foreach (var uiRoot in uiRootDict)
            {
                uiRoot.Value.SetActive(false);
            }

            Debug.Log("OpenUIRoot : " + uiRootType);

            uiRootDict[uiRootType].SetActive(true);
        }

        private void SendMessageData(string message)
        {
            messageDataSend.name = username;
            messageDataSend.message = message;

            string convertToJson = JsonUtility.ToJson(messageDataSend);

            webSocket.Send(convertToJson);
        }

        private void OnConnectionSuccess(string msg)
        {
            OpenUIRoot(UIRootType.Lobby);
        }

        private void OnConnectionFail(string msg)
        {
            ShowPopup(msg);
        }

        private void OnReceiveMessage(string msg)
        {

            if (string.IsNullOrEmpty(msg))
                return;

            Debug.Log("OnReceiveMessage : " + msg);

            MessageData msgData = JsonUtility.FromJson<MessageData>(msg);

            if(msgData.name == username)
            {
                receiveStrOwner += "<color=#FFB000>"+ msgData.name + " :</color> " + msgData.message + "\n";
                receiveStrOther += "\n";
            }else
            {
                receiveStrOwner += "\n";
                receiveStrOther += "<color="+ msgData.colorCode + ">" + msgData.name + " :</color> " + msgData.message + "\n";
            }
        }

        private void OnCreateRoom(string msg)
        {
            if(msg == "success")
            {
                Debug.Log("Create room success.");
                OpenUIRoot(UIRootType.Chat);
            }
            else
            {
                Debug.Log("Create room fail.");
                ShowPopup(msg);
            }
        }

        private void OnJoinRoom(string msg)
        {
            if (msg == "success")
            {
                Debug.Log("Join room success.");
                OpenUIRoot(UIRootType.Chat);
            }
            else
            {
                ShowPopup(msg);
            }
        }

        private void OnLeaveRoom(string msg)
        {
            Debug.Log("Leave room success.");
            OpenUIRoot(UIRootType.Lobby);
        }
    }
}
