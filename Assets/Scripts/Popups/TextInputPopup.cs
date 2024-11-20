using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TextInputPopup : InputPopup<TextInputPopup, string, TextInputPopup.InputOptions, TextPopupQuickItem>
{
	public class InputOptions : InputPopupShowOptions<string>
	{
		public string InitialValue;
		public int MaxLength;
	}

	protected override void OnShow(InputOptions options)
	{
		Result.Value = options.InitialValue;
		MaxLength = options.MaxLength;
	}

	protected override bool ResultValid => Result.Value.Length > 0;

	private int MaxLength = 32;

	protected override void UpdatePreview()
	{
		if (Result.Value.Length > MaxLength)
			Result.Value = Result.Value.Substring(0, MaxLength);
		base.UpdatePreview();
	}

	public void Randomize()
	{
		Result.Value = QuickFillEntries[UnityEngine.Random.Range(0, QuickFillEntries.Count)].Value;
	}

	public void OnTextInputChanged(string value)
	{
		Result.Value = value;
		UpdatePreview();
	}

	protected override int ValueSortScore(string value)
	{
		return Levenshtein.Distance(Result, value);
	}

	protected override void QuickFillClicked(string value)
	{
		Result.Value = value;
	}

	protected override void SetQuickFillEntry(TextPopupQuickItem item, string value)
	{
		item.Preview.text = value;
	}
}
