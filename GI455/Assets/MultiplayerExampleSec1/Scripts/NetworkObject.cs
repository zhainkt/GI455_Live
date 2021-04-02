using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerExampleSec1;

public class NetworkObject : MonoBehaviour
{
    public NetworkDataOption.ReplicateObject replicateData;

    public bool IsOwner()
    {
        if (replicateData == null)
            return false;

        return replicateData.ownerID == SocketConnection_Sec1.instance.ClientID;
    }
}
