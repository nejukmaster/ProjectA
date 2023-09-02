using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : NetworkBehaviour
{
    PlayerController controller => GetComponent<PlayerController>();
    public void BasicAttack()
    {
        if(!IsServer) return;
        GameObject target = null;
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("Enemy"))
            if (target == null || (Vector3.Distance(target.transform.position, transform.position) > Vector3.Distance(o.transform.position, transform.position) && Vector3.Distance(target.transform.position, transform.position) <= StatusManager.instance.GetStatus(controller.ID).GetStat("attackRange"))) target = o;
        if(target != null)
        {
            transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));
            target.GetComponent<MobController>().Damaged(1f);
        }
    }

    public void SlideAttack()
    {

    }
}
