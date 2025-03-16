using Unity.Netcode;
using UnityEngine;

public class NetworkTransformTest : NetworkBehaviour
{
    public NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Update()
    {
        if (!IsOwner)
        {
            transform.position = networkedPosition.Value;
        }
    }
    public void SetPosition(Vector3 newPos)
    {
        if (IsServer)
        {
            networkedPosition.Value = newPos;
        }
    }
}