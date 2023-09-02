using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MobStatus : NetworkBehaviour
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

    public int GetStatus(string s)
    {
        return status.GetStat(s);
    }

}
