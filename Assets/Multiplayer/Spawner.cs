using UnityEngine;
using Unity.Netcode;

public class Spawner : NetworkBehaviour
{

    public GameObject carPrefab;

    private int playerCount = 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // GameObject prefabToSpawn = playerCount == 0 ? firePlayerPrefab : waterPlayerPrefab;

        var playerInstance = Instantiate(carPrefab, Vector3.zero, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        playerCount++;
    }

}
