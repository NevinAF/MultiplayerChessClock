using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum InputQuickFillIconType
{
	Recommended,
	History,
	Other
}

public class InputPopupShowOptions<T>
{
	public string Title;
	public Action<T> OnSuccess;
}

public abstract class InputPopup<S, T, O, I> : SingletonMono<S>
	where S : InputPopup<S, T, O, I>
	where O : InputPopupShowOptions<T>
	where I : InputPopupQuickItem
{
	[Serializable]
	public struct QuickFillEntry
	{
		public T Value;
		public InputQuickFillIconType Icon;
	}

	public static void Show(O options)
	{
		OnSuccess = options.OnSuccess;
		Instance.Title.Value = options.Title;
		Instance.OnShow(options);
		Instance.UpdatePreview();
		Instance.gameObject.SetActive(true);

		PopupManager.RegisterClickOffLayer(Hide);
	}

	public static void Hide()
	{
		Instance.gameObject.SetActive(false);
	}

	public static void AddQuickFillEntry(T value, InputQuickFillIconType icon)
	{
		int index = Instance.QuickFillEntries.FindIndex(e => EqualityComparer<T>.Default.Equals(e.Value, value));
		if (index < 0)
		{
			index = Instance.QuickFillEntries.Count;

			var entry = new QuickFillEntry
			{
				Value = value,
				Icon = icon
			};

			Instance.QuickFillEntries.Add(entry);

			if (QuickFillList != null)
				CreateQuickFillItem(entry);
		}
		else {
			var entry = Instance.QuickFillEntries[index];
			entry.Icon = icon;
			Instance.QuickFillEntries[index] = entry;

			var item = QuickFillList[index];
			Instance.SetQuickFillEntry(item, value);
			item.SetIcon(icon);
		}
	}

	protected static void UpdateQuickEntries()
	{
		if (QuickFillList == null)
			return;

		for (int i = 0; i < QuickFillList.Count; i++)
			Instance.SetQuickFillEntry(QuickFillList[i], Instance.QuickFillEntries[i].Value);
	}

	private static void CreateQuickFillItem(QuickFillEntry entry)
	{
		I item = Instantiate(Instance.QuickFillPrefab, Instance.QuickFillGroup);
		int index = QuickFillList.Count;
		item.Button.onClick.AddListener(() => QuickFillButtonClicked(index));

		Instance.SetQuickFillEntry(item, entry.Value);
		item.SetIcon(entry.Icon);
		// item.Index = QuickFillList.Count;

		QuickFillList.Add(item);
	}

	private static void QuickFillButtonClicked(int index)
	{
		Instance.QuickFillClicked(Instance.QuickFillEntries[index].Value);
		Instance.UpdatePreview();
	}

	protected static Action<T> OnSuccess;
	protected static List<I> QuickFillList;

	public SDispatcher<bool> IsResultValid;
	public SDispatcher<string> Title;
	public SDispatcher<T> Result;
	public Transform QuickFillGroup;
	public I QuickFillPrefab;
	public List<QuickFillEntry> QuickFillEntries;

	protected virtual bool ResultValid => true;
	protected virtual bool SortItemsViaChildIndex => false;


	protected virtual void Start()
	{
		QuickFillList = new List<I>();
		for (int i = 0; i < QuickFillEntries.Count; i++)
			CreateQuickFillItem(QuickFillEntries[i]);

		UpdatePreview();
	}

	public virtual void OnSubmit()
	{
		if (ResultValid)
		{
			OnSuccess.Invoke(Result);
			AddQuickFillEntry(Result, InputQuickFillIconType.History);
		}
		PopupManager.CloseUntil(Hide);
	}

	public virtual void OnCancel()
	{
		PopupManager.CloseUntil(Hide);
	}

	protected virtual void UpdatePreview()
	{
		if (QuickFillList == null)
			return;

		IsResultValid.Value = ResultValid;

		int previewIndex = 0;
		foreach (var entry in QuickFillEntries.Select((e, i) => new { e, i }).OrderBy(e =>
			IsResultValid ? ValueSortScore(e.e.Value) : 0
		).ThenBy(e => e.e.Icon switch
		{
			InputQuickFillIconType.History => 0,
			InputQuickFillIconType.Recommended => 1,
			_ => 2
		}).ThenBy(e => -e.i))
		{
			QuickFillList[entry.i].transform.SetSiblingIndex(previewIndex);
			previewIndex++;
		}
	}

	protected abstract void OnShow(O options);
	protected abstract int ValueSortScore(T value);
	protected abstract void QuickFillClicked(T value);
	protected abstract void SetQuickFillEntry(I item, T value);
}