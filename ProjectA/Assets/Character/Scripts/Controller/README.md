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
```hlsl
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
