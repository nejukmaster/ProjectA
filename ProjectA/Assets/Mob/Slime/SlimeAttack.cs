using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(SlimeController))]
public class SlimeAttack : NetworkBehaviour
{
    SlimeController controller;
    private void Awake()
    {
        controller = GetComponent<SlimeController>();
    }

    public void BasicAttack()
    {
        PlayerController pc = controller.target.GetComponent<PlayerController>();
        if (pc.IsOwner) {
            pc.DamagedServerRpc();
        }
    }
}
