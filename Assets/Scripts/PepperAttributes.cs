using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PepperAttributes : NetworkBehaviour
{
    public Rigidbody2D pepper;
    public int pointValue;
    private Coroutine pepperMoveControls;
    private float moveIncrement = 1f;
    private int ymul;
    private int xmul;
    private int randomInt; // this is for move direction
    private int randomInt2; // this is for which pepper spawns
    private bool correctionMove = false;

    [SerializeField] private GameObject redPepperPrefab;
    [SerializeField] private GameObject orangePepperPrefab;
    [SerializeField] private GameObject yellowPepperPrefab;

    private Points thisIsPoints;


    void Start() // at start need to set a new pepper as the right object, freeze rotation
    {
        pepper = GetComponent<Rigidbody2D>();
        pepper.freezeRotation = true;
        StartNewMovement(); // tell the pepper to start their moves

        thisIsPoints = FindObjectOfType<Points>(); // declare points object so we can tranfer pepper's point value if caught

    }

    void Update() // Tell the pepper to manually do certain moves if its out of bounds, overriding random movement
    {
        if (!correctionMove) // Check to ensure we aren't already correcting the location
        {
            if (pepper.position.x > 8) // pepper is too far to the right
            {
                if (pepperMoveControls != null)
                {
                    StopCoroutine(pepperMoveControls);
                }
                correctionMove = true;
                pepperMoveControls = StartCoroutine(MoveForTime(4));
            }
            else if (pepper.position.x < -8) // pepper is too far to the left
            {
                if (pepperMoveControls != null)
                {
                    StopCoroutine(pepperMoveControls);
                }
                correctionMove = true;
                pepperMoveControls = StartCoroutine(MoveForTime(2));
            }
            else if (pepper.position.y > 5) // pepper is at the top of the map (invisible)
            {
                if (pepperMoveControls != null)
                {
                    StopCoroutine(pepperMoveControls);
                }
                correctionMove = true;
                pepperMoveControls = StartCoroutine(MoveForTime(3));
            }
            else if (pepper.position.y < 0) // pepper is too low, could hit a player doing nothing
            {
                if (pepperMoveControls != null)
                {
                    StopCoroutine(pepperMoveControls);
                }
                correctionMove = true;
                pepperMoveControls = StartCoroutine(MoveForTime(1));
            }
        }
    }

    void StartNewMovement() // if the pepper isn't already moving, start it going
    {
        if (pepperMoveControls == null)
        {
            randomInt = Random.Range(1, 5); // 1 up, 2 right, 3 down, 4 left
            pepperMoveControls = StartCoroutine(MoveForTime(randomInt)); // choose random mov to make
        }
    }

    IEnumerator MoveForTime(int direction)
    {
        if (direction == 1) // move up
        {
            ymul = 1;
            xmul = 0;
        }
        else if (direction == 2) // move right
        {
            ymul = 0;
            xmul = 1;
        }
        else if (direction == 3) // move down
        {
            ymul = -1;
            xmul = 0;
        }
        else // move left
        {
            ymul = 0;
            xmul = -1;
        }
        float timeElapsed = 0f;
        while (timeElapsed < moveIncrement) // tell pepper to move for moveIncrement (1s)
        {
            timeElapsed += Time.deltaTime;
            pepper.velocity = new Vector2(xmul * pointValue, ymul * pointValue);
            yield return null;
        }
        correctionMove = false; // if this was a correction move tell pepper it no longer needs to do correction moves
        pepperMoveControls = null; // disable movement
        StartNewMovement(); // start a new movement
    }

    // Handle collision with the player
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player")) // If the player touches the pepper
        {
            // Handle the pepper's destruction and respawning logic
            HandlePepperCollectionServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void HandlePepperCollectionServerRpc()
    {
        thisIsPoints.AddPointsServerRpc(pointValue); // update server with the points gained

        // Play collection sound
        soundControls.Instance.PlaySoundServerRpc(2);

        // Spawn a new pepper via ServerRpc
        SpawnNewPepperServerRpc();

        // Properly despawn the pepper
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null && networkObject.IsSpawned)
        {
            networkObject.Despawn(true);
        }
    }

    // Spawn a new pepper at a random position at the top of the map
    [ServerRpc(RequireOwnership = false)]
    void SpawnNewPepperServerRpc()
    {
        randomInt2 = Random.Range(1, 4); // this is basically just choosing a random pepper to spawn, otherwise it's the exact same as in PrefabManager, see there
        Vector2 pepperSpawnPosition = new Vector2(Random.Range(-8f, 8f), 5f);
        if (randomInt2 == 1)
        {
            GameObject newPepper = Instantiate(redPepperPrefab, pepperSpawnPosition, Quaternion.identity);
            NetworkObject networkObjectPepper = newPepper.GetComponent<NetworkObject>();
            if (networkObjectPepper != null)
            {
                networkObjectPepper.Spawn();
            }
        }
        else if (randomInt2 == 2)
        {
            GameObject newPepper = Instantiate(orangePepperPrefab, pepperSpawnPosition, Quaternion.identity);
            NetworkObject networkObjectPepper = newPepper.GetComponent<NetworkObject>();
            if (networkObjectPepper != null)
            {
                networkObjectPepper.Spawn();
            }
        }
        else
        {
            GameObject newPepper = Instantiate(yellowPepperPrefab, pepperSpawnPosition, Quaternion.identity);
            NetworkObject networkObjectPepper = newPepper.GetComponent<NetworkObject>();
            if (networkObjectPepper != null)
            {
                networkObjectPepper.Spawn();
            }
        }
    }
}
