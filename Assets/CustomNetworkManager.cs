using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class CustomNetworkManager : NetworkManager
{
	private bool tryClient = true;

	public override void Update()
	{
		base.Update();

		if (NetworkServer.active || NetworkClient.active)
			return;

		if (tryClient)
		{
			tryClient = false;
			StartClient();
		}
		else
		{
			tryClient = true;
			StartHost();
		}
	}
}