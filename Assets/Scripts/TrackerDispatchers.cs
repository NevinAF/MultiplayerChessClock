using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TrackerDispatchers : MonoBehaviour
{
	public SDispatcher<bool> Valid;

	public SDispatcher<string> Name;
	public SDispatcher<Color> Color;
	public SDispatcher<string> Rank;
	public SDispatcher<int> IconIndex;

	[Space(6)]
	public SDispatcher<bool> TimerActive;
	[FormerlySerializedAs("HumanReadableTime")]
	public SDispatcher<string> HR_PhysicalTimeRemaining;
	public SDispatcher<string> HR_TurnTimeRemaining;
	public SDispatcher<Sprite> Icon;

	
	private float m_time;
	private double m_turnTime;

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
		disconnect += other.Rank.PipeTo(Rank);
		disconnect += other.IconIndex.PipeTo(IconIndex);
		disconnect += other.TimerActive.PipeTo(TimerActive);
		disconnect += other.HR_PhysicalTimeRemaining.PipeTo(HR_PhysicalTimeRemaining);
		disconnect += other.HR_TurnTimeRemaining.PipeTo(HR_TurnTimeRemaining);
		disconnect += other.Icon.PipeTo(Icon);
		pipedFrom = other;
	}

	public void UpdateFromSource(in TrackerNetworkData data)
	{
		if (pipedFrom != null)
		{
			UnityEngine.Debug.LogError("UpdateFromSource called while already piped from another source. This should not be possible.");
			return;
		}

		Valid.Value = data.Valid;

		if (!data.Valid)
			return;

		m_time = data.m_time;
		m_turnTime = data.m_turnTime;

		Name.Value = data.Name;
		Color.Value = (Color)data.Color;
		Rank.Value = data.Rank;
		IconIndex.Value = data.IconIndex;

		TimerActive.Value = data.TimerActive;
		Icon.Value = GlobalSpriteList.GetSprite(data.IconIndex);

		if (TimerActive.Value)
			UpdateTimers();
		else {
			HR_PhysicalTimeRemaining.Value = Formatting.Time(data.PhysicalTimeRemaining);
			float remain = data.TurnTimeRemaining;
			HR_TurnTimeRemaining.Value =
				remain == 0 ? "" :
				remain > 0 ? "+" + Formatting.Time(remain) :
				Formatting.Time(remain);
		}
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
		if (Icon != null && IconIndex != null)
			Icon.Value = GlobalSpriteList.GetSprite(IconIndex);
	}

	public void UpdateTimers()
	{
		if (pipedFrom != null)
		{
			UnityEngine.Debug.LogError("UpdateTimers called while already piped from another source. This should not be possible.");
			return;
		}

		if (!TimerActive.Value)
			return;

		float remain = (float)(m_turnTime - LobbyNetworkManager.CTime);

		if (remain >= 0)
		{
			HR_TurnTimeRemaining.Value = "+" + Formatting.Time(remain);
			HR_PhysicalTimeRemaining.Value = Formatting.Time(m_time);
		}
		else {
			HR_TurnTimeRemaining.Value = Formatting.Time(remain);
			HR_PhysicalTimeRemaining.Value = Formatting.Time(m_time + remain);
		}
	}
}
