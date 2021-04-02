using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerExampleSec2;

public class Movement_Sec2 : MonoBehaviour
{
    private NetworkObject_Sec2 netObj;

    private void Start()
    {
        netObj = GetComponent<NetworkObject_Sec2>();
    }

    void Update()
    {
        if(netObj.IsOwner())
        {
            this.transform.position += Vector3.right * 3 * Input.GetAxis("Horizontal") * Time.deltaTime;

            if(Input.GetKeyDown(KeyCode.Space))
            {
                SocketConnection_Sec2.Instance.SpawnNetworkObject("Bullet", this.transform.position, this.transform.rotation);
            }
        }
        else
        {
            //this.transform.position = netObj.replicateData.position;
            //this.transform.rotation = netObj.replicateData.rotation;
            this.transform.position = Vector3.Lerp(this.transform.position, netObj.replicateData.position, 3 * Time.deltaTime);
        }
    }
}
