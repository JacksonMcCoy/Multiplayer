using Unity.Netcode;
using UnityEngine;

public class EvilControl : NetworkBehaviour
{
    public Rigidbody2D evil;
    private int moveSpeed = 10;

    [SerializeField] private Vector3 evilSpawnRight = new Vector3(-5, 1, 0); // Default Spawn (Left Side)
    [SerializeField] private Vector3 evilSpawnLeft = new Vector3(5, 1, 0);  // Alternate Spawn (Right Side)

    void Start()
    {
        evil = GetComponent<Rigidbody2D>(); // declare evil, make him not rotate (and believe me, he will. He is a very light and fragile boy)
        evil.freezeRotation = true;
    }

    void Update()
    {
        if (IsOwner) // Handle movement only if this is the local player's control
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        // Movement logic for Evil, just like the Frog, handled via server. It's basically the same except we don't have to obey silly things like gravity and *touching grass*
        if (Input.GetKey(KeyCode.W))
        {
            MoveUpServerRpc();
        }
        else if (Input.GetKey(KeyCode.A))
        {
            MoveLeftServerRpc();
        }
        else if (Input.GetKey(KeyCode.D))
        {
            MoveRightServerRpc();
        }
        else if (Input.GetKey(KeyCode.S))
        {
            MoveDownServerRpc();
        }
        else
        {
            StopMovingServerRpc(); // stop moving if not doing anything.
        }
    }

    // Move up
    [ServerRpc]
    void MoveUpServerRpc()
    {
        Vector2 newVelocity = evil.velocity;
        newVelocity.y = moveSpeed;
        evil.velocity = newVelocity;
    }

    // Move left
    [ServerRpc]
    void MoveLeftServerRpc()
    {
        Vector2 newVelocity = evil.velocity;
        newVelocity.x = -moveSpeed;
        evil.velocity = newVelocity;

        // Flip the sprite when moving left
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    // Move right
    [ServerRpc]
    void MoveRightServerRpc()
    {
        Vector2 newVelocity = evil.velocity;
        newVelocity.x = moveSpeed;
        evil.velocity = newVelocity;

        // Flip the sprite when moving right
        transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    // Move down
    [ServerRpc]
    void MoveDownServerRpc()
    {
        Vector2 newVelocity = evil.velocity;
        newVelocity.y = -moveSpeed;
        evil.velocity = newVelocity;
    }

    // Stop movement
    [ServerRpc]
    void StopMovingServerRpc()
    {
        evil.velocity = Vector2.zero;
    }

    // run into player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // If Evil touches Player
        {
            soundControls.Instance.PlaySoundServerRpc(0); // evil death noise
            float playerX = collision.transform.position.x; // Get Player's X position

            // Decide which spawn point to use
            Vector3 newSpawnPosition = (playerX > 0) ? evilSpawnRight : evilSpawnLeft;

            // **Teleport Evil using RPC**
            TeleportToSpawnServerRpc(newSpawnPosition); // this is a pseudo-death. Evil dies on hit always so there's no need to kill it and respawn it, inefficient
        }
    }

    // tell server to teleport evil, we died
    [ServerRpc(RequireOwnership = false)]
    void TeleportToSpawnServerRpc(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
    }
}