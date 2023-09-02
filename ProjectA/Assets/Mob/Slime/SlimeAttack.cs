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
        if (!IsServer || controller.target == null) return;
        PlayerController pc = controller.target.GetComponent<PlayerController>();
        {
            pc.Damaged(2);
        }
    }
}
