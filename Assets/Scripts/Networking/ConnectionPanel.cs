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
	public LobbyDiscovery networkDiscovery;
	public NetworkManager networkManager;

	public UnityEvent Connected;

	public double lobbyTimeoutBuffer = 3;

	public SortedList<LobbyServerResponse, LobbyButton> discoveredServers = new SortedList<LobbyServerResponse, LobbyButton>();

	private void OnEnable()
	{
		while (LobbyParent.childCount > 0)
		{
			Destroy(LobbyParent.GetChild(0).gameObject);
		}

		networkDiscovery.StartDiscovery();
		networkDiscovery.OnServerFound.AddListener(DiscoveredServer);
	}

	private void Update()
	{
		if (NetworkServer.active || NetworkClient.active)
		{
			Connected?.Invoke();
			gameObject.SetActive(false);
			return;
		}

		for (int i = discoveredServers.Count - 1; i >= 0; i--)
		{
			var button = discoveredServers.Values[i];
			if (Time.timeAsDouble > button.lastUpdateTime + networkDiscovery.ActiveDiscoveryInterval + lobbyTimeoutBuffer)
			{
				Destroy(button.gameObject);
				discoveredServers.RemoveAt(i);
			}
		}
	}

	private void OnDisable()
	{
		DestroyAll();
		networkDiscovery.StopDiscovery();

		if (NetworkServer.active)
		{
			networkDiscovery.AdvertiseServer();
		}
	}

	private void DestroyAll()
	{
		for (int i = discoveredServers.Count - 1; i >= 0; i--)
		{
			Destroy(discoveredServers.Values[i].gameObject);
		}
		discoveredServers.Clear();
	}

	public void OnRefresh()
	{
		DestroyAll();
		networkDiscovery.StartDiscovery();
	}

	public void DiscoveredServer(LobbyServerResponse info)
	{
		if (!discoveredServers.TryGetValue(info, out LobbyButton lobby))
		{
			lobby = Instantiate(LobbyPrefab, LobbyParent);
			lobby.Button.onClick.AddListener(() => networkManager.StartClient(info.uri));

			discoveredServers.Add(info, lobby);
		}

		lobby.lastUpdateTime = Time.timeAsDouble;
		lobby.Name.Value = info.name;
		lobby.IconIndex.Value = info.iconIndex;
		lobby.PlayerCount.Value = info.playerCount;
		lobby.Desc.Value = info.EndPoint.Address.ToString();
	}

	public void HostLAN()
	{
		networkManager.StartHost();
	}
}
