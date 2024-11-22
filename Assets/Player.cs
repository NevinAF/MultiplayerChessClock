using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Player : NetworkBehaviour
{
	public static List<Player> Players = new List<Player>();
	public static Action<Player> OnPlayerAdded;
	public static Action<Player> OnPlayerRemoved;

	[SyncVar(hook = nameof(CallOnUsernameChanged))]
	public string Username;
	[SyncVar(hook = nameof(OnIconIndexChanged))]
	public int IconIndex;
	
	[SyncVar(hook = nameof(CallOnDescriptionChanged))]
	public string Description;

	[SyncVar(hook = nameof(CallOnIsHostChanged))]
	public bool IsHost;

	public UnityEvent<string> OnUsernameChanged;
	public UnityEvent<Sprite> OnIconChanged;
	public UnityEvent<string> OnDescriptionChanged;
	public UnityEvent<bool> OnIsHostChanged;

	public override void OnStartClient()
	{
		base.OnStartClient();

		Players.Add(this);
		OnPlayerAdded?.Invoke(this);
	}

	public override void OnStopClient()
	{
		base.OnStopClient();

		Players.Remove(this);
		OnPlayerRemoved?.Invoke(this);
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		Description = connectionToClient.address;
		IsHost = isLocalPlayer;
	}

	public override void OnStartAuthority()
	{
		base.OnStartAuthority();

		LocalSettings.Instance.Name.Attach(SetUsername);
		LocalSettings.Instance.IconIndex.Attach(SetIconIndex);
	}

	public override void OnStopAuthority()
	{
		base.OnStopAuthority();

		LocalSettings.Instance.Name.Detach(SetUsername);
		LocalSettings.Instance.IconIndex.Detach(SetIconIndex);
	}

	private void CallOnUsernameChanged(string oldUsername, string newUsername)
	{
		OnUsernameChanged?.Invoke(newUsername);
	}

	private void OnIconIndexChanged(int oldIndex, int newIndex)
	{
		OnIconChanged?.Invoke(GlobalSpriteList.GetSprite(newIndex));
	}

	private void CallOnDescriptionChanged(string oldDescription, string newDescription)
	{
		OnDescriptionChanged?.Invoke(newDescription);
	}

	private void CallOnIsHostChanged(bool oldIsHost, bool newIsHost)
	{
		OnIsHostChanged?.Invoke(newIsHost);
	}

	private void SetUsername(string newUsername)
	{
		Username = newUsername;
	}

	private void SetIconIndex(int newIndex)
	{
		IconIndex = newIndex;
	}
}
