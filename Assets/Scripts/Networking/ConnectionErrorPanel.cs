using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class ConnectionErrorPanel : ConnectionPanel
{
	protected override LobbyButton DiscoveredServer(LobbyServerResponse info)
	{
		var disconnectInfo = LobbyNetworkManager.Instance.Info;
		bool nameMatch = info.name == disconnectInfo.name;
		bool addressMatch = disconnectInfo.Address == info.uri.Host + ":" + info.uri.Port;

		if (nameMatch && addressMatch)
		{
			networkManager.StartClient(info.uri);
		}
		else if (nameMatch || addressMatch) {
			
			LobbyButton lobby = base.DiscoveredServer(info);
			lobby.Desc.Value += nameMatch ? " <i>(Same Name)" : " <i>(Same Address)";
			return lobby;
		}

		return null;
	}

}
