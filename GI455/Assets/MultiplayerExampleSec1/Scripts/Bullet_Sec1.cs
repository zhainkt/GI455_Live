using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerExampleSec1;

public class Bullet_Sec1 : MonoBehaviour
{
    private NetworkObject netObj;
    private float countTime;

    // Start is called before the first frame update
    void Start()
    {
        netObj = GetComponent<NetworkObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if(netObj.IsOwner())
        {
            this.transform.position += Vector3.up * 10.0f * Time.deltaTime;

            countTime += Time.deltaTime;

            if(countTime > 2.0f)
            {
                SocketConnection_Sec1.instance.DestroyNetworkObject(netObj.replicateData.objectID);
            }
        }
        else
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, netObj.replicateData.position, 10.0f*Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Box" && netObj.IsOwner())
        {
            Debug.Log("Add Score");
            GameManager_Sec1.instance.AddScore(5);
            SocketConnection_Sec1.instance.DestroyNetworkObject(netObj.replicateData.objectID);
        }
    }
}
