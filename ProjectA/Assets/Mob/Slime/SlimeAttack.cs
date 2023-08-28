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
        if (controller.target != null)
        {
            PlayerController pc = controller.target.GetComponent<PlayerController>();
            if (pc.IsOwner)
            {
                pc.Damaged(0.2f);
            }
        }
    }
}
