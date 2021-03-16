using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerExampleSec1
{
    public class NetworkDataOption
    {
        [Serializable]
        public class ReplicateObject
        {
            public string objectID;
            public string ownerID;
            public string prefName;
            public Vector3 position;
        }

        [SerializeField]
        public class ReplicateObjectList
        {
            public List<ReplicateObject> replicateObjectList = new List<ReplicateObject>();
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