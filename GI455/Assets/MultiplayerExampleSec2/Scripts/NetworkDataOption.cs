using System;
using System.Collections.Generic;
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
        public class ReplicateObject
        {
            public string objectID;
            public string ownerID;
            public string prefName;
            public Vector3 position;
        }
        
        [Serializable]
        public class ReplicateObjectList
        {
            public List<ReplicateObject> replicateObjectList = new List<ReplicateObject>();
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
