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
		}
	}



	private void OnDisable()
	{
		networkDiscovery.StopDiscovery();
	}

	public void DiscoveredServer(ServerResponse info)
	{
		UnityEngine.Debug.Log("Discovered Server: " + info.EndPoint.Address + " " + info.uri);
		LobbyButton lobby = Instantiate(LobbyPrefab, LobbyParent);
		lobby.IP.text = info.uri + " " + info.EndPoint.Address;
		lobby.Button.onClick.AddListener(() => networkManager.StartClient(info.uri));
	}

	public void HostLAN()
	{
		networkManager.StartHost();
		networkDiscovery.AdvertiseServer();
	}
}
