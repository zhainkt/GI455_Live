using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplyerExample;

public class NetObject : MonoBehaviour
{
    public string ownerID;
    public string objectID;

    public Vector3 position;
    public Quaternion rotation;

    public void Update()
    {
        if(ownerID != SocketConnection.instance.clientID)
        {
            this.transform.position = position;
            this.transform.rotation = rotation;
        }
    }

    public void Destroy(float duration = 0.0f)
    {
        if (ownerID == SocketConnection.instance.clientID)
        {
            StartCoroutine(IEDestroy(duration));
        }
    }

    private IEnumerator IEDestroy(float duration)
    {
        yield return new WaitForSeconds(duration);

        SocketConnection.instance.DestroyNetworkObject(objectID);
    }
}
