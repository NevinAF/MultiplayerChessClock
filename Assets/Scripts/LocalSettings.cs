using UnityEngine;

public class LocalSettings : SingletonMono<LocalSettings>
{
	public SDispatcher<string> Name;
	public SDispatcher<int> IconIndex;
	public SDispatcher<string> DefaultLobbyName;
	public SDispatcher<int> DefaultLobbyIconIndex;

	public SDispatcher<Sprite> Icon;
	public SDispatcher<Sprite> DefaultLobbyIcon;

	private static string NameAsLobby(string name) => name.Length > 24 ? name.Substring(0, 24) + "'s Lobby" : name + "'s Lobby";

	protected override void Awake()
	{
		base.Awake();

		Name.Value = PlayerPrefs.GetString("Name", SystemInfo.deviceName);
		IconIndex.Value = PlayerPrefs.GetInt("IconIndex", GlobalSpriteList.RandomIconIndex);
		DefaultLobbyName.Value = PlayerPrefs.GetString("DefaultLobbyName", NameAsLobby(Name.Value));
		DefaultLobbyIconIndex.Value = PlayerPrefs.GetInt("DefaultLobbyIconIndex", GlobalSpriteList.RandomIconIndex);

		IconIndex.PipeTo(Icon, GlobalSpriteList.GetSprite);
		DefaultLobbyIconIndex.PipeTo(DefaultLobbyIcon, GlobalSpriteList.GetSprite);
	}


	private static readonly TextInputPopup.InputOptions NameInputOptions = new TextInputPopup.InputOptions
	{
		MaxLength = 31,
		Title = "Enter Username",
		OnSuccess = (name) => {
			if (Instance.DefaultLobbyName.Value == NameAsLobby(Instance.Name.Value))
				Instance.DefaultLobbyName.Value = NameAsLobby(name);
			Instance.Name.Value = name;
			PlayerPrefs.SetString("Name", name);
			PlayerPrefs.Save();
		}
	};

	public void OnEditName()
	{
		NameInputOptions.InitialValue = Name.Value;
		TextInputPopup.Show(NameInputOptions);
	}

	private static readonly SpriteInputPopup.InputOptions IconInputOptions = new SpriteInputPopup.InputOptions
	{
		Title = "Select Icon",
		OnSuccess = (index) => {
			Instance.IconIndex.Value = index;
			PlayerPrefs.SetInt("IconIndex", index);
			PlayerPrefs.Save();
		}
	};

	public void OnEditIcon()
	{
		IconInputOptions.currentValue = IconIndex.Value;
		SpriteInputPopup.Show(IconInputOptions);
	}

	private static readonly TextInputPopup.InputOptions DefaultLobbyNameOptions = new TextInputPopup.InputOptions
	{
		MaxLength = 32,
		Title = "Enter Default Lobby Name",
		OnSuccess = (address) => {
			Instance.DefaultLobbyName.Value = address;
			PlayerPrefs.SetString("DefaultLobbyName", address);
			PlayerPrefs.Save();
		}
	};

	public void OnEditDefaultLobbyName()
	{
		DefaultLobbyNameOptions.InitialValue = DefaultLobbyName.Value;
		TextInputPopup.Show(DefaultLobbyNameOptions);
	}

	private static readonly SpriteInputPopup.InputOptions DefaultLobbyIconInputOptions = new SpriteInputPopup.InputOptions
	{
		Title = "Select Default Lobby Icon",
		OnSuccess = (index) => {
			Instance.DefaultLobbyIconIndex.Value = index;
			PlayerPrefs.SetInt("DefaultLobbyIconIndex", index);
			PlayerPrefs.Save();
		}
	};

	public void OnEditDefaultLobbyIcon()
	{
		DefaultLobbyIconInputOptions.currentValue = DefaultLobbyIconIndex.Value;
		SpriteInputPopup.Show(DefaultLobbyIconInputOptions);
	}
}