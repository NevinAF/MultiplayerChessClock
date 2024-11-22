using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using Mirror.Discovery;
using System.Linq;
using UnityEngine.Events;

public class CustomNetworkManager : NetworkManager
{

	// private bool tryClient = true;

	public LobbyDiscovery networkDiscovery;
	// public Button startHostButton;
	// public Button startClientButton;
	// public Button stopButton;

	private bool returningHome = false;

	public UnityEvent Disconnected;
	public UnityEvent Connected;
	public UnityEvent ReturnHome;

	public override void OnClientConnect()
	{
		base.OnClientConnect();

		Connected?.Invoke();
	}

	public override void OnClientDisconnect()
	{
		base.OnClientDisconnect();

		if (!returningHome)
		{
			Disconnected?.Invoke();
		}
	}

	public void OnReturnHome()
	{
		returningHome = true;
		StopAll();
		ReturnHome?.Invoke();
	}

	public override void Update()
	{
		if (NetworkServer.active && !networkDiscovery.IsAdvertising)
		{
			networkDiscovery.AdvertiseServer();
		}
		else returningHome = false;
	}

	public void StopAll()
	{
		networkDiscovery.StopDiscovery();
		StopClient();
		StopServer();
		// discoveryTimeout = 0;
	}
}