using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConnectionPanel : MonoBehaviour
{
	public LobbyButton LobbyPrefab;
	public Transform LobbyParent;
	public NetworkDiscovery networkDiscovery;
	public NetworkManager networkManager;

	public UnityEvent Connected;

	public double lobbyTimeout = 10;

	public Dictionary<ServerResponse, LobbyButton> discoveredServers = new Dictionary<ServerResponse, LobbyButton>();

	private void OnEnable()
	{
		while (LobbyParent.childCount > 0)
		{
			Destroy(LobbyParent.GetChild(0).gameObject);
		}

		networkDiscovery.StartDiscovery();
	}

	private void Update()
	{
		if (NetworkServer.active || NetworkClient.active)
		{
			Connected?.Invoke();
			gameObject.SetActive(false);
			networkDiscovery.StopDiscovery();

			if (NetworkServer.active)
			{
				networkDiscovery.AdvertiseServer();
			}

			return;
		}

		foreach (KeyValuePair<ServerResponse, LobbyButton> kvp in discoveredServers)
		{
			if (Time.timeAsDouble > kvp.Value.TimeoutAt)
			{
				Destroy(kvp.Value.gameObject);
				discoveredServers.Remove(kvp.Key);
			}
		}
	}

	private void OnDisable()
	{
	}

	public void OnRefresh()
	{
		while (LobbyParent.childCount > 0)
		{
			Destroy(LobbyParent.GetChild(0).gameObject);
		}

		networkDiscovery.StartDiscovery();
	}

	public void DiscoveredServer(ServerResponse info)
	{
		if (!discoveredServers.TryGetValue(info, out LobbyButton lobby))
		{
			lobby = Instantiate(LobbyPrefab, LobbyParent);
			lobby.IP.text = info.uri + " " + info.EndPoint.Address;
			lobby.Button.onClick.AddListener(() => networkManager.StartClient(info.uri));

			discoveredServers[info] = lobby;
		}

		lobby.TimeoutAt = Time.timeAsDouble + lobbyTimeout;
	}

	public void HostLAN()
	{
		networkManager.StartHost();
	}
}
