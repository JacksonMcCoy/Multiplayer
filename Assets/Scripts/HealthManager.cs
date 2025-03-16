using Unity.Netcode;
using TMPro;
using UnityEngine;

public class HealthManager : NetworkBehaviour
{
    private TextMeshProUGUI healthText;
    private Points thisIsPoints;

    private NetworkVariable<int> networkedHealth = new NetworkVariable<int>(
        3, // starting health
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    void Start()
    {
        healthText = GetComponentInChildren<TextMeshProUGUI>(); // Health text is assigned to this script which is assigned to Health text. Gotta love unity...
        thisIsPoints = FindObjectOfType<Points>(); // declare points so we can use the function there when the frog dies

        if (healthText == null)
        {
            return;
        }

        UpdateHealthUI(); // set health to value. Important becasue default text in unity is ? as the value
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + networkedHealth.Value.ToString(); // update text to show correct points
        }
    }

    [ServerRpc]
    public void TakeDamageServerRpc(int damageAmount, NetworkObjectReference playerObjectReference)
    {
        networkedHealth.Value -= damageAmount; // reduce health (should always be by 1 but just in case...)

        if (networkedHealth.Value <= 0) // if health is 0 or somehow less then kill the player :O
        {
            networkedHealth.Value = 0;
            DieServerRpc(playerObjectReference);
        }
    }

    [ServerRpc]
    public void DieServerRpc(NetworkObjectReference playerObjectReference)
    {
        thisIsPoints.HandleDeathServerRpc(); // heandle player death from Points script (more on why in Points script)
        if (playerObjectReference.TryGet(out NetworkObject playerNetworkObject))
        {
            playerNetworkObject.Despawn(); // Despawn the player object
        }
        NetworkObject networkObject = GetComponent<NetworkObject>();  // Get the NetworkObject of the text
        if (networkObject != null)
        {
            networkObject.Despawn();  // Despawn points text, since we're dead it will be on the death screen and we don't need 2
        }
    }

    private void OnEnable()
    {
        networkedHealth.OnValueChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        networkedHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        // Ensure health updates the UI text whenever the value changes
        UpdateHealthUI();
    }
}
