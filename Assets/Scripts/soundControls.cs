using Unity.Netcode;
using UnityEngine;

public class soundControls : NetworkBehaviour
{
    public AudioSource evil_loses;
    public AudioSource evil_wins;
    public AudioSource collectPepper;
    public AudioSource frogSpawn;
    public AudioSource evilSpawns;
    public AudioSource touchGrass;

    public static soundControls Instance;

    private void Awake() // set Instance to be a refernce to the object
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Different sound objects
    public void PlayEvilLose()
    {
        evil_loses.Play();
    }

    public void PlayEvilWin()
    {
        evil_wins.Play();
    }

    public void PlayCollectPepper()
    {
        collectPepper.Play();
    }

    public void PlayFrogSpawn()
    {
        frogSpawn.Play();
    }

    public void PlayEvilSpawns()
    {
        evilSpawns.Play();
    }

    public void PlayTouchGrass()
    {
        touchGrass.Play();
    }

    // master controls, take in an int, then tell all clients to play it
    [ServerRpc(RequireOwnership = false)]
    public void PlaySoundServerRpc(int soundId)
    {
        PlaySoundClientRpc(soundId);
    }

    // client just plays the couns corresponding to the int that gets passed
    [ClientRpc]
    private void PlaySoundClientRpc(int soundId)
    {
        if (Instance == null) return;

        switch (soundId)
        {
            case 0:
                Instance.PlayEvilLose();
                break;
            case 1:
                Instance.PlayEvilWin();
                break;
            case 2:
                Instance.PlayCollectPepper();
                break;
            case 3:
                Instance.PlayFrogSpawn();
                break;
            case 4:
                Instance.PlayEvilSpawns();
                break;
            case 5:
                Instance.PlayTouchGrass();
                break;
        }
    }
}
