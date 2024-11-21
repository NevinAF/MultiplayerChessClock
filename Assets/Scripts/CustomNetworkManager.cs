using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using Mirror.Discovery;
using System.Linq;

public class CustomNetworkManager : NetworkManager
{

	// private bool tryClient = true;

	// public NetworkDiscovery networkDiscovery;
	// public Button startHostButton;
	// public Button startClientButton;
	// public Button stopButton;

	// public double discoveryTimeout = 0;

	// public override void Start()
	// {
	// 	base.Start();

	// 	startHostButton.onClick.AddListener(AdvertiseHost);
	// 	startClientButton.onClick.AddListener(StartDiscovery);
	// 	stopButton.onClick.AddListener(StopAll);
	// }

	// public override void Update()
	// {
	// 	base.Update();

	// 	if (NetworkServer.active || NetworkClient.active)
	// 	{
	// 		startHostButton.interactable = false;
	// 		startClientButton.interactable = false;
	// 		stopButton.interactable = true;
	// 	}
	// 	else
	// 	{
	// 		startHostButton.interactable = Time.timeAsDouble - discoveryTimeout > 0;
	// 		startClientButton.interactable = Time.timeAsDouble - discoveryTimeout > 0;
	// 		stopButton.interactable = false;
	// 	}
	// }

	// public void StartDiscovery()
	// {
	// 	networkDiscovery.StartDiscovery();
	// 	discoveryTimeout = Time.timeAsDouble + 5;
	// }

	// public void AdvertiseHost()
	// {
	// 	StartHost();
	// 	networkDiscovery.AdvertiseServer();
	// 	discoveryTimeout = 0;
	// }

	// public void DiscoveredServer(ServerResponse info)
	// {
	// 	StartClient(info.uri);
	// 	discoveryTimeout = 0;
	// }

	public void StopAll()
	{
		StopClient();
		StopServer();
		// discoveryTimeout = 0;
	}
}