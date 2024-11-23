using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class ConnectionErrorPanel : ConnectionPanel
{
	protected override LobbyButton DiscoveredServer(LobbyServerResponse info)
	{
		bool nameMatch = info.name == LobbyInfoDispatcher.Instance.Name;
		bool addressMatch = LobbyInfoDispatcher.Instance.Address == info.uri.Host + ":" + info.uri.Port;

		UnityEngine.Debug.Log("DiscoveredServer: " + info.name + " " + info.uri.Host + ":" + info.uri.Port + " " + nameMatch + " " + addressMatch);

		if (nameMatch && addressMatch)
		{
			ConnectUri(info.uri);
		}
		else if (nameMatch || addressMatch) {
			
			LobbyButton lobby = base.DiscoveredServer(info);
			lobby.Desc.Value += nameMatch ? " <i>(Same Name)" : " <i>(Same Address)";
			return lobby;
		}

		return null;
	}

	protected override void Update()
	{
		base.Update();

		HasLobbiesForRecover.Value = discoveredServers.Count > 0;
	}


}
