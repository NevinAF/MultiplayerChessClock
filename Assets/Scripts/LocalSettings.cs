using UnityEngine;

public class LocalSettings : SingletonMono<LocalSettings>
{
	public SDispatcher<string> Name;
	public SDispatcher<int> IconIndex;

	public SDispatcher<Sprite> Icon;

	protected override void Awake()
	{
		base.Awake();

		Name.Value = PlayerPrefs.GetString("Name", SystemInfo.deviceName);
		IconIndex.Value = PlayerPrefs.GetInt("IconIndex", GlobalSpriteList.RandomIconIndex);

		IconIndex.PipeTo(Icon, GlobalSpriteList.GetSprite);
	}


	private static readonly TextInputPopup.InputOptions NameInputOptions = new TextInputPopup.InputOptions
	{
		MaxLength = 31,
		Title = "Enter Username",
		OnSuccess = (name) => {
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
}