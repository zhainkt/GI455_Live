using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace ChatWebSocket
{
    public class UIManager : MonoBehaviour
    {
        public enum UIRootType
        {
            Connection,
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

        public Button btnConnectToServer;
        public Button btnPopupOK;
        public Button btnSendMessage;

        public InputField inputFieldName;
        public InputField inputMessage;

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

            webSocket.OnConnectionSuccess += OnConnectionSuccess;
            webSocket.OnConnectionFail += OnConnectionFail;
            webSocket.OnReceive += OnReceiveMessage;

            uiRootDict.Add(UIRootType.Connection, uiRootConnection);
            uiRootDict.Add(UIRootType.Chat, uiRootChat);

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
            uiRootPopUp.SetActive(true);
            textPopUpMsg.text = msg;
        }

        private void OpenUIRoot(UIRootType uiRootType)
        {
            foreach (var uiRoot in uiRootDict)
            {
                uiRoot.Value.SetActive(false);
            }

            uiRootDict[uiRootType].SetActive(true);
        }

        private void OnConnectionSuccess(string msg)
        {
            OpenUIRoot(UIRootType.Chat);
        }

        private void SendMessageData(string message)
        {
            messageDataSend.name = username;
            messageDataSend.message = message;

            string convertToJson = JsonUtility.ToJson(messageDataSend);

            webSocket.Send(convertToJson);
        }

        private void OnConnectionFail(string msg)
        {
            ShowPopup(msg);
        }

        private void OnReceiveMessage(string msg)
        {
            Debug.Log("OnReceive : " + msg);

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
    }
}
