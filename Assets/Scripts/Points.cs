using Unity.Netcode;
using TMPro;
using UnityEngine;

public class Points : NetworkBehaviour
{
    private TextMeshProUGUI pointsText;

    // Network variable to store points (this will be shared across server and client)
    private NetworkVariable<int> networkedPoints = new NetworkVariable<int>(
        0, // starting points
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    void Start()
    {
        pointsText = GetComponentInChildren<TextMeshProUGUI>(); // Assuming the PointsText is a child of the object this script is attached to

        // Check if pointsText is not null
        if (pointsText != null)
        {
            UpdatePointsUI();
        }
    }

    void Update()
    {
        // Only update points UI for the owning player (client)
        if (IsOwner)
        {
            UpdatePointsUI();
        }
    }

    private void UpdatePointsUI()
    {
        if (pointsText != null)
        {
            pointsText.text = "Score: " + networkedPoints.Value.ToString(); // update the text to show the new value for points
        }
    }

    // Method to add points (called when the player collects something)
    [ServerRpc]
    public void AddPointsServerRpc(int incAmount)
    {
        networkedPoints.Value += incAmount; //update the points value and if it's less than 0, set to 0.

        if (networkedPoints.Value < 0) // This should never happen, if it does it is a bug
        {
            networkedPoints.Value = 0;
        }
    }

    // Method to handle player death (despawn the player object and handle UI)
    // You may wonder; why is the player death handled in points of all things? It's beause we need to pass the player's points for the death screen
    // If we define this in the player, we don't know the points value and have to go digging to find it, extra declarations, etc. Not fun.
    // Instead, call this function from the plater when he dies so we already have points
    [ServerRpc]
    public void HandleDeathServerRpc()
    {
        // Handle player death logic on the server
        KillPointsTextClientRpc();

        // Find and show the death screen
        DeathScreen deathScreen = FindObjectOfType<DeathScreen>(); // Find it manually
        if (deathScreen != null)
        {
            int finalScore = networkedPoints.Value; // honestly this might be pointless but just in case, assign it to an int
            deathScreen.ShowDeathScreenClientRpc(finalScore); // enables the death screen with the final score
        }
    }

    // ServerRpc: Kills the points text on the server
    [ClientRpc]
    private void KillPointsTextClientRpc()
    {
        // Destroy points text only on the server
        if (pointsText != null)
        {
            pointsText.gameObject.SetActive(false); // Remove text from the scene on the server side
        }
    }

    // Listen for points changes and update the UI text across clients
    private void OnEnable()
    {
        networkedPoints.OnValueChanged += OnPointsChanged;
    }

    private void OnDisable()
    {
        networkedPoints.OnValueChanged -= OnPointsChanged;
    }

    private void OnPointsChanged(int oldValue, int newValue)
    {
        // Ensure points updates the UI text whenever the value changes
        UpdatePointsUI();
    }
}
