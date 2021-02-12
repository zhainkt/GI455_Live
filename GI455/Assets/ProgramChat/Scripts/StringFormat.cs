using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringFormat : MonoBehaviour
{
    class MessageData
    {
        public string username;
        public string message;
        public string color;
    }

    // Start is called before the first frame update
    void Start()
    {
        //====================== String to Class
        string messageFromServer = "inwza#hello world#green";

        string[] strSplit = messageFromServer.Split('#');

        MessageData messageData = new MessageData();
        messageData.username = strSplit[0];
        messageData.message = strSplit[1];
        messageData.color = strSplit[2];
        //======================================


        //====================== Class to String
        string strToSend = messageData.username + "#" + messageData.message + "#" + messageData.color;
        //"inwza#hello world#green"
        //======================================
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
