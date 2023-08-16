using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class SlimeController : NetworkBehaviour
{
    public GameObject target;

    [SerializeField] float detectionRange;
    [SerializeField] float attackRange;
    Animator animator;
    NavMeshAgent agent;
    public void Start()
    {
        base.OnNetworkSpawn();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            target = players[0];
            for (int i = 0; i < players.Length; i++)
            {
                if (Vector3.Distance(players[i].transform.position, transform.position) < Vector3.Distance(target.transform.position, transform.position))
                {
                    target = players[i];
                }
            }
            float distanceToNearest = Vector3.Distance(target.transform.position, transform.position);
            if (distanceToNearest <= detectionRange && distanceToNearest > attackRange)
            {
                agent.SetDestination(target.transform.position);
            }
            else if(distanceToNearest <= attackRange)
            {
                agent.SetDestination(transform.position);
                animator.SetTrigger("Attack");
            }
            transform.LookAt(new Vector3(target.transform.position.x,transform.position.y,target.transform.position.z));
        }
        else
        {
            agent.SetDestination(transform.position);
        }
        animator.SetBool("Move", Vector3.Distance(transform.position, agent.destination) > attackRange);
    }
}
