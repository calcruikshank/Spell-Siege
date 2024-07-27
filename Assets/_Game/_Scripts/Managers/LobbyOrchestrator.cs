using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable CS4014

public class LobbyOrchestrator : NetworkBehaviour
{
    private const int MaxRetries = 5;

    [SerializeField] private MainLobbyScreen _mainLobbyScreen;
    [SerializeField] private CreateLobbyScreen _createScreen;
    [SerializeField] private RoomScreen _roomScreen;

    private void Start()
    {
        _mainLobbyScreen.gameObject.SetActive(true);
        _createScreen.gameObject.SetActive(false);
        _roomScreen.gameObject.SetActive(false);

        CreateLobbyScreen.LobbyCreated += CreateLobby;
        LobbyRoomPanel.LobbySelected += OnLobbySelected;
        RoomScreen.LobbyLeft += OnLobbyLeft;
        RoomScreen.StartPressed += OnGameStart;

        NetworkObject.DestroyWithScene = true;
    }

    #region Main Lobby

    private async void OnLobbySelected(Lobby lobby)
    {
        using (new Load("Joining Lobby..."))
        {
            try
            {
                await MatchmakingService.JoinLobbyWithAllocation(lobby.Id);

                _mainLobbyScreen.gameObject.SetActive(false);
                _roomScreen.gameObject.SetActive(true);

                NetworkManager.Singleton.StartClient();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                CanvasUtilities.Instance.ShowError("Failed joining lobby");
            }
        }
    }

    #endregion

    #region Create
    private async void CreateLobby(LobbyData data)
    {
        using (new Load("Creating Lobby..."))
        {
            int retries = 0;

            while (true)
            {
                try
                {
                    await MatchmakingService.CreateLobbyWithAllocation(data);

                    _createScreen.gameObject.SetActive(false);
                    _roomScreen.gameObject.SetActive(true);

                    // Starting the host immediately will keep the relay server alive
                    NetworkManager.Singleton.StartHost();
                    break; // Exit the loop if the request succeeds
                }
                catch (LobbyServiceException ex) when (ex.Reason == LobbyExceptionReason.RateLimited)
                {
                    retries++;

                    if (retries >= MaxRetries)
                    {
                        Debug.LogError("Max retries reached. Failed creating lobby.");
                        CanvasUtilities.Instance.ShowError("Failed creating lobby: Too many requests. Please try again later.");
                        break;
                    }

                    // Wait before retrying
                    int delay = (int)Math.Pow(2, retries) * 1000; // Exponential backoff
                    Debug.LogWarning($"Rate limit exceeded. Retrying in {delay / 1000} seconds...");
                    await Task.Delay(delay);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    CanvasUtilities.Instance.ShowError("Failed creating lobby");
                    break;
                }
            }
        }
    }


    #endregion

    #region Room

    private readonly Dictionary<ulong, bool> _playersInLobby = new();
    public static event Action<Dictionary<ulong, bool>> LobbyPlayersUpdated;
    private float _nextLobbyUpdate;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            _playersInLobby.Add(NetworkManager.Singleton.LocalClientId, false);
            UpdateInterface();
        }

        // Client uses this in case host destroys the lobby
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnClientConnectedCallback(ulong playerId)
    {
        if (!IsServer) return;

        // Add locally
        if (!_playersInLobby.ContainsKey(playerId)) _playersInLobby.Add(playerId, false);

        PropagateToClients();

        UpdateInterface();
    }

    private void PropagateToClients()
    {
        foreach (var player in _playersInLobby) UpdatePlayerClientRpc(player.Key, player.Value);
    }

    [ClientRpc]
    private void UpdatePlayerClientRpc(ulong clientId, bool isReady)
    {
        if (IsServer) return;

        if (!_playersInLobby.ContainsKey(clientId)) _playersInLobby.Add(clientId, isReady);
        else _playersInLobby[clientId] = isReady;
        UpdateInterface();
    }

    private void OnClientDisconnectCallback(ulong playerId)
    {
        if (IsServer)
        {
            // Handle locally
            if (_playersInLobby.ContainsKey(playerId)) _playersInLobby.Remove(playerId);

            // Propagate all clients
            RemovePlayerClientRpc(playerId);

            UpdateInterface();
        }
        else
        {
            // This happens when the host disconnects the lobby
            _roomScreen.gameObject.SetActive(false);
            _mainLobbyScreen.gameObject.SetActive(true);
            OnLobbyLeft();
        }
    }

    [ClientRpc]
    private void RemovePlayerClientRpc(ulong clientId)
    {
        if (IsServer) return;

        if (_playersInLobby.ContainsKey(clientId)) _playersInLobby.Remove(clientId);
        UpdateInterface();
    }

    public void OnReadyClicked()
    {
        SetReadyServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(ulong playerId)
    {
        _playersInLobby[playerId] = true;
        PropagateToClients();
        UpdateInterface();
    }

    private void UpdateInterface()
    {
        LobbyPlayersUpdated?.Invoke(_playersInLobby);
    }

    private async void OnLobbyLeft()
    {
        using (new Load("Leaving Lobby..."))
        {
            _playersInLobby.Clear();
            NetworkManager.Singleton.Shutdown();
            await MatchmakingService.LeaveLobby();
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        CreateLobbyScreen.LobbyCreated -= CreateLobby;
        LobbyRoomPanel.LobbySelected -= OnLobbySelected;
        RoomScreen.LobbyLeft -= OnLobbyLeft;
        RoomScreen.StartPressed -= OnGameStart;

        // We only care about this during lobby
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }
    }

    private async void OnGameStart()
    {
        using (new Load("Starting the game..."))
        {
            await MatchmakingService.LockLobby();
            NetworkManager.Singleton.SceneManager.LoadScene("SimpleGame", LoadSceneMode.Single);
        }
    }

    #endregion
}
