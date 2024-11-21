using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public struct SerializedActionData
{
	public byte type;
	public byte target;
	public ushort data;

	public TrackerActionNetworkData ToNetworkData() => new TrackerActionNetworkData
	{
		type = type,
		target = target,
		data = data
	};
}

public class ActionInputPopup : InputPopup<ActionInputPopup, SerializedActionData, ActionInputPopup.InputOptions, ActionPopupQuickItem>
{
	public static readonly List<int> emptyList = new List<int>();

	public class InputOptions : InputPopupShowOptions<SerializedActionData>
	{
		public int editTarget;
		public byte buttonIndex;
		public byte attachedTracker;

		public Action<int> OnDelete;
	}

	public static TrackerActionNetworkData ResultNetworkData => new TrackerActionNetworkData
	{
		attachedTracker = Instance.AttachedID,
		buttonIndex = (byte)Instance.ButtonIndex.Value,
		type = Instance.Result.Value.type,
		target = Instance.Result.Value.target,
		data = Instance.Result.Value.data
	};

	public int EditTarget { get; private set; }

	protected override void OnShow(InputOptions options)
	{
		if (TrackerManager.Instance.ValidTrackerCount == 0)
		{
			UnityEngine.Debug.LogError("No valid trackers to attach to! The ActionInputPopup should never be shown when there are no valid trackers!");
			return;
		}

		EditTarget = options.editTarget;
		OnDelete = options.OnDelete;
		if (options.editTarget >= 0)
		{
			var data = LobbyNetworkManager.GetTrackerActionData(options.editTarget);
			Result.Value = new SerializedActionData
			{
				type = data.type,
				target = data.target,
				data = data.data
			};
			ButtonIndex.Value = data.buttonIndex;
			AttachedID = data.attachedTracker;

			EditTargetValid.Value = true;
			TypeValid.Value = true;
		}
		else {
			Result.Value = new SerializedActionData
			{
				type = 0,
				target = options.attachedTracker,
				data = 0
			};
			ButtonIndex.Value = options.buttonIndex;
			AttachedID = options.attachedTracker;

			EditTargetValid.Value = false;
			TypeValid.Value = false;
		}

		TargetTracker.PipeFrom(Result.Value.target);
		AttachedTracker.PipeFrom(AttachedID);

		UpdateMaxButtonIndex();
		LobbyNetworkManager.OnActionsChange += UpdateMaxButtonIndex;

		UpdateQuickEntries();
	}


	protected override void Start()
	{
		base.Start();
		for (int i = 0; i < TypeDataInput.Length; i++)
			TypeDataInput[i].SetData = SetData;

		ButtonIndex.PipeTo(ButtonName, x => x == 0 ? "Primary" : $"Button #{x}");
	}

	private void OnDisable()
	{
		LobbyNetworkManager.OnActionsChange -= UpdateMaxButtonIndex;
	}

	protected override bool ResultValid =>
		TypeValid &&
		TargetTracker.Valid.Value &&
		AttachedTracker.Valid.Value &&
		TypeDataInput[Result.Value.type - 1].IsDataValid();

	public Color DropdownFocusColor;
	public Color DropdownNormalColor;

	public byte AttachedID;
	public SDispatcher<int> ButtonIndex;
	public SDispatcher<string> ButtonName;
	public Action<int> OnDelete;

	// public SDispatcher<bool> ButtonIndexIncrementValid;
	// public SDispatcher<bool> ButtonIndexDecrementValid;
	public SDispatcher<bool> EditTargetValid;
	public SDispatcher<bool> TypeValid;


	public SDispatcher<string> TypeName;

	public TrackerDispatchers TargetTracker;
	public TrackerDispatchers AttachedTracker;

	public PrefabLayoutList<ButtonIndexInput> ButtonIndexInputs;

	public void SetType(int value)
	{
		var data = Result.Value;
		data.type = (byte)value;

		if (value == 0) {
			TypeValid.Value = false;
		}
		else {
			TypeValid.Value = true;

			if (TypeDataInput[value - 1].action.Initialized)
			{
				data.data = TypeDataInput[value - 1].action.NetworkData.data;
			}
			else {
				data.data = TypeDataInput[value - 1].DefaultData;
			}
		}

		Result.Value = data;
		UpdatePreview();
	}

	public void SetData(ushort value)
	{
		var data = Result.Value;
		data.data = value;
		Result.Value = data;
		if (gameObject.activeSelf)
			UpdatePreview();
	}

	public void OnAddSubmit()
	{
		EditTarget = -1;
		OnSubmit();
	}

	public void OnEditSubmit()
	{
		OnSubmit();
	}

	public void OnDeleteSubmit()
	{
		if (EditTarget >= 0)
			OnDelete?.Invoke(EditTarget);
		OnCancel();
	}

	// public int maxButtonIndex = 0;

	private void UpdateMaxButtonIndex(SyncList<TrackerActionNetworkData>.Operation op, int index, TrackerActionNetworkData data) => 
		UpdateMaxButtonIndex();
	private void UpdateMaxButtonIndex()
	{
		int maxButtonIndex = 0;
		for (int i = 0; i < LobbyNetworkManager.ActionCount; i++)
		{
			var data = LobbyNetworkManager.GetTrackerActionData(i);
			if (data.attachedTracker == AttachedID)
				maxButtonIndex = Math.Max(maxButtonIndex, data.buttonIndex);
		}

		if (maxButtonIndex >= byte.MaxValue)
			maxButtonIndex = byte.MaxValue - 1;

		for (int i = 0; i <= maxButtonIndex; i++)
		{
			if (ButtonIndexInputs.Count <= i)
			{
				int index = i + 1;
				var input = ButtonIndexInputs.Initialize();
				input.SetIndex(index);
				input.Button.onClick.AddListener(() => ButtonIndex.Value = index);
				ButtonIndex.Attach(input.IntToBoolEvent.SendEvent);
			}
			else ButtonIndexInputs[i].gameObject.SetActive(true);
		}

		for (int i = maxButtonIndex + 1; i < ButtonIndexInputs.Count; i++)
			ButtonIndexInputs[i].gameObject.SetActive(false);

		ButtonIndex.Value = Math.Min(ButtonIndex, (byte)(maxButtonIndex + 1));
		// ButtonIndexIncrementValid.Value = ButtonIndex < maxButtonIndex;
	}

	public TrackerActionDataInput[] TypeDataInput;

	protected override void UpdatePreview()
	{
		SerializedActionData result = Result;
		for (int i = 1; i <= TypeDataInput.Length; i++)
		{
			if (i == result.type)
			{
				TypeDataInput[i - 1].gameObject.SetActive(true);
			}
			else TypeDataInput[i - 1].gameObject.SetActive(false);
		}

		// ButtonIndexIncrementValid.Value = ButtonIndex < maxButtonIndex;
		// ButtonIndexDecrementValid.Value = ButtonIndex > 0;

		if (result.type != 0)
		{
			TypeDataInput[result.type - 1].action.Dispatcher.SetNetworkData(ResultNetworkData);
			TypeDataInput[result.type - 1].UpdatePreview();
			TypeDataInput[result.type - 1].action.ApplySolo(emptyList);
			TypeName.Value = TypeDataInput[result.type - 1].Name;
		}
		else {
			TypeName.Value = "<Select Action Type>";
		}

		base.UpdatePreview();
	}

	protected override int ValueSortScore(SerializedActionData value)
	{
		SerializedActionData result = Result;
		int score = 0;

		if (result.type != 0)
		{
			if (result.type != value.type)
				score += ushort.MaxValue;
		
			score += Math.Abs((short)result.data - (short)value.data);
		}


		if (result.target != value.target)
			score += ushort.MaxValue * 2;

		return score;
	}

	protected override void QuickFillClicked(SerializedActionData value)
	{
		if (value.type == 0)
		{
			value.type = value.target;
			value.target = AttachedID;
		}

		Result.Value = value;
		TypeValid.Value = true;
	}

	protected override void SetQuickFillEntry(ActionPopupQuickItem item, SerializedActionData value)
	{
		if (value.type == 0)
		{
			value.type = value.target;
			value.target = AttachedID;
		}

		item.SetData(value.ToNetworkData());
	}

	public readonly static TrackerInputPopup.InputOptions attachedPopupOptions = new TrackerInputPopup.InputOptions
	{
		Title = "Select Attached Tracker",
		OnSuccess = target => {
			Instance.AttachedID = target;
			Instance.AttachedTracker.PipeFrom(target);
			Instance.UpdateMaxButtonIndex();
			Instance.UpdatePreview();
		}
	};

	public void OpenAttachedPopup()
	{
		attachedPopupOptions.currentValue = AttachedID;
		TrackerInputPopup.Show(attachedPopupOptions);
	}

	public readonly static TrackerInputPopup.InputOptions targetPopupOptions = new TrackerInputPopup.InputOptions
	{
		Title = "Select Target Tracker",
		OnSuccess = target => {
			var data = Instance.Result.Value;
			data.target = target;
			Instance.Result.Value = data;
			Instance.TargetTracker.PipeFrom(target);
			Instance.UpdatePreview();
		},
	};

	public void OpenTargetPopup()
	{
		targetPopupOptions.currentValue = Result.Value.target;
		TrackerInputPopup.Show(targetPopupOptions);
	}

	public void OnSetButtonIndex(int index)
	{
		ButtonIndex.Value = (byte)index;
	}

	// public void OnIncrementButtonIndex()
	// {
	// 	if (ButtonIndex < maxButtonIndex)
	// 		ButtonIndex.Value++;
	// 	UpdatePreview();
	// }

	// public void OnDecrementButtonIndex()
	// {
	// 	if (ButtonIndex > 0)
	// 		ButtonIndex.Value--;
	// 	UpdatePreview();
	// }

	private void OnValidate()
	{
		if (ButtonName != null && ButtonIndex != null)
			ButtonName.Value = ButtonIndex == 0 ? "Primary" : $"Button #{ButtonIndex.Value}";
	}
}
