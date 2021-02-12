using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System;
using UnityEngine.UI;

namespace ChatWebSocket
{
    public class WebSocketConnection : MonoBehaviour
    {

        struct MessageData
        {
            public string username;
            public string message;

            public MessageData(string username, string message)
            {
                this.username = username;
                this.message = message;
            }
        }

        public GameObject rootConnection;
        public GameObject rootMessenger;

        public InputField inputUsername;
        public InputField inputText;
        public Text sendText;
        public Text receiveText;
        
        private WebSocket ws;

        private string tempMessageString;

        public void Start()
        {
            //Set default is connection UI;
            rootConnection.SetActive(true);
            rootMessenger.SetActive(false);
        }

        private void Update()
        {
            UpdateNotifyMessage();
        }

        public void Connect()
        {
            string url = "ws://127.0.0.1:8080/";

            ws = new WebSocket(url);

            ws.OnMessage += OnMessage;

            ws.Connect();

            //Change UI to messenger after connected.
            rootConnection.SetActive(false);
            rootMessenger.SetActive(true);
        }

        public void Disconnect()
        {
            if (ws != null)
                ws.Close();
        }
        
        public void SendMessage()
        {
            if (string.IsNullOrEmpty(inputText.text) || ws.ReadyState != WebSocketState.Open)
                return;

            MessageData messageData = new MessageData(inputUsername.text,
                                                        inputText.text);

            string toJsonStr = JsonUtility.ToJson(messageData);
            
            ws.Send(toJsonStr);
            inputText.text = "";
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void UpdateNotifyMessage()
        {
            if (string.IsNullOrEmpty(tempMessageString) == false)
            {
                MessageData receiveMessageData = JsonUtility.FromJson<MessageData>(tempMessageString);

                if (receiveMessageData.username == inputUsername.text)
                {
                    sendText.text += "<color=red>" + receiveMessageData.username + "</color> : " + receiveMessageData.message + "\n";
                    receiveText.text += "\n";
                }
                else
                {
                    sendText.text += "\n";
                    receiveText.text += receiveMessageData.username + " : " + receiveMessageData.message + "\n";
                }

                tempMessageString = "";
            }
        }

        private void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            Debug.Log(messageEventArgs.Data);

            tempMessageString = messageEventArgs.Data;
        }
    }
}


