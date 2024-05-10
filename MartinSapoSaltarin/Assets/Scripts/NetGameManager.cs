using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class NetGameManager : NetworkBehaviour, IGameManager
{
    [SerializeField]
    GameObject playerPrefab;

    [SerializeField]
    Canvas serverModeCanvas;

    Dictionary<ulong, GameObject> players = new Dictionary<ulong, GameObject>();
    AudioSource audioSource;
    [SerializeField] TextMeshProUGUI codeText;
    private string joinCode;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    public override void OnNetworkSpawn()
    {
        SpawnClientPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        if (!IsServer)
        {
            Debug.Log("Cliente conectado, solicitando un nuevo player...");
            //SpawnClientPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            Debug.Log("Server arrancado");
            serverModeCanvas.enabled = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnClientPlayerServerRpc(ulong clientId)
    {
        Debug.Log("Cliente conectado, Creando un nuevo player..." + clientId);
        var player = Instantiate(playerPrefab, new Vector3(-7, 0, 0), Quaternion.identity);
        player.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
        player.GetComponent<NetPlayer>().SetPlayerId(clientId);
        players.Add(clientId, player);
    }


    // Start is called before the first frame update
    async void Start()
    {
        if (NetData.Instance.IsServer)
        {
            await InitializeRelay();
            await CreateRelay();
            JoinRelay(joinCode);
        }
        else
        {
            await InitializeRelay();
            JoinRelay(NetData.Instance.joinCode);
        }
        Debug.Log("NetGameManager awake. Is server: " + IsServer + " Is Client: " + IsClient);
        Debug.Log("NetGameManager awake. NetData Is server: " + NetData.Instance.IsServer);
    }

    public void OnPlayerDeath(ulong clientId)
    {
        //audioSource.Play();
        OnPlayerDeathClientRpc(clientId);
        SpawnClientPlayerServerRpc(clientId);
    }

    [ClientRpc]
    private void OnPlayerDeathClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId) audioSource.Play();
    }

    public GameObject GetCurrentPlayer()
    {
        if (IsClient)
        {
            if (players.ContainsKey(NetworkManager.Singleton.LocalClientId)) return players[NetworkManager.Singleton.LocalClientId];
            return null;
        }
        if (players.Count == 0) return null;
        GameObject farestFrog = players.First().Value;
        foreach (var player in players)
        {
            if (player.Value.transform.position.x > farestFrog.transform.position.x) farestFrog = player.Value;
        }
        return farestFrog;
    }

    public void SetClientPlayer(GameObject player, ulong clientId)
    {
        Debug.Log($"Player {clientId}  registrado en cliente {NetworkManager.Singleton.LocalClientId}");
        players.Add(clientId, player);
    }

    private async Task InitializeRelay()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Singed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async Task CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Join code: {joinCode}");
            codeText.text += joinCode;
            //NetData.Instance.joinCode = joinCode;
            //code = joinCode;
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost(); //Cambio fase 2
            //code = joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);

        }
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log($"Joining Relay with {joinCode}");
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            if (!NetData.Instance.IsServer)
                NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

}
