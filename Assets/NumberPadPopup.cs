using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class NumberPadPopup : InputPopup<NumberPadPopup, int, NumberPadPopup.InputOptions, NumberPopupQuickItem>
{
	public delegate string FormatCallback(int unit, int value);

	public class InputOptions : InputPopupShowOptions<int>
	{
		public string InitialText;
		public string UnitsText;
		public int NumberOfFormats;
		public FormatCallback Formatter;
	}

	protected override void OnShow(InputOptions options)
	{
		UnitsText.Value = options.UnitsText;
		RawText.Value = options.InitialText ?? "—";
		UnitType.Value = 0;
		Formats = options.NumberOfFormats;
		Formatter = options.Formatter;
	}

	public SDispatcher<string> RawText;
	public SDispatcher<string> FormattedPreview;
	public SDispatcher<string> UnitsText;
	public SDispatcher<int> UnitType;
	public SDispatcher<bool> IsntEmpty;

	public int Formats { get; private set; }
	public FormatCallback Formatter { get; private set; }

	protected override bool ResultValid => Result.Value != int.MaxValue;

	public void OnNumberPadButton(int value)
	{
		if (RawText.Value == "—")
			RawText.Value = value.ToString();
		else
			RawText.Value += value.ToString();

		UpdatePreview();
	}

	public void OnNumberPadBackspace()
	{
		int minLength = RawText.Value.StartsWith("-") ? 2 : 1;
		if (RawText.Value.Length > minLength)
			RawText.Value = RawText.Value.Substring(0, RawText.Value.Length - 1);
		else
			RawText.Value = "—";

		UpdatePreview();
	}

	public void OnNumberPadClear()
	{
		RawText.Value = "—";
		UpdatePreview();
	}

	public void OnNumberPadNegative()
	{
		if (RawText.Value.StartsWith("-"))
			RawText.Value = RawText.Value.Substring(1);
		else
			RawText.Value = "-" + RawText.Value;

		UpdatePreview();
	}

	public void OnNextUnitType()
	{
		UnitType.Value = (UnitType.Value + 1) % Formats;
		UpdatePreview();
	}

	protected override void UpdatePreview()
	{
		if (RawText.Value == "—" || !int.TryParse(RawText.Value, out int value))
		{
			Result.Value = int.MaxValue;
			FormattedPreview.Value = "";
		}
		else
		{
			Result.Value = value;
			FormattedPreview.Value = Formatter(UnitType, value);
		}

		IsntEmpty.Value = RawText.Value != "—";

		base.UpdatePreview();

		for (int i = 0; i < QuickFillList.Count; i++)
			SetQuickFillEntry(QuickFillList[i], QuickFillEntries[i].Value);
	}

	protected override int ValueSortScore(int value)
	{
		return Levenshtein.Distance(value.ToString(), Result.Value.ToString());
	}

	protected override void SetQuickFillEntry(NumberPopupQuickItem item, int value)
	{
		item.Preview.text = Formatter(UnitType, value);
		item.Value.text = value.ToString();
	}

	private void OnValidate()
	{
		if (IsResultValid != null && RawText != null)
			IsResultValid.Value = RawText.Value != "—" && int.TryParse(RawText.Value, out _);

		if (IsntEmpty != null && RawText != null)
			IsntEmpty.Value = RawText.Value != "—";
	}

	protected override void QuickFillClicked(int value)
	{
		RawText.Value = value.ToString();
	}
}
