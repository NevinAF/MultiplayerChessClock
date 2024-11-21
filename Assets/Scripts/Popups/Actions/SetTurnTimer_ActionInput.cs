using System;
using UnityEngine;
using UnityEngine.UI;

public class SetTurnTimer_ActionInput : TrackerActionDataInput
{
	public override ushort DefaultData => 30; // 30 seconds

	public override void UpdatePreview()
	{
	}

	private static NumberPadPopup.InputOptions TurnTimeAmountOptions = new NumberPadPopup.InputOptions
	{
		Title = "Set Turn Time",
		Type = NumberPadPopup.InputType.Duration,
		MaxValue = TimeShort.MaxValue,
		disallowNegative = true
	};

	private void SetSeconds(int seconds)
	{
		SetData(TimeShort.FromSeconds(seconds));
	}

	public void OpenTimePopup()
	{
		TurnTimeAmountOptions.InitialText = TimeShort.ToSeconds(action.NetworkData.data).ToString();
		TurnTimeAmountOptions.OnSuccess = SetSeconds;
		NumberPadPopup.Show(TurnTimeAmountOptions);
	}
}