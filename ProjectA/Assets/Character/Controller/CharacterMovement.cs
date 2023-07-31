using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement
{
    PlayerController controller;
    public CharacterMovement(PlayerController controller)
    {
        this.controller = controller;
    }

    public Vector3 BasicMove(float verti, float horizon, float moveSpeed)
    {
        Vector3 r = Vector3.zero;
        Vector2 vertical = new Vector2(PlayerController.CameraToPlayerVector.x,PlayerController.CameraToPlayerVector.z);
        Vector2 horizontal = new Vector2(vertical.y, vertical.x);

        Vector2 movementDir = (vertical * verti + horizontal * horizon).normalized * moveSpeed;
        r += new Vector3(movementDir.x, 0, movementDir.y);

        return r;
    }
}
