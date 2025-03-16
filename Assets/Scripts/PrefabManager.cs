using Unity.Netcode;
using UnityEngine;

public class PrefabManager : NetworkBehaviour
{
    [SerializeField] private GameObject frogPrefab;
    [SerializeField] private GameObject evilPrefab;

    [SerializeField] private Vector3 frogSpawnPosition = new Vector3(5, -1, 0);
    [SerializeField] private Vector3 evilSpawnPosition = new Vector3(-5, 1, 0);

    [SerializeField] private GameObject redPepperPrefab;
    [SerializeField] private GameObject orangePepperPrefab;
    [SerializeField] private GameObject yellowPepperPrefab;

    [SerializeField] private Vector3 pepperSpawnPosition = new Vector3(0, 0, 0);

    private bool spawnedPeppers = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner) // Only handle replacing for the current player
        {
            string selectedRole = IsServer ? "frog" : "evil"; // Server gets Frog, Client gets Evil

            // Spawn the correct prefab immediately
            RequestSpawnPrefabServerRpc(OwnerClientId, selectedRole);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReplaceWithCorrectPrefabServerRpc(ulong clientId, string selectedRole)
    {
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            return;
        }

        var client = NetworkManager.Singleton.ConnectedClients[clientId];

        // === FIX: If no PlayerObject exists, spawn one before replacing ===
        if (client.PlayerObject == null)
        {
            RequestSpawnPrefabServerRpc(clientId, selectedRole);
            return;
        }

        // Destroy the old PlayerObject
        client.PlayerObject.Despawn();
        Destroy(client.PlayerObject.gameObject);

        // Spawning the correct prefab
        SpawnCorrectPrefab(clientId, selectedRole);
    }

    public void SpawnCorrectPrefab(ulong clientId, string selectedRole)
    {
        if (selectedRole == "frog")
            soundControls.Instance.PlaySoundServerRpc(3); // play frog spawn noise
        else
            soundControls.Instance.PlaySoundServerRpc(4); // play evil spawn noise
        GameObject prefabToSpawn = (selectedRole == "frog") ? frogPrefab : evilPrefab;
        Vector3 spawnPosition = selectedRole == "frog" ? frogSpawnPosition : evilSpawnPosition;
        Quaternion spawnRotation = Quaternion.identity;

        if (prefabToSpawn == null)
        {
            return; // If prefab is not assigned, exit function
        }

        GameObject newPlayer = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);

        if (newPlayer == null)
        {
            return; // If instantiation fails, exit function
        }

        NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Destroy(newPlayer);
            return; // If no NetworkObject is found, exit function
        }

        networkObject.SpawnAsPlayerObject(clientId); // make sure to designate that this is the local player and set ownership, critical for move functions
        networkObject.ChangeOwnership(clientId);

        if (selectedRole == "frog" && !spawnedPeppers) // If we are spawning a frog and we didn't already do so, spawn peppers
        {
            SpawnPeppers();
            spawnedPeppers = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnPrefabServerRpc(ulong clientId, string selectedRole)
    {
        // Spawn initial PlayerObject
        SpawnCorrectPrefab(clientId, selectedRole);
    }

    private void SpawnPeppers() // just spawn the peppers
    {
        // Spawn Yellow Pepper
        GameObject newPepper = Instantiate(yellowPepperPrefab, pepperSpawnPosition + new Vector3(1, 0, 0), Quaternion.identity);
        NetworkObject networkObjectYellow = newPepper.GetComponent<NetworkObject>(); // Unique variable for yellow
        if (networkObjectYellow != null)
        {
            networkObjectYellow.Spawn();  // Spawn the yellow pepper across all clients
        }

        // Spawn Orange Pepper
        GameObject newPepper1 = Instantiate(orangePepperPrefab, pepperSpawnPosition, Quaternion.identity);
        NetworkObject networkObjectOrange = newPepper1.GetComponent<NetworkObject>(); // Unique variable for orange
        if (networkObjectOrange != null)
        {
            networkObjectOrange.Spawn();  // Spawn the orange pepper across all clients
        }

        // Spawn Red Pepper
        GameObject newPepper2 = Instantiate(redPepperPrefab, pepperSpawnPosition - new Vector3(1, 0, 0), Quaternion.identity);
        NetworkObject networkObjectRed = newPepper2.GetComponent<NetworkObject>(); // Unique variable for red
        if (networkObjectRed != null)
        {
            networkObjectRed.Spawn();  // Spawn the red pepper across all clients
        }
    }
}


