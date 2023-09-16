Character Controller
====================
This chapter describes the process of creating C# scripts to move characters. The basic character movement uses Unity's built-in script, the Character Controller script, and this component is received from the Player Controller to implement the character's movement. 

### Network Settings

Before implementing the player's movements, first set up for multi-play using Unity Netcode. Create a Network Manager and register the Player you created before PlayerPrefab. 
![Alt text](/ExplainImgs/NetworkManagerSettings.png)
After that, we attach the NetworkTransform component and NetworkAnimator to the Player Prefab we created. NetworkTransform manages and synchronizes the transforms of that object on the server, and NetworkAnimator manages and synchronizes the animators of that object on the server.
![Alt text](/ExplainImgs/PlayerNetworkObjectSettings.png)
Okay, then It's ready.

### Character Controller Script
The Character Controller script is responsible for the actual movement of this character.I will explain the script while looking at the code.
```c#
public class PlayerController : NetworkBehaviour  //NetworkBehaviour inheritance
{
    public static Vector3 CameraToPlayerVector;  //Direction vector from the main camera to this character

    public Vector3 characterTrackingPoint;      //The coordinates of the part that the camera will follow in this character
    
    public float gravity = -9.8f;              //Character's Gravity
    [SerializeField] float walkSpeed;          //the walking speed of a character
    [SerializeField] float jumpIntensity;      //the strength of a jump
    [SerializeField] float runSpeed;          //running speed

    [SerializeField] Animator animator;        //Player's animator
    public CharacterController controller;    //Player's Character Controller

    Camera cam;                               //Main Camera
    CharacterMovement movement;               //Character Movement Object, which summarizes Character's motion operation
    float ySpeed = 0.0f;                      //The movement of the current character on the y-axis
    float invincibility;                      //Invincible time
    bool canMove = true;                      //Parameters for whether the character can be moved
    bool onGround = true;                    //Parameters to check if the character is touching the ground
```
The first thing to see is that he inherited NetworkBehaviour, not MonoBehaviour. Because Network Behaviour enables Unity to use network-related properties or methods.

CameraToPlayerVector and character tracking points are both variables to be used for camera tracking and will be used to determine the direction of each camera and where the camera will look. Each of the Serialized variables under it is a property that can arbitrarily change the player's movement. Animator and controller are the variables to import each of the Player's Animator and Character Controller components, and the private variables below are the variables to be used in the controller's system.
```c#
 public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();
    //When the local player is a server
    if(isServer) controller = GetComponent<CharacterController>();    //Gets the character controller.
    if (!IsOwner) return;                            //If the local player owns this script
    cam = Camera.main;                                                        //Initialize the main camera
    cam.GetComponent<CameraController>().SetTrackingObj(this);                //Set the camera to track this player object
    movement = new CharacterMovement(this);                                   //Character Movement object initialization
}
```
OnNetworkSpawn is invoked on each NetworkBehaviour associated with a NetworkObject spawned. So, it is usually used for initialization, and as shown in the code, it is possible to initialize the server and the client separately using NetworkBehaviour's properties. Here, you can see that only the Character Controller responsible for the character's movement is initialized on the server, and the rest of the camera-related and Character Movement objects are initialized on the client.
```c#
private void Update()
{
    if(!IsOwner) return;                //Run this character's owner only
    if (invincibility > 0) invincibility -= Time.deltaTime;    //Invincible Time Countdown
    if (!Application.isFocused) return;    //Run only when this window is selected
    if(canMove) MovePlayerServer();        //Move the Player
}
```
This is actually the Update Block that will work on the client side. Use isOwner to verify ownership so that it cannot work on the server or other clients, and if there is no problem, send a character-moving packet to the server. The function that does this is the MovePlayerServer function at the bottom. Next, we will look at this MovePlayerServer function.
```c#
void MovePlayerServer()
{
    Vector3 moveDir = Vector3.zero;                            //Local variable that stores the vector that the character will eventually move to
    Quaternion rotateDir = this.transform.rotation;            //Save Player Rotation Quarterion
    ySpeed += gravity*Time.deltaTime;                          //Gravity application
    if (Input.anyKey)    //If keystrokes are detected
    {
        CameraToPlayerVector = Camera.main.transform.forward; //Gets the vector the camera is looking at.
            
        float horizon = Input.GetAxis("Horizontal");         //Receive Horizontal Key Input
        float verti = Input.GetAxis("Vertical");             //Receive vertical axis key input

        if (Input.GetKey(KeyCode.LeftShift))                //running key
            moveDir += movement.BasicMove(verti, horizon, runSpeed);    //Apply runSpeed
        else
            moveDir += movement.BasicMove(verti, horizon, walkSpeed);    //Apply walkSpeed if you are not pressing the Run key
        rotateDir = Quaternion.LookRotation(new Vector3(moveDir.x,0,moveDir.z));    //Character rotation. No longitudinal rotation.

        if(Input.GetKeyDown(KeyCode.Space) && onGround)    //jump key
        {
            ySpeed = jumpIntensity;                        //Set y-axis speed to jump strength
            JumpPlayerServerRpc();                         //Send player's jump data in packets
            onGround = false;                             //Player fell off the ground
        }
    }
    moveDir.y = ySpeed;                                    //Apply y-axis motion
    MovePlayerServerRpc(rotateDir, moveDir * Time.deltaTime, new Vector3(moveDir.x,0,moveDir.z).magnitude);    //Send calculated player movement data to the server in packets
}
```
What should be noted here is that this data is delivered to the server through ServerRpc after completing the computation of the player's. This is because what actually moves the player is handled by the server.
```c#
[ServerRpc]            //Server Rpc Attribute. Can only call in client
void MovePlayerServerRpc(Quaternion rotateDir, Vector3 p_moveDir, float p_deltaTime)
{
    this.transform.rotation = rotateDir;                          //Apply rotation
    controller.Move(p_moveDir);                                   //Apply Move
    animator.SetFloat("Speed", speed);                            //Pass the speed value to the animator.
}
```
ServerRpc is invoked from the client and runs on the server. Therefore, the character can be moved through the Character Controller initialized in server. And it also applies the rotation value of the character.

### Final

As a result, This is the movement of the character implemented.

[![Video Label](https://youtu.be/BLV-J57aiv4/0.jpg)](https://youtu.be/BLV-J57aiv4)
