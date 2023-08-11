using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static Vector3 CameraToPlayerVector;

    public float gravity = -9.8f;
    [SerializeField] float walkSpeed;
    [SerializeField] float jumpIntensity;
    [SerializeField] float runSpeed;
    [SerializeField] Animator animator;

    CharacterController controller;
    Camera cam;
    CharacterMovement movement;
    float ySpeed = 0.0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main;
        movement = new CharacterMovement(this);
    }

    void Update()
    {
        Vector3 moveDir = Vector3.zero;
        ySpeed += gravity*Time.deltaTime;
        if (Input.anyKey)
        {
            CameraToPlayerVector = Camera.main.transform.forward;
            
            float horizon = Input.GetAxis("Horizontal");
            float verti = Input.GetAxis("Vertical");

            if (Input.GetKey(KeyCode.LeftShift))
                moveDir += movement.BasicMove(verti, horizon, runSpeed);
            else
                moveDir += movement.BasicMove(verti, horizon, walkSpeed);
            this.transform.rotation = Quaternion.LookRotation(new Vector3(moveDir.x,0,moveDir.z));

            if(Input.GetKeyDown(KeyCode.Space))
            {
                ySpeed = jumpIntensity;
                animator.SetTrigger("Jump");
            }
        }
        animator.SetFloat("Speed", moveDir.magnitude);
        moveDir.y = ySpeed;
        controller.Move(new Vector3((moveDir * Time.deltaTime).x,moveDir.y, (moveDir * Time.deltaTime).z));
    }

}
