using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MultiplayerExampleSec1
{
    [Serializable]
    public class NetworkDataOption
    {
        [Serializable]
        public class ReplicateObject
        {
            public string objectID;
            public string ownerID;
            public string prefName;
            public Vector3 position;
            public Quaternion rotation;

            public NetworkObject netObj;

        }

        [SerializeField]
        public class ReplicateObjectList
        {
            public List<ReplicateObject> replicateObjectList = new List<ReplicateObject>();

            public byte[] ToByteArr()
            {
                var binFormatter = new BinaryFormatter();
                var mStream = new MemoryStream();
                binFormatter.Serialize(mStream, this);
                return mStream.ToArray();
            }

            public ReplicateObjectList FromByteArr(byte[] byteArr)
            {
                var mStream = new MemoryStream();
                var binFormatter = new BinaryFormatter();

                mStream.Write(byteArr, 0, byteArr.Length);
                mStream.Position = 0;
                return binFormatter.Deserialize(mStream) as ReplicateObjectList;
            }
        }

        [Serializable]
        public class EventCallbackGeneral
        {
            public string eventName;
            public string data;
        }

        [Serializable]
        public class EventSendCreateRoom : EventCallbackGeneral
        {
            public Room.RoomOption roomOption;
        }

        [Serializable]
        public class EventSendReplicate : EventCallbackGeneral
        {
            public string roomName;
        }
    }

    public class Room
    {
        [Serializable]
        public class RoomOption
        {
            public string roomName;
        }

        public RoomOption roomOption;
    }
}