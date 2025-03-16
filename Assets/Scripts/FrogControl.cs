using Unity.Netcode;
using UnityEngine;

public class FrogControl : NetworkBehaviour
{
    public Rigidbody2D frog; // frog
    public int frogJump = 12; // just enough to almost touch the top but not quite
    public int moveSpeed = 10; // he zoomin

    private HealthManager healthManager;

    // Network variable for grounded! Local variable DOES NOT work (see bottom of the page)
    private NetworkVariable<bool> networkedGrounded = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    void Start()
    {
        frog = GetComponent<Rigidbody2D>(); // setup the frog and make him not rotate when hitting things
        frog.freezeRotation = true;

        healthManager = FindObjectOfType<HealthManager>();
    }

    void Update()
    {
        if (IsOwner) // don't control movement if foreign actor
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        float moveX = 0f; // declare s2s movement direction as 0 by default
        if (Input.GetKey(KeyCode.A)) // move left
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            moveX = -1f; // move direction and sprite orientation are opposite because the sprite faces left by default
        }
        else if (Input.GetKey(KeyCode.D)) // move right
        {
            transform.localScale = new Vector3(-1*Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            moveX = 1f; // move direction and sprite orientation are opposite because the sprite faces left by default
        }

        // Tell server we want to move horizontal (must always do this or else we'll keep moving forever)
        UpdateMovementServerRpc(moveX);

        // check for up key if yes AND grounded do that. No jumping midair, that's illegal
        if (Input.GetKeyDown(KeyCode.W) && networkedGrounded.Value)
        {
            JumpServerRpc();
        }

        // check for down code, if yeah then do that
        if (Input.GetKey(KeyCode.S))
        {
            MoveDownServerRpc();
        }
    }

    // Tell the server we want to move left/right
    [ServerRpc]
    void UpdateMovementServerRpc(float xDir ) // xDir either 1 or -1 depending on left/right
    {
        Vector2 newVelocity = frog.velocity; // get the current velocity
        newVelocity.x = xDir * moveSpeed; // get s2s movement = direction x speed
        if (xDir != 0 && frog.velocity.y > 0) // if we are going up immedietely stop doing that, does it make logical sense? IDK, but it looks stupid otherwise
        {
            newVelocity.y = 0;
        }
        frog.velocity = newVelocity; // update with new velocity settings
    }

    // Tell the server we want to jump
    [ServerRpc]
    void JumpServerRpc()
    {
        Vector2 newVelocity = frog.velocity; // get the current velocity
        newVelocity.y = frogJump; // set going up to be = frogJump (12)
        frog.velocity = newVelocity; // update with new velocity settings
    }

    // Tell the server we want to move down
    [ServerRpc]
    void MoveDownServerRpc()
    {
        Vector2 newVelocity = frog.velocity; // get the current velocity
        if (newVelocity.y > 0) // if > 0 immediately stop going up, we want to go down
            newVelocity.y = 0;
        else // if <= 0 decrease velocity further (faster than letting gravity do it)
            newVelocity.y -= 0.5f;
        frog.velocity = newVelocity; // update with new velocity settings
    }

    // Enter/exit conditions which tell the server to update Grounded state on tocuhing/not touching
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // If you hit the ground then you touched grass! I wish I could do that :(
        {
            soundControls.Instance.PlaySoundServerRpc(5);
            SetGroundedState(true); // also set grounded so you can't jump
        }
        if (collision.gameObject.CompareTag("DamageDealer")) // if you hit damageDealer (evil) then take 1 point of damage
        {
            healthManager.TakeDamageServerRpc(1, gameObject);
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // no longer touching grass :(
        {
            SetGroundedState(false);
        }
    }

    // Server call to update grounded. MUST update server as a local variable means the server doesn't know if you are grounded or not
    private void SetGroundedState(bool isGrounded)
    {
        if (IsServer)
        {
            networkedGrounded.Value = isGrounded;
        }
    }
}

