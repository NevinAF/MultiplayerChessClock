using System;
using System.Net;
using System.Runtime.InteropServices;
using Mirror;

public struct LobbyServerResponse : NetworkMessage
{
	// The server that sent this
	// this is a property so that it is not serialized,  but the
	// client fills this up after we receive it
	public IPEndPoint EndPoint { get; set; }

	public Uri uri;

	// Prevent duplicate server appearance when a connection can be made via LAN on multiple NICs
	public long serverId;

	[MarshalAs(UnmanagedType.LPStr, SizeConst = 32)]
	public string name;
	public int iconIndex;
	public byte playerCount;

	public bool Equals(LobbyServerResponse other)
	{
		return serverId == other.serverId && uri == other.uri;
	}
}