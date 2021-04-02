using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
    public class ServerSendData
    {
        public string eventName;
        public bool status;
        public string data;
    }

    [Serializable]
    public class CreateRoomSendData : ServerSendData
    {
        public Room.RoomOption roomOption;
    }

    [Serializable]
    public class LoginSendData : ServerSendData
    {
        public LoginOption loginOption;
    }

    [Serializable]
    public class RegisterSendData : ServerSendData
    {
        public RegisterOption registerOption;
    }

    [Serializable]
    public class ReplicateSendData : ServerSendData
    {
        public string roomName;
        public string replicateData;
        public byte[] replicateByteData;
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

        [NonSerialized]
        public NetObject netObject;
    }

    [Serializable]
    public class ReplicateList
    {
        public List<ReplicateObject> replicationObjectList = new List<ReplicateObject>();

        [NonSerialized]
        public Dictionary<string, ReplicateObject> replicationObjectDict = new Dictionary<string, ReplicateObject>();

        /*public byte[] ToByteArr()
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, this);
            return mStream.ToArray();
        }

        public ReplicateList FromByteArr(byte[] byteArr)
        {
            var mStream = new MemoryStream();
            var binFormatter = new BinaryFormatter();

            mStream.Write(byteArr, 0, byteArr.Length);
            mStream.Position = 0;
            return binFormatter.Deserialize(mStream) as ReplicateList;
        }*/
    }
}