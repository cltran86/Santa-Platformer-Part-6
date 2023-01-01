using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractEnemy : MonoBehaviour
{
    protected Rigidbody2D _rb;

    [Header("Movement")]
    [SerializeField]
    protected float maxSpeed = 5;
    [SerializeField]
    protected float acceleration = 0.5f;
    protected float currentSpeed;
    protected float currentDirection = 1;

    [Header("Ground Detection")]
    [SerializeField]
    protected float groundDistance;
    [SerializeField]
    protected float wallDistance;
    [SerializeField]
    protected Vector3 colliderOffset;
    [SerializeField]
    protected LayerMask groundLayer;
    [SerializeField]
    protected LayerMask destructableLayer;
    protected bool onGround = false;

    protected bool isDead = false;

    protected abstract void UpdateMovement();
    protected abstract void CheckGround();

    // Start is called before the first frame update
    void Start()
    {
        currentDirection = transform.localScale.x;
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        UpdateMovement();
        CheckWall();
        CheckGround();
    }

    public void Defeat()
    {
        if (isDead) return;
        isDead = true;

        // Change the layer of the enemy to prevent any potential re-collisions
        gameObject.layer = LayerMask.NameToLayer("Default");

        // Disable capsule collider
        GetComponent<CapsuleCollider2D>().enabled = false;

        // "pop" the enemy upwards
        GetComponent<Rigidbody2D>().AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        // Flip the enemy upside down
        transform.localScale = new Vector3(transform.localScale.x, -1, 1);
    }

    public void CheckWall()
    {
        // Check if we hit a wall
        bool hit = Physics2D.Raycast(transform.position + colliderOffset * currentDirection, currentDirection == 1 ? Vector3.right : Vector3.left, wallDistance, groundLayer)
            || Physics2D.Raycast(transform.position + colliderOffset * currentDirection, currentDirection == 1 ? Vector3.right : Vector3.left, wallDistance, destructableLayer);

        // Change the direction we are facing
        if (hit) currentDirection = -currentDirection;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + colliderOffset, transform.position + colliderOffset + Vector3.down * groundDistance);
        Gizmos.DrawLine(transform.position - colliderOffset, transform.position - colliderOffset + Vector3.down * groundDistance);

        // check for walls
        Gizmos.DrawLine(transform.position + colliderOffset, transform.position + colliderOffset + Vector3.right * wallDistance);
        Gizmos.DrawLine(transform.position - colliderOffset, transform.position - colliderOffset + Vector3.left * wallDistance);
    }
}
