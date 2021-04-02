using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerExampleSec2;

public class NetworkObject_Sec2 : MonoBehaviour
{
    public NetworkDataOption.ReplicateObject replicateData;

    public bool IsOwner()
    {
        if (replicateData == null)
            return false;

        return replicateData.ownerID == SocketConnection_Sec2.Instance.ClientID;
    }
}
