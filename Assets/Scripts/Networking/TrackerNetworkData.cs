using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

public struct TrackerNetworkData
{
	public byte m_flags;
	[MarshalAs(UnmanagedType.LPStr, SizeConst = 20)]
	public string m_name;
	[MarshalAs(UnmanagedType.LPStr, SizeConst = 4)]
	public string m_rank;
	public ushort m_icon;
	public Color32 m_color;
	/// <summary>
	/// The time in seconds remaining.
	/// </summary>
	public float m_time;
	/// <summary>
	/// IF TimerActive, this represents the network time which the turn time will run out.
	/// </summary>
	public double m_turnTime;

	public TrackerNetworkData(
		string name,
		string rank,
		ushort iconIndex,
		Color32 color,
		float time
	) {
		m_flags = 1;
		m_name = name;
		m_rank = rank;
		m_icon = iconIndex;
		m_color = color;
		m_time = time;
		m_turnTime = 0;
	}

	public readonly bool Valid => (m_flags & 1) == 1;
	public void Invalidate() => m_flags &= 0xFE;
	public void Validate() => m_flags |= 1;

	public readonly bool TimerActive => (m_flags & 2) == 2;
	public void StartTimer()
	{
		if (TimerActive)
			return;

		// Should never have negative time when starting the timer.
		if (m_turnTime < 0) m_turnTime = 0;

		m_flags |= 2;
		m_turnTime += LobbyNetworkManager.RunTime;
	}
	public void StopTimer()
	{
		if (!TimerActive)
			return;

		m_flags &= 0xFD;

		// If more time has passed than on the turn timer, 
		m_turnTime -= LobbyNetworkManager.RunTime;
		if (m_turnTime < 0)
			m_time += (float)m_turnTime;
	}

	public readonly string Name => m_name;
	public void SetName(string value)
	{
		if (value.Length > 20)
		{
			UnityEngine.Debug.LogError("Name must be 20 characters or less");
			value = value.Substring(0, 20);
		}
		m_name = value;
	}

	public readonly string Rank => m_rank;
	public void SetRank(string value)
	{
		if (value.Length > 4)
		{
			UnityEngine.Debug.LogError("Rank must be 4 characters or less");
			value = value.Substring(0, 4);
		}
		m_rank = value;
	}

	public readonly ushort IconIndex => m_icon;
	public void SetIconIndex(ushort value) => m_icon = value;

	public readonly Color32 Color => m_color;
	public void SetColor(Color32 value) => m_color = value;

	public readonly float PhysicalTimeRemaining => TimerActive && m_turnTime < LobbyNetworkManager.RunTime
		? m_time + (float)(m_turnTime - LobbyNetworkManager.RunTime)
		: m_time;
	public void SetPhysicalTimeRemaining(float value)
	{
		if (TimerActive && m_turnTime < LobbyNetworkManager.RunTime)
			m_time = value - (float)(m_turnTime - LobbyNetworkManager.RunTime);
		else m_time = value;
	}
	public void IncPhysicalTimeRemaining(float value) => m_time += value;

	public readonly float TurnTimeRemaining => TimerActive
		? (float)(m_turnTime - LobbyNetworkManager.RunTime)
		: (float)m_turnTime;
	public void SetTurnTimeRemaining(float value)
	{
		if (TimerActive)
		{
			if (m_turnTime < LobbyNetworkManager.RunTime)
				m_time += (float)(m_turnTime - LobbyNetworkManager.RunTime);
			m_turnTime = LobbyNetworkManager.RunTime + value;
		}
		else m_turnTime = value;
	}

	public override string ToString()
	{
		return $"{{{(Valid ? "V" : "I")} {m_name} {m_rank} {string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:X2}{1:X2}{2:X2}", m_color.r, m_color.g, m_color.b)} icon_{m_icon} {(TimerActive ? "On" : "Off")}_{m_time:00}+{TurnTimeRemaining:00}}}";
	}
}