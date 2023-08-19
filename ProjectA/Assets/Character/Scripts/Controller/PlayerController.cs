using UnityEngine;
using Unity.Netcode;
using UnityEditor;
using System;

public class PlayerController : NetworkBehaviour
{
    public static Vector3 CameraToPlayerVector;

    public Vector3 characterTrackingPoint;
    
    public float gravity = -9.8f;
    [SerializeField] float walkSpeed;
    [SerializeField] float jumpIntensity;
    [SerializeField] float runSpeed;
    [SerializeField] Animator animator;

    public CharacterController controller;
    Camera cam;
    CharacterMovement movement;
    float ySpeed = 0.0f;
    float invincibility;
    bool canMove = true;
    bool onGround = true;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsServer) controller = GetComponent<CharacterController>();
        if (!IsOwner) return;
        cam = Camera.main;
        cam.GetComponent<CameraController>().SetTrackingObj(this);
        movement = new CharacterMovement(this);
    }

    private void Update()
    {
        if(!IsOwner) return;
        Debug.Log("tlfgod");
        if (invincibility > 0) invincibility -= Time.deltaTime;
        if (!Application.isFocused) return;
        if(canMove) MovePlayerServer();
    }

    void MovePlayerServer()
    {
        Vector3 moveDir = Vector3.zero;
        Quaternion rotateDir = this.transform.rotation;
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
            rotateDir = Quaternion.LookRotation(new Vector3(moveDir.x,0,moveDir.z));

            if(Input.GetKeyDown(KeyCode.Space) && onGround)
            {
                ySpeed = jumpIntensity;
                JumpPlayerServerRpc();
                onGround = false;
            }
        }
        moveDir.y = ySpeed;
        MovePlayerServerRpc(rotateDir, moveDir, Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        GameObject go = hit.gameObject;
        if (go.CompareTag("Ground"))
        {
            if (hit.moveDirection == new Vector3(0, -1, 0) && !onGround)
            {
                onGround = true;
            }
        }
    }

    [ServerRpc]
    void MovePlayerServerRpc(Quaternion rotateDir, Vector3 p_moveDir, float p_deltaTime)
    {
        Vector3 moveDir = new Vector3((p_moveDir * p_deltaTime).x, p_moveDir.y, (p_moveDir * p_deltaTime).z);
        this.transform.rotation = rotateDir;
        controller.Move(moveDir);
        animator.SetFloat("Speed", new Vector3(p_moveDir.x,0,p_moveDir.z).magnitude);
    }
    [ServerRpc]
    void JumpPlayerServerRpc()
    {
        animator.SetTrigger("Jump");
    }
    [ServerRpc]
    public void DamagedServerRpc()
    {
        if (invincibility <= 0)
        {
            animator.SetTrigger("Damage");
        }
    }

    public void SetCanMove(int p_int)
    {
        this.canMove = p_int == 1;
    }

    public void Invincible(float p_float)
    {
        invincibility = p_float;
    }

}

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    private void OnSceneGUI()
    {
        PlayerController controller = (PlayerController)target;

        EditorGUI.BeginChangeCheck();
        Handles.color = Color.magenta;
        Vector3 trackingOffset = Handles.FreeMoveHandle(controller.transform.position + controller.characterTrackingPoint, 2f, Vector3.zero, Handles.CylinderHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            controller.characterTrackingPoint = trackingOffset - controller.transform.position;
        }
    }
}
