using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathScreen : NetworkBehaviour
{
    public TMP_Text pointsText;
    public Button restartButton;
    public GameObject deathScreenBackground;

    void Start()
    {
        // Add listener to restart button if it's set up in the inspector
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartButton);
        }
    }

    [ClientRpc]
    public void ShowDeathScreenClientRpc(int score)
    {
        gameObject.SetActive(true); // Make death screen active
        soundControls.Instance.PlaySoundServerRpc(1); // Play sound

        if (pointsText != null)
        {
            pointsText.text = score.ToString() + " points"; // if points exists then update it
        }

        // turn on death Screen, as its disabled by default
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void RestartButton()
    {
        // Call a ServerRpc to notify the host to restart the scene
        RestartGameServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RestartGameServerRpc()
    {
        // Clients can restart the game but it should be done server side, no unneccessary access
        if (IsServer)
        {
            // Hide death screen otherwise you can't do anything once it restarts
            HideDeathScreenClientRpc();

            // Shutdown the network, disconnect all players and networked objects
            NetworkManager.Singleton.Shutdown(); // "Clears the slate" otherwise you end up with 6 peppers and 2 evils since they don't despawn on death of player 1

            // Reload the scene after disconnecting
            NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);

            // Force client to reload the scene, otherwise they won't have working points/health display
            ForceSceneReloadClientRpc();
        }
    }

    [ClientRpc]
    private void ForceSceneReloadClientRpc()
    {
        SceneManager.LoadScene("SampleScene"); // Clients reload the scene manually
    }

    [ClientRpc]
    private void HideDeathScreenClientRpc()
    {
        // Hide death screen on all clients
        gameObject.SetActive(false);  // Disable the death screen object
        deathScreenBackground.SetActive(false); // Hide the background (death screen)
    }
}



