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
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System;

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
    private Lobby hostLobby;
    [SerializeField] private int numOfPlayers = 4;

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
        if (NetData.Instance.IsLobby)
        {
            await InitializeRelay();
            hostLobby = await GetFirstLobby();
            if (hostLobby == null)
            {
                await CreateRelay();
                await CreateLobby(joinCode);
                StartCoroutine(HandleLobbyHeartbeat());
            }
            else
            {
                await JoinLobby();
                JoinRelay(hostLobby.Data["JoinCode"].Value);
            }
        }

        if (NetData.Instance.IsServer)
        {
            await InitializeRelay();
            await CreateRelay();
            JoinRelay(joinCode);
        }
        else
        {
            await InitializeRelay();
            JoinRelay(NetData.Instance.JoinCode);
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
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(numOfPlayers);
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

    private async Task<Lobby> GetFirstLobby()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " Available slots: " + lobby.AvailableSlots);
            }
            if (queryResponse.Results.Count > 0) return queryResponse.Results[0];
            return null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    private Unity.Services.Lobbies.Models.Player GetPlayer()
    {
        var id = AuthenticationService.Instance.PlayerId;
        var playerName = "Player " + id;

        return new Unity.Services.Lobbies.Models.Player(id, data:
            new Dictionary<string, PlayerDataObject>
            {
                {
                    "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)
                },
            });
    }

    private async Task<Lobby> CreateLobby(string joinCode)
    {
        string lobbyName = "NetFrog Lobby" + Guid.NewGuid();
        try
        {
            var lobbyOptions = new Unity.Services.Lobbies.CreateLobbyOptions()
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "JoinCode", new DataObject(DataObject.VisibilityOptions.Public,joinCode)
                    }
                },
                Player = GetPlayer(),
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, numOfPlayers, lobbyOptions);
            Debug.Log("A lobby has been created: " + lobby.Name + " " + lobby.MaxPlayers);
            return lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    private IEnumerator HandleLobbyHeartbeat()
    {
        while (hostLobby != null)
        {
            yield return new WaitForSeconds(15f);
            LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }

    private async Task JoinLobby()
    {
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions()
            {
                Player = GetPlayer()
            };
            await Lobbies.Instance.JoinLobbyByIdAsync(hostLobby.Id, options);

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

}
