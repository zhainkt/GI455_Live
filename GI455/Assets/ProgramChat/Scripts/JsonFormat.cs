using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JsonFormat : MonoBehaviour
{
    class MessageDataJson
    {
        public string username;
        public string message;
        public string color;
    }

    struct Point
    {
        public int x, y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public Text text; 

    // Start is called before the first frame update
    void Start()
    {
        //====================== String to Class
        //string jsonStr =  {"username":"inwza007","message":"hello","color":"green"}
        //                  {"username":"inwza007","message":"hello world","color":"red"}
        //MessageDataJson messageDataJson = JsonUtility.FromJson<MessageDataJson>(jsonStr);
        //Debug.Log(messageDataJson.username);
        //Debug.Log(messageDataJson.message);
        //Debug.Log(messageDataJson.color);
        //======================================

        //====================== Class to String
        //MessageDataJson newMessageDataToSend = new MessageDataJson();
        //newMessageDataToSend.username = "inwza007";
        //newMessageDataToSend.message = "hello world";
        //newMessageDataToSend.color = "red";

        //string toJsonStr = JsonUtility.ToJson(newMessageDataToSend);
        //Debug.Log(toJsonStr);
        //======================================

        Point a = new Point(10, 10);
        Point b = a;
        a.x = 100;
        Debug.Log(b.x);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
