using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
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
            //this.transform.position += Vector3.up * 10.0f * Time.deltaTime;
            this.transform.position = Vector3.MoveTowards(this.transform.position, Vector3.up * 2.5f, 10.0f * Time.deltaTime);
        }
        else
        {
            this.transform.position = netObj.replicateData.position;
        }
    }
}
