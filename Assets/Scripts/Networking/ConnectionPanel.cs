using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class ConnectionPanel : MonoBehaviour
{
	public SDispatcher<bool> HasLobbiesForRecover;

	public UnityEvent OnConnecting;

	public LobbyButton LobbyPrefab;
	public Transform LobbyParent;
	public LobbyDiscovery networkDiscovery;
	public NetworkManager networkManager;

	public double lobbyTimeoutBuffer = 3;

	public Dictionary<LobbyServerResponse, LobbyButton> discoveredServers = new Dictionary<LobbyServerResponse, LobbyButton>();
	public Queue<LobbyServerResponse> deleteQueue = new Queue<LobbyServerResponse>();

	protected virtual void OnEnable()
	{
		while (LobbyParent.childCount > 0)
		{
			Destroy(LobbyParent.GetChild(0).gameObject);
		}

		networkDiscovery.StartDiscovery();
		networkDiscovery.OnServerFound.AddListener(InternalDiscoveredServer);

		HasLobbiesForRecover.Value = LobbySaves.HasSavedLobbies();
	}

	protected virtual void Update()
	{
		foreach (var server in discoveredServers)
		{
			if (Time.timeAsDouble - server.Value.lastUpdateTime > lobbyTimeoutBuffer)
			{
				Destroy(server.Value.gameObject);
				deleteQueue.Enqueue(server.Key);
			}
		}

		while (deleteQueue.Count > 0)
		{
			discoveredServers.Remove(deleteQueue.Dequeue());
		}
	}

	private void OnDisable()
	{
		DestroyAll();
		networkDiscovery.StopDiscovery();
		networkDiscovery.OnServerFound.RemoveListener(InternalDiscoveredServer);
	}

	private void DestroyAll()
	{
		foreach (var server in discoveredServers)
			Destroy(server.Value.gameObject);
		discoveredServers.Clear();
	}

	public void OnRefresh()
	{
		DestroyAll();
		networkDiscovery.StartDiscovery();
	}

	protected void InternalDiscoveredServer(LobbyServerResponse info) => DiscoveredServer(info);

	protected virtual LobbyButton DiscoveredServer(LobbyServerResponse info)
	{
		if (!discoveredServers.TryGetValue(info, out LobbyButton lobby))
		{
			lobby = Instantiate(LobbyPrefab, LobbyParent);
			lobby.Button.onClick.AddListener(() => ConnectUri(info.uri));

			discoveredServers.Add(info, lobby);
		}

		lobby.lastUpdateTime = Time.timeAsDouble;
		lobby.Name.Value = info.name;
		lobby.IconIndex.Value = info.iconIndex;
		lobby.PlayerCount.Value = info.playerCount;
		lobby.Desc.Value = info.uri.Host + ":" + info.uri.Port;
		return lobby;
	}

	public void HostLAN()
	{
		OnConnecting?.Invoke();
		networkManager.StartHost();

		var uri = NetworkManager.singleton.transport.ServerUri();
		LobbyInfoDispatcher.Instance.Address.Value = uri.Host + ":" + uri.Port;
	}

	public void ConnectUri(Uri uri)
	{
		OnConnecting?.Invoke();
		networkManager.StartClient(uri);
		LobbyInfoDispatcher.Instance.Address.Value = uri.Host + ":" + uri.Port;
	}

	public void SetHasLobbiesForRecover(bool value) => HasLobbiesForRecover.Value = value;
}
