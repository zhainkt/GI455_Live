using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplyerExample;

public class SimpleMove : MonoBehaviour
{
    private NetObject netObject;
    
    void Start()
    {
        netObject = GetComponent<NetObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (netObject.ownerID == SocketConnection.instance.clientID)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                this.transform.position -= Vector3.right * 5.0f * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                this.transform.position += Vector3.right * 5.0f * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                this.transform.position += Vector3.up * 5.0f * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                this.transform.position -= Vector3.up * 5.0f * Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                netObject.Destroy();
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                SocketConnection.instance.SpawnNetworkObject("Bullet", this.transform.position, this.transform.rotation);
            }
        }
    }
}
