using System;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRecoverEntry : MonoBehaviour
{
	public SDispatcher<string> Name;
	public SDispatcher<string> Address;
	public SDispatcher<string> Date;
	public SDispatcher<Sprite> Icon;
	public SDispatcher<string> TrackerCount;
	public SDispatcher<string> ActionCount;
	public SDispatcher<string> Runtime;

	public SDispatcher<bool> Invalid;

	public Sprite InvalidIcon;

	public Button Recover;
	public Button Delete;

	private DateTime time;

	public void SetData(LobbySaves.Entry data)
	{
		Invalid.Value = data.Address == "Unknown" || data.Date == default || data.TrackerCount == -1 || data.ActionCount == -1 || data.LobbyIcon == -1 || data.Runtime == default || data.Version != LobbySaves.Entry.CurrentVersion;

		Name.Value = data.LobbyName;
		Address.Value = data.Address;
		if (data.Date == default)
		{
			Date.Value = "? Date Unknown ?";
			time = default;
		}
		else time = data.Date;
		Icon.Value = data.LobbyIcon == -1 ? InvalidIcon : GlobalSpriteList.GetSprite(data.LobbyIcon);
		TrackerCount.Value = data.TrackerCount == -1 ? "???" : data.TrackerCount.ToString();
		ActionCount.Value = data.ActionCount == -1 ? "???" : data.ActionCount.ToString();
		Runtime.Value = "Runtime: "+ (data.Runtime == default ? "???" : Formatting.Time(data.Runtime.TotalSeconds));
	}

	public void Update()
	{
		if (time != default)
		{
			TimeSpan timeSince = DateTime.Now - time;
			if (timeSince.TotalDays >= 1) {
				Date.Value = time.ToString("yyyy-MM-dd HH:mm:ss");
				time = default;
			}
			else Date.Value = Formatting.Time(timeSince.TotalSeconds) + " ago";
		}
	}
}