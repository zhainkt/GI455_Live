using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerExampleSec1;

public class MoveObject : MonoBehaviour
{
    private NetworkObject netObj;
    public Transform shootingPoint;

    void Start()
    {
        netObj = GetComponent<NetworkObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (netObj.IsOwner())
        {
            this.transform.position += Vector3.right * Input.GetAxis("Horizontal") * 3.0f * Time.deltaTime;

            if(Input.GetKeyDown(KeyCode.Space))
            {
                SocketConnection_Sec1.instance.SpawnNetworkObject("Bullet_1", shootingPoint.position, shootingPoint.rotation);
            }
        }
        else
        {
            this.transform.position = Vector3.Lerp(this.transform.position, netObj.replicateData.position, 5.0f * Time.deltaTime);
        }
    }
}
