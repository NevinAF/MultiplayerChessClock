using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TrackerDispatchers : MonoBehaviour
{
	public SDispatcher<bool> Valid;

	public SDispatcher<string> Name;
	public SDispatcher<Color> Color;
	public SDispatcher<double> SetTime;
	public SDispatcher<string> Rank;
	public SDispatcher<int> IconIndex;

	[Space(6)]
	public SDispatcher<bool> TimerActive;
	public SDispatcher<string> HumanReadableTime;
	public SDispatcher<Sprite> Icon;

	protected TrackerDispatchers pipedFrom = null;
	private Action disconnect;

	public void PipeFrom(byte networkID)
	{
		TrackerPresenter presenter = TrackerManager.GetTracker(networkID);
		PipeFrom(presenter.Dispatcher);
	}

	public void PipeFrom(TrackerDispatchers other)
	{
		if (pipedFrom == other)
			return;

		ClearData();

		disconnect += other.Valid.PipeTo(Valid);
		disconnect += other.Name.PipeTo(Name);
		disconnect += other.Color.PipeTo(Color);
		disconnect += other.SetTime.PipeTo(SetTime);
		disconnect += other.Rank.PipeTo(Rank);
		disconnect += other.IconIndex.PipeTo(IconIndex);
		disconnect += other.TimerActive.PipeTo(TimerActive);
		disconnect += other.HumanReadableTime.PipeTo(HumanReadableTime);
		disconnect += other.Icon.PipeTo(Icon);
		pipedFrom = other;
	}

	public void UpdateFromNetwork(byte networkID)
	{
		if (pipedFrom != null)
		{
			UnityEngine.Debug.LogError("UpdateFromNetwork called while already piped from another source. This should not be possible.");
			return;
		}

		var data = LobbyNetworkManager.GetTrackerData(networkID);

		Valid.Value = data.Valid;

		if (!data.Valid)
			return;

		Name.Value = data.name;
		SetTime.Value = data.setTime;
		Color.Value = (Color)data.color32;
		Rank.Value = data.rank;
		IconIndex.Value = data.iconIndex;

		TimerActive.Value = data.TimerActive;
		Icon.Value = GlobalSpriteList.GetSprite(data.iconIndex);
		HumanReadableTime.Value = Formatting.Time(data.TimeRemaining);
	}

	public void ClearData()
	{
		disconnect?.Invoke();
		disconnect = null;
		pipedFrom = null;
	}

	public void OnDestroy()
	{
		ClearData();
	}

	private void OnValidate()
	{
		if (HumanReadableTime != null && SetTime != null)
			HumanReadableTime.Value = Formatting.Time(SetTime);

		if (Icon != null && IconIndex != null)
			Icon.Value = GlobalSpriteList.GetSprite(IconIndex);
	}
}
