using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MultiplayerExampleSec2
{
    public class NetworkDataOption
    {
        [Serializable]
        public class EventCallbackGeneral
        {
            public string eventName;
            public bool status;
            public string data;
        }

        [Serializable]
        public class EventSendCreateRoom : EventCallbackGeneral
        {
            public Room.RoomOption roomOption;
        }

        [Serializable]
        public class EventSendReplicateData : EventCallbackGeneral
        {
            public string roomName;
        }

        [Serializable]
        public class ReplicateObject
        {
            public string objectID;
            public string ownerID;
            public string prefName;
            public Vector3 position;
            public Quaternion rotation;

            public NetworkObject_Sec2 netObj;
        }
        
        [Serializable]
        public class ReplicateObjectList
        {
            public List<ReplicateObject> replicateObjectList = new List<ReplicateObject>();

            /*public byte[] ToByteArr()
            {
                var binFormatter = new BinaryFormatter();
                var mStream = new MemoryStream();
                binFormatter.Serialize(mStream, this);
                return mStream.ToArray();
            }

            public static ReplicateObjectList FromByteArr(byte[] byteArr)
            {
                var mStream = new MemoryStream();
                var binFormatter = new BinaryFormatter();

                mStream.Write(byteArr, 0, byteArr.Length);
                mStream.Position = 0;
                return binFormatter.Deserialize(mStream) as ReplicateObjectList;
            }*/
        }
    }

    public class Room
    {
        [Serializable]
        public class RoomOption
        {
            public string roomName;
            public string mapName;
        }

        public RoomOption roomOption;
    }
}
