using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Animator _animator;
    private AfterImage _afterImage;
    private ScoreKeeper _sk;

    [Header("Movement")]
    [SerializeField]
    private float maxSpeed = 5;
    [SerializeField]
    private float acceleration = 0.5f;
    private float horizontalTilt;
    private float currentSpeed;

    [Header("Jumping")]
    [SerializeField]
    private float jumpHeight = 3;

    [Header("Ground Detection")]
    [SerializeField]
    private float groundDistance;
    [SerializeField]
    private Vector3 colliderOffset;
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private LayerMask destructableLayer;
    private bool onGround = false;

    [Header("Dashing")]
    [SerializeField]
    private Color dashColour = Color.red;
    [SerializeField]
    private bool canDash = false;
    [SerializeField]
    private float dashSpeed = 20;
    [SerializeField]
    private float dashTime = 0.1f;
    private bool isDashing = false;
    // for the direction, 1 is to the right, -1 is to the left
    private float currentDirection = 1;

    [Header("Butt Slam")]
    [SerializeField]
    private Color slamColour = Color.yellow;
    [SerializeField]
    private float slamSpeed = -20;
    private bool isSlamming = false;

    [Header("Death stuff")]
    [SerializeField]
    private float minY = -5; // If we go lower than this number, we die
    private bool isDead = false;


    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _afterImage = GetComponent<AfterImage>();
        _sk = GameObject.Find("ScoreKeeper").GetComponent<ScoreKeeper>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        if (transform.position.y < minY) OnDeath();

        horizontalTilt = Input.GetAxisRaw("Horizontal");

        if (horizontalTilt > 0) currentDirection = 1; // to the right
        else if (horizontalTilt < 0) currentDirection = -1; // to the left

        // input for various actions
        if (Input.GetButtonDown("Jump")) StartJump();
        if (Input.GetButtonUp("Jump")) CancelJump();
        if (Input.GetButtonDown("Fire1")) StartDash();
        if (Input.GetButtonDown("Fire2")) StartSlam();

        // update animation here for smoother transitions
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        UpdateMovement();
        CheckGround();
    }

    private void UpdateMovement()
    {
        if (!isDashing && !isSlamming)
        {
            // if the horizontal tilt is not zero, add to the speed
            if (horizontalTilt != 0)
            {
                // calculate the current speed based on tilt and acceleration
                // clamp the speed so that the speed never exceeds the maximum
                currentSpeed = Mathf.Clamp(currentSpeed + horizontalTilt * acceleration, -maxSpeed, maxSpeed);
            }
            // if the horizontal tilt is zero, slowly decrease to zero
            else
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0, acceleration);
            }

            // apply that speed the the rigid body as a velocity
            _rb.velocity = new Vector2(currentSpeed, _rb.velocity.y);
        }
        else if (isDashing)
        {
            _rb.velocity = new Vector2(dashSpeed * currentDirection, 0);
        }
        else if (isSlamming)
        {
            _rb.velocity = new Vector2(0, slamSpeed);
        }
    }

    private void UpdateAnimation()
    {
        // change the direction of the sprite based on current direction
        transform.localScale = new Vector2(currentDirection, 1);

        // change the animation state based on the booleans of the actions
        // states are:
        // 0 for idle
        // 1 for walking
        // 2 for jumping
        // 3 for dashing
        // 4 for butt slam
        int state = 0;
        if (isSlamming)
        {
            state = 4;
        }
        else if (isDashing)
        {
            state = 3;
        }
        else if (!onGround)
        {
            state = 2;
        }
        else if (horizontalTilt != 0)
        {
            state = 1;
        }

        _animator.SetInteger("state", state);
    }

    private void CheckGround()
    {
        onGround = Physics2D.Raycast(transform.position + colliderOffset, Vector3.down, groundDistance, groundLayer)
            || Physics2D.Raycast(transform.position - colliderOffset, Vector3.down, groundDistance, groundLayer)
            || Physics2D.Raycast(transform.position + colliderOffset, Vector3.down, groundDistance, destructableLayer)
            || Physics2D.Raycast(transform.position - colliderOffset, Vector3.down, groundDistance, destructableLayer);

        // if the player is performing a butt slam, check the raycasts if an object is hit and if it is destructable
        if (isSlamming)
        {
            RaycastHit2D x = Physics2D.Raycast(transform.position + colliderOffset, Vector3.down, groundDistance, destructableLayer);
            RaycastHit2D y = Physics2D.Raycast(transform.position - colliderOffset, Vector3.down, groundDistance, destructableLayer);
            if (x) TryDestroyPlatform(x.transform.gameObject);
            if (y) TryDestroyPlatform(y.transform.gameObject);
        }

        if (!isDashing && onGround) canDash = true;
        if (onGround)
        {
            isSlamming = false;
            
            // if we arent dashing, turn off the after image
            if (!isDashing) _afterImage.StopAfterImage();
        }
    }

    private void StartJump()
    {
        if (!onGround) return;

        // calculate the force required
        float jumpForce = Mathf.Sqrt(jumpHeight * -2 * Physics2D.gravity.y * _rb.gravityScale) * _rb.mass;

        // apply the force onto the rigid body
        _rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
    }

    private void CancelJump()
    {
        if (_rb.velocity.y > 0)
        {
            Vector3 v = _rb.velocity;
            v.y = 0;
            _rb.velocity = v;
        }
    }

    private void StartDash()
    {
        // check if we are able to dash
        if (!canDash) return;

        // ensure that we cannot double dash by turning canDash = false;
        canDash = false;

        // Set the bool isDashing to true
        isDashing = true;

        // Start the after image
        _afterImage.SetColour(dashColour);
        _afterImage.StartAfterImage();

        // start a coroutine to make sure the dash is disabled after the alotted time
        StartCoroutine(TimeDash());
    }

    IEnumerator TimeDash()
    {
        yield return new WaitForSeconds(dashTime);
        isDashing = false;

        // Stop the after image
        _afterImage.StopAfterImage();
    }

    private void StartSlam()
    {
        // check if we are not on the ground
        if (onGround) return;

        // set is slamming to true
        isSlamming = true;

        // cancel out the dashing if we are current dashing
        isDashing = false;

        // Start the after image
        _afterImage.SetColour(slamColour);
        _afterImage.StartAfterImage();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + colliderOffset, transform.position + colliderOffset + Vector3.down * groundDistance);
        Gizmos.DrawLine(transform.position - colliderOffset, transform.position - colliderOffset + Vector3.down * groundDistance);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Destructable") && isDashing)
        {
            // destroy the object with a new function
            TryDestroyPlatform(collision.gameObject);
        } else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (isSlamming || isDashing)
            {
                collision.gameObject.GetComponent<AbstractEnemy>().Defeat();
            }
            else
            {
                OnDeath();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Collectable"))
        {
            GameObject g = collision.gameObject;

            // Add to score
            _sk.AddScore(1);
            Debug.Log("Score is " + _sk.Score);

            // Make sprite invisible and disable collider
            g.GetComponent<CircleCollider2D>().enabled = false;
            g.GetComponent<SpriteRenderer>().enabled = false;

            // Play an animation
            g.GetComponent<ParticleSystem>().Play();

            // move the object forward for layering
            g.transform.position = new Vector3(g.transform.position.x, g.transform.position.y, -0.05f);
        } else if (collision.gameObject.layer == LayerMask.NameToLayer("Goal"))
        {
            collision.GetComponent<Goal>().CompleteLevel();
        }
    }

    private void TryDestroyPlatform(GameObject g)
    {
        // play some sort of animation
        g.GetComponent<BoxCollider2D>().enabled = false;
        g.GetComponent<SpriteRenderer>().enabled = false;
        g.GetComponent<ParticleSystem>().Play();
        g.transform.position = new Vector3(g.transform.position.x, g.transform.position.y, -0.05f);
    }

    private void OnDeath()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        // set velocity to 0
        _rb.velocity = Vector2.zero;

        // flip santa upside down
        transform.localScale = new Vector3(currentDirection, -1, 1);

        // "pop" santa with pjump force
        float jumpForce = Mathf.Sqrt(jumpHeight * -2 * Physics2D.gravity.y * _rb.gravityScale) * _rb.mass;
        _rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);

        // disable capsule collider
        GetComponent<CapsuleCollider2D>().enabled = false;

        // reset level after 3 seconds
        yield return new WaitForSeconds(3);
        
        // we will temporarily just reload the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
