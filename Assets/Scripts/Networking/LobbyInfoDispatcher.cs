using Edgegap;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class LobbyInfoDispatcher : SingletonMono<LobbyInfoDispatcher>
{
	public SDispatcher<string> Name;
	public SDispatcher<int> IconIndex;
	public SDispatcher<string> Address;
	public SDispatcher<int> PlayerCount;

	public SDispatcher<string> LobbyRuntime;

	public SDispatcher<Sprite> Icon;
	// public SDispatcher<string> LobbyUptime;

	protected override void Awake()
	{
		base.Awake();

		
		LobbyNetworkManager.OnNameChanged += Name.SetValue;
		LobbyNetworkManager.OnIconChanged += IconIndex.SetValue;

		Player.OnPlayerAdded += _ => PlayerCount.Value = Player.Players.Count;
		Player.OnPlayerRemoved += _ => PlayerCount.Value = Player.Players.Count;


		IconIndex.PipeTo(Icon, GlobalSpriteList.GetSprite);
	}

	private static readonly TextInputPopup.InputOptions LobbyNameOptions = new TextInputPopup.InputOptions
	{
		MaxLength = 32,
		Title = "Enter Lobby Name"
	};

	public void OnEditLobbyName()
	{
		LobbyNameOptions.InitialValue = Name;
		LobbyNameOptions.OnSuccess = LobbyNetworkManager.Cmd_ChangeLobbyName;
		TextInputPopup.Show(LobbyNameOptions);
	}

	private static readonly SpriteInputPopup.InputOptions LobbyIconInputOptions = new SpriteInputPopup.InputOptions
	{
		Title = "Select Lobby Icon"
	};

	public void OnEditLobbyIcon()
	{
		LobbyIconInputOptions.currentValue = IconIndex;
		LobbyIconInputOptions.OnSuccess = LobbyNetworkManager.Cmd_ChangeLobbyIcon;
		SpriteInputPopup.Show(LobbyIconInputOptions);
	}

	private void Update()
	{
		if (NetworkClient.active)
		{
			LobbyRuntime.Value = "Runtime: " + Formatting.Time(LobbyNetworkManager.RunTime);
		}
	}
}