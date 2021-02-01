using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System;

namespace ChatWebSocket
{
    public class WebSocketConnection : MonoBehaviour
    {
        [System.Serializable]
        public struct RoomData
        {
            public string roomName;
        }

        [System.Serializable]
        public struct ServerCreateRoomEventData
        {
            public string Event;
            public RoomData Data;
        }

        

        private WebSocket ws;

        public delegate void DelegateHandler(string msg);

        public event DelegateHandler OnConnectionSuccess;
        public event DelegateHandler OnConnectionFail;
        public event DelegateHandler OnReceive;

        private bool isConnection;

        public void Connect(string ip, int port)
        {
            string url = $"ws://{ip}:{port}/";

            InternalConnect(url);
        }

        public void Connect()
        {
            //string url = "ws://gi455chatserver.et.r.appspot.com/";
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

                CreateRoom("RoomTest");
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
            var eventData = new ServerCreateRoomEventData();

            eventData.Event = "createRoom";
            eventData.Data.roomName = roomName;

            string toJson = JsonUtility.ToJson(eventData);
            Debug.Log(toJson);

            ws.Send(toJson);
        }

        public void Send(string data)
        {
            if (!IsConnected())
                return;

            ws.Send(data);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (ws != null)
                ws.Close();
        }

        private void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            if (OnReceive != null)
                OnReceive(messageEventArgs.Data);
        }
    }
}


