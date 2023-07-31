using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static Vector3 CameraToPlayerVector;

    public float gravity = -9.8f;
    [SerializeField] float playerSpeed;
    [SerializeField] float jumpIntensity;

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
        ySpeed += gravity;
        if (Input.anyKey)
        {
            CameraToPlayerVector = Camera.main.transform.forward;
            
            float horizon = Input.GetAxis("Horizontal");
            float verti = Input.GetAxis("Vertical");

            moveDir += movement.BasicMove(verti, horizon, playerSpeed);
            this.transform.rotation = Quaternion.LookRotation(new Vector3(moveDir.x,0,moveDir.z));

            if(Input.GetKeyDown(KeyCode.Space))
            {
                ySpeed = jumpIntensity;
            }
        }
        moveDir.y = ySpeed;
        controller.Move(moveDir * Time.deltaTime);
    }

}
