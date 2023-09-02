using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class SlimeController : MobController
{
    protected override void Move()
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
            if (distanceToNearest <= status.GetStatus("detect_range") && distanceToNearest > status.GetStatus("attack_range"))
            {
                agent.SetDestination(target.transform.position);
            }
            else if(distanceToNearest <= status.GetStatus("attack_range"))
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
        animator.SetBool("Move", Vector3.Distance(transform.position, agent.destination) > status.GetStatus("attack_range"));
    }

    public override void Damaged(float damage)
    {
        animator.SetTrigger("Damaged");
        
    }
}
