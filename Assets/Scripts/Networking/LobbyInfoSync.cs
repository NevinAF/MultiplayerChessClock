using Edgegap;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class LobbyInfoSync : NetworkBehaviour
{
	[SyncVar(hook = nameof(CallOnNameChanged))]
	public string Name;

	[SyncVar(hook = nameof(CallOnIconChanged))]
	public int Icon;

	public UnityEvent<string> OnNameChanged;
	public UnityEvent<Sprite> OnIconChanged;
	public SDispatcher<string> Address;
	public SDispatcher<int> PlayerCount;

	public SDispatcher<string> LobbyRuntime;
	// public SDispatcher<string> LobbyUptime;

	private void Awake()
	{
		Player.OnPlayerAdded += _ => PlayerCount.Value = Player.Players.Count;
		Player.OnPlayerRemoved += _ => PlayerCount.Value = Player.Players.Count;

		Name = null;
		Icon = -1;

		void DefaultNameChanged(string value)
		{
			if (NetworkServer.active || NetworkClient.active)
				return;
			Name = value;
		}

		void DefaultIconChanged(int value)
		{
			if (NetworkServer.active || NetworkClient.active)
				return;
			Icon = value;
		}

		LocalSettings.Instance.DefaultLobbyName.Attach(DefaultNameChanged);
		LocalSettings.Instance.DefaultLobbyIconIndex.Attach(DefaultIconChanged);
	}

	public void SetLobbyName(string name)
	{
		if (name.Length > 32)
			name = name.Substring(0, 32);
		Name = name;
	}

	public void SetLobbyIcon(int icon)
	{
		Icon = icon;
	}

	public void CallOnNameChanged(string oldName, string newName)
	{
		OnNameChanged?.Invoke(newName);
	}

	public void CallOnIconChanged(int oldIcon, int newIcon)
	{
		OnIconChanged?.Invoke(GlobalSpriteList.GetSprite(newIcon));
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();

		OnNameChanged?.Invoke(Name);
		OnIconChanged?.Invoke(GlobalSpriteList.GetSprite(Icon));

		var uri = NetworkManager.singleton.transport.ServerUri();
		Address.Value = uri.Host + ":" + uri.Port;
	}

	public override void OnStopClient()
	{
		base.OnStopClient();

		Name = LocalSettings.Instance.DefaultLobbyName;
		Icon = LocalSettings.Instance.DefaultLobbyIconIndex;
	}

	private static readonly TextInputPopup.InputOptions LobbyNameOptions = new TextInputPopup.InputOptions
	{
		MaxLength = 32,
		Title = "Enter Lobby Name"
	};

	public void OnEditLobbyName()
	{
		LobbyNameOptions.InitialValue = Name;
		LobbyNameOptions.OnSuccess = Cmd_SetLobbyName;
		TextInputPopup.Show(LobbyNameOptions);
	}

	private static readonly SpriteInputPopup.InputOptions LobbyIconInputOptions = new SpriteInputPopup.InputOptions
	{
		Title = "Select Lobby Icon"
	};

	public void OnEditLobbyIcon()
	{
		LobbyIconInputOptions.currentValue = Icon;
		LobbyIconInputOptions.OnSuccess = Cmd_SetLobbyIcon;
		SpriteInputPopup.Show(LobbyIconInputOptions);
	}

	private void Update()
	{
		if (isClient)
		{
			LobbyRuntime.Value = "Runtime: " + Formatting.Time(LobbyNetworkManager.RunTime);
			// LobbyUptime.Value = Formatting.Time(LobbyNetworkManager.RunTime);
		}
	}


	[Command(requiresAuthority = false)]
	public void CmdSetLobbyName(string name)
	{
		Name = name;
	}
	public static void Cmd_SetLobbyName(string name) => LobbyNetworkManager.Instance.Info.CmdSetLobbyName(name);

	[Command(requiresAuthority = false)]
	public void CmdSetLobbyIcon(int icon)
	{
		Icon = icon;
	}
	public static void Cmd_SetLobbyIcon(int icon) => LobbyNetworkManager.Instance.Info.CmdSetLobbyIcon(icon);
}