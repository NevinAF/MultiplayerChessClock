using System;
using UnityEngine;
using UnityEngine.UI;

public class OnOffTimer_ActionInput : TrackerActionDataInput
{
	public TrackerDispatchers CopyFrom;

	private int saved_copy_target = 0;

	public SDispatcher<bool> IsToggle;
	public SDispatcher<bool> IsOnOrOff;
	public SDispatcher<bool> OnOrOff_IsOn;
	public SDispatcher<bool> IsCopyFrom;
	public SDispatcher<bool> CopyFrom_IsInvert;

	public TrackerDispatchers dispatchers;


	public override void UpdatePreview()
	{
		short toggleTarget = (short)action.NetworkData.data;

		IsToggle.Value = toggleTarget == short.MaxValue - 2;
		IsOnOrOff.Value = toggleTarget >= short.MaxValue - 1;
		IsCopyFrom.Value = !IsToggle.Value && !IsOnOrOff.Value;
		UnityEngine.Debug.Log("UpdatePreview: " + toggleTarget);

		if (IsOnOrOff.Value) {
			OnOrOff_IsOn.Value = toggleTarget == short.MaxValue - 1;
		} else if (IsCopyFrom.Value) {
			CopyFrom_IsInvert.Value = toggleTarget < 0;
			saved_copy_target = CopyFrom_IsInvert.Value ? (-toggleTarget - 1) : toggleTarget;
		}

		dispatchers.PipeFrom((byte)saved_copy_target);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		dispatchers.ClearData();
	}

	public void SetIsOn(bool isOn)
	{
		SetData((ushort)(isOn ? short.MaxValue - 1 : short.MaxValue));
	}

	public void SetIsInvert(bool isInvert)
	{
		SetData((ushort)(isInvert ? -(saved_copy_target + 1) : saved_copy_target));
	}

	public void SetCopyTarget(byte target)
	{
		saved_copy_target = target;
		SetIsInvert(CopyFrom_IsInvert.Value);
	}

	public void SelectCopyTarget()
	{
		SetIsInvert(CopyFrom_IsInvert.Value);
	}

	public void SelectOnOff()
	{
		SetIsOn(OnOrOff_IsOn.Value);
	}

	public void SelectToggle()
	{
		SetData((ushort)(short.MaxValue - 2));
	}

	private readonly TrackerInputPopup.InputOptions attachedPopupOptions = new TrackerInputPopup.InputOptions
	{
		Title = "Select Copy From Tracker"
	};

	public override ushort DefaultData => ushort.MaxValue - 2; // Toggle

	public void OpenCopyFromPopup()
	{
		attachedPopupOptions.OnSuccess = SetCopyTarget;
		attachedPopupOptions.currentValue = saved_copy_target;
		TrackerInputPopup.Show(attachedPopupOptions);
	}
}