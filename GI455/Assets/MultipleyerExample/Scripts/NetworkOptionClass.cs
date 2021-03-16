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
        private float[] position = new float[3];
        private float[] rotation = new float[4];

        [NonSerialized]
        public NetObject netObject;
        
        public void SetPositionData(Vector3 toSet)
        {
            position[0] = float.Parse(toSet.x.ToString("0.0"));
            position[1] = float.Parse(toSet.y.ToString("0.0"));
            position[2] = float.Parse(toSet.z.ToString("0.0"));
        }

        public void SetRotationData(Quaternion toSet)
        {
            rotation[0] = float.Parse(toSet.x.ToString("0.0"));
            rotation[1] = float.Parse(toSet.y.ToString("0.0"));
            rotation[2] = float.Parse(toSet.z.ToString("0.0"));
            rotation[2] = float.Parse(toSet.w.ToString("0.0"));
        }

        public Vector3 GetPositionData()
        {
            return new Vector3(position[0], position[1], position[2]);
        }

        public Quaternion GetRotationData()
        {
            return new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
        }
    }

    [Serializable]
    public class ReplicateList
    {
        public List<ReplicateObject> replicationObjectList = new List<ReplicateObject>();

        [NonSerialized]
        public Dictionary<string, ReplicateObject> replicationObjectDict = new Dictionary<string, ReplicateObject>();

        public byte[] ToByteArr()
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
        }
    }
}