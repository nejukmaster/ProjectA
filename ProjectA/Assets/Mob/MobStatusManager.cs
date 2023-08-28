using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MobStatusManager : NetworkBehaviour
{
    protected Status status;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        status = InitializeStatus();
    }

    protected virtual Status InitializeStatus()
    {
        return null;
    }
}
