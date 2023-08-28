using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStatus : NetworkBehaviour
{
    [ClientRpc]
    public void StatusChangeClientRpc(string status, float value)
    {
        HPBar.instance.SetHp(value);
    }
}
