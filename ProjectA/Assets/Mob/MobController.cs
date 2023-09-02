using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MobController : NetworkBehaviour
{
    public GameObject target;
    protected MobStatus status;
    protected Animator animator;
    protected NavMeshAgent agent;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        status = GetComponent<MobStatus>();
    }

    private void Update()
    {
        if (IsServer)
        {
            Move();
        }
    }

    protected virtual void Move()
    {

    }

    public virtual void Damaged(float damage)
    {

    }

    [ServerRpc]
    public void DamagedServerRpc(float damage)
    {
        Damaged(damage);
    }
}
