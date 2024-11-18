using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TrackerInputPopup : InputPopup<TrackerInputPopup, byte, TrackerInputPopup.InputOptions, TrackerPopupQuickItem>
{
	public class InputOptions : InputPopupShowOptions<byte>
	{
		public int currentValue;
	}

	private bool valid = false;
	private int currentValue;

	protected override void OnShow(InputOptions options)
	{
		if (TrackerManager.Instance.ValidTrackerCount == 0)
		{
			UnityEngine.Debug.LogError("No valid trackers to attach to! The ActionInputPopup should never be shown when there are no valid trackers!");
			return;
		}

		valid = false;
		currentValue = options.currentValue;

		for (int i = 0; i < TrackerManager.Instance.ValidTrackerCount; i++)
			AddQuickFillEntry((byte)i, InputQuickFillIconType.Other);

		LobbyNetworkManager.OnTrackersChange += OnTrackersChanged;
	}

	protected override bool ResultValid => valid;

	private void OnDisable()
	{
		for (int i = 0; i < QuickFillList.Count; i++)
			QuickFillList[i].ClearData();

		LobbyNetworkManager.OnTrackersChange -= OnTrackersChanged;
	}

	private void OnTrackersChanged(SyncList<TrackerNetworkData>.Operation op, int index, TrackerNetworkData data)
	{
		if (op == SyncList<TrackerNetworkData>.Operation.OP_ADD)
			AddQuickFillEntry((byte)index, InputQuickFillIconType.Other);
	}

	protected override int ValueSortScore(byte value) => throw new Exception("Results should never be valid and this should never be called.");

	protected override void QuickFillClicked(byte value)
	{
		Result.Value = value;
		valid = true;
		OnSubmit();
		valid = false;
	}

	protected override void SetQuickFillEntry(TrackerPopupQuickItem item, byte value)
	{
		item.SetData(value);
		item.IsFocused.Value = value == currentValue;
	}
}
