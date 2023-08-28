using UnityEngine;
using Unity.Netcode;
using UnityEditor;
using System;

public class PlayerController : NetworkBehaviour
{
    public static Vector3 CameraToPlayerVector;

    public Vector3 characterTrackingPoint;
    public ulong ID;
    
    public float gravity = -20f;
    [SerializeField] float walkSpeed;
    [SerializeField] float jumpIntensity;
    [SerializeField] float runSpeed;
    [SerializeField] Animator animator;
    [SerializeField] NetworkObject m_networkObject;

    CharacterController controller;
    Camera cam;
    CharacterMovement movement;
    PlayerStatus playerStatus;
    float ySpeed = 0.0f;
    float invincibility;
    bool canMove = true;
    bool onGround = true;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            controller = GetComponent<CharacterController>();
        }
        if (!IsOwner) return;
        cam = Camera.main;
        cam.GetComponent<CameraController>().SetTrackingObj(this);
        movement = new CharacterMovement(this);
        InitializeServerRpc(NetworkManager.Singleton.LocalClientId);
        LosePanel.instance.player = this;
        playerStatus = GetComponent<PlayerStatus>();
        StatusManager.instance.InstanceStatusServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if(IsServer)
        {
            if (invincibility > 0) invincibility -= Time.deltaTime;
        }
        if(!IsOwner) return;
        if (!Application.isFocused) return;
        if(canMove) MovePlayerServer();
    }

    void MovePlayerServer()
    {
        Vector3 moveDir = Vector3.zero;
        Quaternion rotateDir = this.transform.rotation;
        if (Input.anyKey)
        {
            CameraToPlayerVector = Camera.main.transform.forward;
            
            float horizon = Input.GetAxis("Horizontal");
            float verti = Input.GetAxis("Vertical");

            if (Input.GetKey(KeyCode.LeftShift))
                moveDir += movement.BasicMove(verti, horizon, runSpeed);
            else
                moveDir += movement.BasicMove(verti, horizon, walkSpeed);
            Vector3 lookRota = new Vector3(moveDir.x, 0, moveDir.z);
            if (lookRota != Vector3.zero)
                rotateDir = Quaternion.LookRotation(lookRota);

            if(Input.GetKeyDown(KeyCode.Space) && onGround)
            {
                ySpeed = jumpIntensity;
                JumpPlayerServerRpc();
                onGround = false;
                return;
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                AttackServerRpc();
                canMove = false;
                return;
            }
        }
        ySpeed += gravity * Time.deltaTime;
        moveDir.y = ySpeed;
        MovePlayerServerRpc(rotateDir, moveDir * Time.deltaTime, new Vector3(moveDir.x,0,moveDir.z).magnitude);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        GameObject go = hit.gameObject;
        if (go.CompareTag("Ground"))
        {
            if (hit.moveDirection == new Vector3(0, -1, 0))
            {
                LendClientRpc();
            }
        }
    }

    [ClientRpc]
    void LendClientRpc()
    {
        onGround = true;
    }

    [ClientRpc]
    void DeathClientRpc()
    {

        LosePanel.instance.SetVisible(true);
        cam.GetComponent<CameraController>().tracking = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    [ClientRpc]
    void RespawnCharacterClientRpc()
    {
        gameObject.SetActive(true);
        if (IsOwner)
        {
            LosePanel.instance.SetVisible(false);
            cam.GetComponent<CameraController>().tracking = true;
            Cursor.visible = false;
            Cursor.lockState= CursorLockMode.Locked;
            canMove = true;
            onGround = true;
        }
    }

    [ServerRpc]
    void InitializeServerRpc(ulong id)
    {
        ID = id;
    }

    [ServerRpc]
    void MovePlayerServerRpc(Quaternion rotateDir, Vector3 p_moveDir, float speed)
    {
        this.transform.rotation = rotateDir;
        controller.Move(p_moveDir);
        animator.SetFloat("Speed", speed);
    }
    [ServerRpc]
    void JumpPlayerServerRpc()
    {
        animator.SetTrigger("Jump");
    }
    [ServerRpc]
    public void DamagedServerRpc(ulong clientId, float p_float)
    {
        if (invincibility <= 0)
        {

            if (StatusManager.instance.GetStatus(ID).GetStat("hp") - p_float > 0)
            {
                animator.SetTrigger("Damage");
                StatusManager.instance.AddStatus(clientId, "hp", -p_float);
            }
            else
            {
                animator.SetTrigger("Death");
                StatusManager.instance.AddStatus(clientId, "hp", -p_float);
                DeathClientRpc();
            }
        }
    }
    [ServerRpc]
    public void AttackServerRpc()
    {
        animator.SetTrigger("Attack");
    }
    [ServerRpc]
    public void RespawnCharacterServerRpc()
    {
        transform.position = Vector3.zero;
        StatusManager.instance.ChangeStatus(ID, "hp", StatusManager.instance.GetStatus(ID).GetStat("max_hp"));
        gameObject.SetActive(true);
        RespawnCharacterClientRpc();
    }
    public void DespawnCharacter()
    {
        gameObject.SetActive(false);
    }

    public void SetCanMove(int p_int)
    {
        if(IsOwner)
            this.canMove = p_int == 1;
    }

    public void Invincible(float p_float)
    {
        if(IsServer)
            invincibility = p_float;
    }

    public void Damaged(float p_float)
    {
        if (IsOwner)
        {
            DamagedServerRpc(NetworkManager.Singleton.LocalClientId, p_float);
        }
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
