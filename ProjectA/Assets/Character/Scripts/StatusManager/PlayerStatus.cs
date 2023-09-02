using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStatus : NetworkBehaviour
{
    [ClientRpc]
    public void HPChangeClientRpc(int hp, int max_hp)
    {
        HPBar.instance.SetHp((float)hp/(float)max_hp);
    }
}
