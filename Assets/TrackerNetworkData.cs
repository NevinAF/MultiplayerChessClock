using System;
using System.Runtime.InteropServices;
using UnityEngine;

public struct TrackerNetworkData
{
	[MarshalAs(UnmanagedType.LPStr, SizeConst = 20)]
	public string name;

	[MarshalAs(UnmanagedType.LPStr, SizeConst = 4)]
	public string rank;
	
	public Color32 color32;

	public double setTime;

	public double TimeRemaining
	{
		readonly get => TimerActive ? setTime - LobbyNetworkManager.CTime : setTime;
		set {
			long activeBit = BitConverter.DoubleToInt64Bits(setTime) & 1L;
			setTime = BitConverter.Int64BitsToDouble((BitConverter.DoubleToInt64Bits(
				activeBit == 1L ? value + LobbyNetworkManager.CTime : value
			) & ~1L) | activeBit);
		}
	}
	public bool TimerActive
	{
		readonly get => (BitConverter.DoubleToInt64Bits(setTime) & 1L) == 1L;
		set => setTime = CreateSetTime(TimeRemaining, value);
	}

	public int iconIndex;
	// bool active : 1; short x : 13; short y : 13; byte size : 5;
	public int active_xy_size;

	public bool Valid
	{
		readonly get => (active_xy_size & 1) == 1;
		set => active_xy_size = value ? active_xy_size | 1 : active_xy_size & ~1;
	}

	public ushort X
	{
		readonly get => (ushort)((active_xy_size >> 1) & 0x1FFF);
		set => active_xy_size = (active_xy_size & 1) | (value << 1) | (active_xy_size & ~0x3FFF);
	}

	public ushort Y
	{
		readonly get => (ushort)((active_xy_size >> 14) & 0x1FFF);
		set => active_xy_size = (active_xy_size & 0x3FFF) | (value << 14);
	}

	public byte Size
	{
		readonly get => (byte)((active_xy_size >> 27) & 0x1F);
		set => active_xy_size = (active_xy_size & 0x7FFFFFF) | (value << 27);
	}

	public static double CreateSetTime(double time, bool active)
	{
		long activeBit = active ? 1L : 0L;
		return BitConverter.Int64BitsToDouble((BitConverter.DoubleToInt64Bits(
			activeBit == 1L ? time + LobbyNetworkManager.CTime : time
		) & ~1L) | activeBit);
	}

	public static bool IsSetTimeActive(double setTime)
	{
		return (BitConverter.DoubleToInt64Bits(setTime) & 1L) == 1L;
	}

	public static double GetSetTimeRemaining(double setTime)
	{
		return IsSetTimeActive(setTime) ? setTime - LobbyNetworkManager.CTime : setTime;
	}


	public override string ToString()
	{
		return $"{{{(Valid ? "V" : "I")} {name} {rank} {color32} icon{iconIndex} {TimerActive}_{TimeRemaining} {X} {Y} {Size}}}";
	}
}