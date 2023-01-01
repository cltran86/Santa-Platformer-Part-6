using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopperEnemy : AbstractEnemy
{
    [Header("Frames")]
    [SerializeField]
    private Sprite groundSprite;
    [SerializeField]
    private Sprite airSprite;

    [Header("Jumping")]
    [SerializeField]
    private float jumpHeight = 3f;
    [SerializeField]
    private float jumpDelay = 0.2f;
    private bool willJump = false;

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
        onGround = Physics2D.Raycast(transform.position + colliderOffset, Vector3.down, groundDistance, groundLayer)
            || Physics2D.Raycast(transform.position - colliderOffset, Vector3.down, groundDistance, groundLayer)
            || Physics2D.Raycast(transform.position + colliderOffset, Vector3.down, groundDistance, destructableLayer)
            || Physics2D.Raycast(transform.position - colliderOffset, Vector3.down, groundDistance, destructableLayer);

        if (!onGround) willJump = false;

        if (onGround && !willJump)
        {
            willJump = true;
            StartCoroutine(DelayedJump());
        }
    }

    IEnumerator DelayedJump()
    {
        // We will be on the ground to start, so swap out the fram
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = groundSprite;

        // Wait for the designated time
        yield return new WaitForSeconds(jumpDelay);

        // swap out the frame and add a jump force to the game object
        sr.sprite = airSprite;
        float jumpForce = Mathf.Sqrt(jumpHeight * -2 * Physics2D.gravity.y * _rb.gravityScale) * _rb.mass;
        _rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
    }
}
