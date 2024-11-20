using System;
using UnityEngine;
using UnityEngine.UI;

public class SetTimer_ActionInput : TrackerActionDataInput
{
	public override ushort DefaultData => 30; // 30 seconds

	public override void UpdatePreview()
	{
	}

	private static NumberPadPopup.InputOptions TimeRemainingOptions = new NumberPadPopup.InputOptions
	{
		Title = "Set Time",
		Type = NumberPadPopup.InputType.Duration,
		MaxValue = TimeShort.MaxValue,
	};

	private void SetSeconds(int seconds)
	{
		SetData(TimeShort.FromSeconds(seconds));
	}

	public void OpenTimePopup()
	{
		TimeRemainingOptions.InitialText = TimeShort.ToSeconds(action.NetworkData.data).ToString();
		TimeRemainingOptions.OnSuccess = SetSeconds;
		NumberPadPopup.Show(TimeRemainingOptions);
	}
}