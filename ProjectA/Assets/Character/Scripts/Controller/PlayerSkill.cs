using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill
{
    PlayerController controller;

    public PlayerSkill(PlayerController controller)
    {
        this.controller = controller;
    }

    public static Vector3 Slide(Transform pc, float distance)
    {
        Vector3 t_endpoint;
        Vector3 t_dir = pc.forward - new Vector3(0, pc.forward.y, 0);

        Ray ray = new Ray(pc.position, t_dir);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);

        if(hit.collider != null)
        {
            t_endpoint = hit.point;
        }
        else
        {
            t_endpoint = pc.position + t_dir.normalized * distance;
        }

        return t_endpoint;
    }

    public IEnumerator SlideCo(Animator animator, float speed, Vector3 endPoint)
    {
        CharacterController pc = controller.GetComponent<CharacterController>();
        animator.SetTrigger("Slide00");
        
        while (Vector3.Distance(controller.transform.position, endPoint) > 10f)
        {
            pc.Move((endPoint - controller.transform.position).normalized * speed * Time.deltaTime);
            yield return null;
        }

        animator.SetTrigger("Slide01");
    }
}
