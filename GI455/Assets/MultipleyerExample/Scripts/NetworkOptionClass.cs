using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplyerExample
{
    [Serializable]
    public class LoginOption
    {
        public string username;
        public string password;
    }

    [Serializable]
    public class RegisterOption : LoginOption
    {
        public string playerName;
    }

    [Serializable]
    public class ServerData
    {
        public string eventName;
        public bool status;
        public string data;
    }

    [Serializable]
    public class CreateRoomData : ServerData
    {
        public Room.RoomOption roomOption;
    }

    [Serializable]
    public class LoginData : ServerData
    {
        public LoginOption loginOption;
    }

    [Serializable]
    public class RegisterData : ServerData
    {
        public RegisterOption registerOption;
    }

    [Serializable]
    public class ReplicateData : ServerData
    {
        public string replicateData;
    }

    [Serializable]
    public class ReplicateObject
    {
        public string ownerID;
        public string objectID;
        public bool isMarkRemove;
        public string prefName;
        public Vector3 position;
        public Quaternion rotation;

        //[NonSerialized]
        public NetObject netObject;
    }

    [Serializable]
    public class ReplicateList
    {
        public List<ReplicateObject> replicateObjectList = new List<ReplicateObject>();
    }
}