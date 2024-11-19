using System;
using UnityEngine;
using UnityEngine.UI;

public class IncDecTimer_ActionInput : TrackerActionDataInput
{
	public override ushort DefaultData => 30; // 30 seconds

	public override void UpdatePreview()
	{
	}

	private static NumberPadPopup.InputOptions IncDecAmountOptions = new NumberPadPopup.InputOptions
	{
		Title = "Inc/Dec Time",
		Type = NumberPadPopup.InputType.Duration,
		MaxValue = TimeShort.MaxValue,
	};

	private void SetSeconds(int seconds)
	{
		SetData(TimeShort.FromSeconds(seconds));
	}

	public void OpenTimePopup()
	{
		IncDecAmountOptions.InitialText = TimeShort.ToSeconds(action.NetworkData.data).ToString();
		IncDecAmountOptions.OnSuccess = SetSeconds;
		NumberPadPopup.Show(IncDecAmountOptions);
	}
}