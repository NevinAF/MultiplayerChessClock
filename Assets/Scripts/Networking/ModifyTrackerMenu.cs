using System;
using System.Collections.Generic;
using Edgegap;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ModifyTrackerMenu : SingletonMono<ModifyTrackerMenu>
{
	public PrefabLayoutList<TrackerButtonPreview> Buttons;

	public TrackerDispatchers Dispatchers;

	public PrefabLayoutList<TrackerDispatchers> TrackerSelection;

	private int contextNetworkID = int.MinValue;
	private int saved_context = int.MinValue;

	protected override void Awake()
	{
		base.Awake();
		Dispatchers.Valid.Value = false;
	}

	private TrackerDispatchers InitializeTrackerSelection(bool setPipe)
	{
		int index = TrackerSelection.Count;
		var instance = TrackerSelection.Initialize();
		instance.transform.SetSiblingIndex(instance.transform.parent.childCount - 2);
		var button = instance.GetComponentInChildren<Button>(true);
		button.enabled = true;
		button.onClick.AddListener(() => SetContext(index));
		if (setPipe)
			instance.PipeFrom((byte)index);
		return instance;
	}

	private void OnEnable()
	{
		LobbyNetworkManager.OnTrackersChange += OnNetworkTrackerChange;
		LobbyNetworkManager.OnActionsChange += OnNetworkActionChange;

		SetContext(saved_context);

		while (TrackerSelection.Count < LobbyNetworkManager.TrackerCount)
			InitializeTrackerSelection(false);

		for (int i = 0; i < LobbyNetworkManager.TrackerCount; i++)
			TrackerSelection[i].PipeFrom((byte)i);
	}

	public void SetContext(int networkID)
	{
		if (contextNetworkID == networkID)
			return;

		if (networkID < 0 || networkID > byte.MaxValue)
		{
			contextNetworkID = int.MinValue;
			Dispatchers.ClearData();
			Dispatchers.Valid.Value = false;
			OnNetworkActionChange(SyncList<TrackerActionNetworkData>.Operation.OP_CLEAR, 0, default);
		}
		else {
			contextNetworkID = networkID;
			Dispatchers.PipeFrom((byte)networkID);
			LobbyNetworkManager.ReinitializeActionCallback(OnNetworkActionChange);
		}
	}

	private void OnDisable()
	{
		LobbyNetworkManager.OnTrackersChange -= OnNetworkTrackerChange;
		LobbyNetworkManager.OnActionsChange -= OnNetworkActionChange;

		saved_context = contextNetworkID;
		SetContext(int.MinValue);

		for (int i = 0; i < TrackerSelection.Count; i++)
			TrackerSelection[i].ClearData();
	}

	public void OnNetworkTrackerChange(SyncList<TrackerNetworkData>.Operation op, int index, TrackerNetworkData data)
	{
		if (op == SyncList<TrackerNetworkData>.Operation.OP_CLEAR)
		{
			for (int i = LobbyNetworkManager.TrackerCount; i < TrackerSelection.Count; i++)
				TrackerSelection[i].ClearData();
			return;
		}

		if (op == SyncList<TrackerNetworkData>.Operation.OP_SET)
		{
			if (contextNetworkID == int.MinValue + 2)
			{
				bool newValid = LobbyNetworkManager.GetTrackerData(index).Valid;
				if (newValid != data.Valid)
					SetContext(index);
			}
		}
		else if (op == SyncList<TrackerNetworkData>.Operation.OP_ADD)
		{
			while (TrackerSelection.Count <= index)
				InitializeTrackerSelection(true);

			if (contextNetworkID == int.MinValue + 2)
				SetContext(index);
		}
		else throw new InvalidOperationException("Invalid operation: " + op);

		
		if (contextNetworkID < 0 && contextNetworkID >= -(byte.MaxValue + 1) && contextNetworkID != -(index + 1))
		{
			var source = LobbyNetworkManager.GetTrackerData((-contextNetworkID) - 1);
			var newData = LobbyNetworkManager.GetTrackerData(index);

			if (source.IconIndex == newData.IconIndex && source.Color.Equals(newData.Color))
				SetContext(index);
		}
	}

	public void OnNetworkActionChange(SyncList<TrackerActionNetworkData>.Operation op, int index, TrackerActionNetworkData data)
	{
		void SpawnButton()
		{
			int thisIndex = Buttons.Count;
			TrackerButtonPreview spawn = Buttons.Initialize();
			spawn.OnClick(() => OnAddActionTo(thisIndex));
			spawn.Name.Value = thisIndex == 0 ? "Primary" : "Button #" + thisIndex;
		}

		if (op == SyncList<TrackerActionNetworkData>.Operation.OP_CLEAR)
		{
			if (Buttons.Count == 0)
			{
				SpawnButton();
				SpawnButton();
			}
			else for (int i = 0; i < Buttons.Count; i++)
			{
				Buttons[i].ClearActions();
				if (i >= 2)
					Buttons[i].gameObject.SetActive(false);
			}
			return;
		}

		if (data.attachedTracker != contextNetworkID)
			return;

		int buttonIndex = data.buttonIndex;
		while (Buttons.Count <= buttonIndex + 1)
			SpawnButton();
		for (int i = 2; i <= buttonIndex + 1; i++)
			Buttons[i].gameObject.SetActive(true);

		TrackerButtonPreview button = Buttons[buttonIndex];

		switch (op)
		{
			case SyncList<TrackerActionNetworkData>.Operation.OP_ADD:
			case SyncList<TrackerActionNetworkData>.Operation.OP_INSERT:
			{
				var action = button.CreateAction(index, data);
				action.Dispatcher.AddOnEditAction(() => {
					OnEditAction(LobbyNetworkManager.GetActionIndex(action.transform.GetSiblingIndex() - 1, action.NetworkData));
				});
				break;
			}
			case SyncList<TrackerActionNetworkData>.Operation.OP_REMOVEAT:
			{
				button.DestroyAction(index, data);
				break;
			}
			case SyncList<TrackerActionNetworkData>.Operation.OP_SET:
			{
				button.DestroyAction(index, data);
				goto case SyncList<TrackerActionNetworkData>.Operation.OP_ADD;
			}
		}
	}

	public void OnCreateNew()
	{
		SetContext(int.MinValue);
		contextNetworkID = int.MinValue + 2; // Special value to indicate that the next tracker added should be selected.
		LobbyNetworkManager.Cmd_AddNewTracker();
	}

	public void OnDuplicateContext()
	{
		int current = contextNetworkID;
		SetContext(int.MinValue);
		contextNetworkID = -(current + 1); // Special value to indicate that the next tracker change that matches this tracker should be selected.
		LobbyNetworkManager.Cmd_DuplicateTracker(current);
	}

	private static NumberPadPopup.InputOptions TimeRemainingOptions = new NumberPadPopup.InputOptions
	{
		Title = "Edit Time Remaining",
		Type = NumberPadPopup.InputType.Duration,
		InitialText = null,
		MaxValue = int.MaxValue,
		OnSuccess = value => {
			value *= (int)Math.Pow(60, NumberPadPopup.Instance.UnitType);
			NumberPadPopup.Instance.Result.Value = value;
			if (Instance.contextNetworkID >= 0)
				LobbyNetworkManager.Cmd_ChangeTracker_PhysicalTimeRemaining(Instance.contextNetworkID, value);
		}
	};

	public void OnEditTimeRemaining()
	{
		// TimeRemainingOptions.InitialText = TrackerNetworkData.GetSetTimeRemaining(Dispatchers.SetTime).ToString("0");
		NumberPadPopup.Show(TimeRemainingOptions);
	}

	public void OnToggleTimerActive() => LobbyNetworkManager.Cmd_ChangeTracker_TimerActive(contextNetworkID, !Dispatchers.TimerActive.Value);

	private static TextInputPopup.InputOptions NameOptions = new TextInputPopup.InputOptions
	{
		Title = "Edit Name",
		MaxLength = 20,
		OnSuccess = value => {
			if (Instance.contextNetworkID >= 0)
				LobbyNetworkManager.Cmd_ChangeTracker_Name(Instance.contextNetworkID, value);
		}
	};

	public void OnEditName()
	{
		NameOptions.InitialValue = Dispatchers.Name.Value;
		TextInputPopup.Show(NameOptions);
	}

	
	private static TextInputPopup.InputOptions RankOptions = new TextInputPopup.InputOptions
	{
		Title = "Edit Rank",
		MaxLength = 3,
		OnSuccess = value => {
			if (Instance.contextNetworkID >= 0)
				LobbyNetworkManager.Cmd_ChangeTracker_Name(Instance.contextNetworkID, value);
		}
	};

	public void OnEditRank()
	{
		RankOptions.InitialValue = Dispatchers.Rank.Value;
		TextInputPopup.Show(RankOptions);
	}

	private static ColorWheelPopup.InputOptions ColorOptions = new ColorWheelPopup.InputOptions
	{
		Title = "Edit Color",
		OnSuccess = value => {
			if (Instance.contextNetworkID >= 0)
				LobbyNetworkManager.Cmd_ChangeTracker_Color(Instance.contextNetworkID, value);
		}
	};

	public void OnEditColor()
	{
		ColorOptions.InitialColor = Dispatchers.Color.Value;
		ColorWheelPopup.Show(ColorOptions);
	}

	public void OnDeleteContext()
	{
		ConfirmationPanel.Instance.Show("Delete Tracker?", "Are you sure you want to delete this tracker?", "Delete", () => {
			if (contextNetworkID >= 0)
				LobbyNetworkManager.Cmd_RemoveTracker(contextNetworkID);
		});
	}

	private static SpriteInputPopup.InputOptions IconOptions = new SpriteInputPopup.InputOptions
	{
		Title = "Edit Icon",
		OnSuccess = value => {
			if (Instance.contextNetworkID >= 0)
				LobbyNetworkManager.Cmd_ChangeTracker_IconIndex(Instance.contextNetworkID, value);
		}
	};

	public void OnEditIcon()
	{
		IconOptions.currentValue = Dispatchers.IconIndex.Value;
		SpriteInputPopup.Show(IconOptions);
	}

	private static ActionInputPopup.InputOptions ActionOptions = new ActionInputPopup.InputOptions
	{
		Title = "Add Action",
		OnDelete = index => LobbyNetworkManager.Cmd_RemoveAction(index),
		OnSuccess = _ => {
			TrackerActionNetworkData data = ActionInputPopup.ResultNetworkData;
			if (ActionInputPopup.Instance.EditTarget >= 0) {
				LobbyNetworkManager.Cmd_ChangeAction(ActionInputPopup.Instance.EditTarget, data);
			}
			else LobbyNetworkManager.Cmd_AddAction(data);
		}
	};

	public void OnEditAction(int index)
	{
		ActionOptions.editTarget = index;
		ActionInputPopup.Show(ActionOptions);
	}

	public void OnAddActionTo(int index)
	{
		UnityEngine.Debug.Log("Adding action to button " + index);
		if (contextNetworkID < 0)
			return;

		ActionOptions.editTarget = -1;
		ActionOptions.attachedTracker = (byte)contextNetworkID;
		ActionOptions.buttonIndex = (byte)index;
		ActionInputPopup.Show(ActionOptions);
	}

	public void ViewingValidChanged(bool valid)
	{
		if (!valid)
			SetContext(int.MinValue);
	}
}
