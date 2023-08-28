using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MobController : NetworkBehaviour
{
    public GameObject target;

    [SerializeField] protected float detectionRange;
    [SerializeField] protected float attackRange;
    protected Animator animator;
    protected NavMeshAgent agent;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Move();
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
