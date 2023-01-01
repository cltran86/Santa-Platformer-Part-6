using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnoombaEnemy : AbstractEnemy
{
    protected override void UpdateMovement()
    {
        // Flip the gameobject to face the correct direction
        transform.localScale = new Vector3(currentDirection, 1, 1);

        // set current speed via clamp
        currentSpeed = Mathf.Clamp(currentSpeed + acceleration * currentDirection, -maxSpeed, maxSpeed);

        // apply speed to the rb velocity
        _rb.velocity = new Vector2(currentSpeed, _rb.velocity.y);
    }

    protected override void CheckGround()
    {
        // Check if there is ground underneath from the front
        bool hit = Physics2D.Raycast(transform.position + colliderOffset * currentDirection, Vector3.down, groundDistance, groundLayer)
            || Physics2D.Raycast(transform.position + colliderOffset * currentDirection, Vector3.down, groundDistance, destructableLayer);
        
        // Check if there is ground underneath from the back
        bool hitBack = Physics2D.Raycast(transform.position + colliderOffset * -currentDirection, Vector3.down, groundDistance, groundLayer)
            || Physics2D.Raycast(transform.position + colliderOffset * -currentDirection, Vector3.down, groundDistance, destructableLayer);

        // Change the direction we are facing
        if (!hit && hitBack) currentDirection = -currentDirection;
    }
}
