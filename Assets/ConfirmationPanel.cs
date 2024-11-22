using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConfirmationPanel : SingletonMono<ConfirmationPanel>
{
	[Serializable]
	public struct ConfirmationPanelEntry
	{
		public string title;
		[TextArea(3, 10)]
		public string message;
		public string confirmButtonText;
		public UnityEvent onConfirm;
	}

	public ConfirmationPanelEntry DisconnectEntry;

	public SDispatcher<string> Title;
	public SDispatcher<string> Message;
	public SDispatcher<string> ConfirmButtonText;
	public Action OnConfirm;

	public void Show(string title, string message, string confirmButtonText, Action onConfirm)
	{
		Title.Value = title;
		Message.Value = message;
		ConfirmButtonText.Value = confirmButtonText;
		OnConfirm = onConfirm;
		gameObject.SetActive(true);
	}

	public void Show(ConfirmationPanelEntry entry)
	{
		Title.Value = entry.title;
		Message.Value = entry.message;
		ConfirmButtonText.Value = entry.confirmButtonText;
		OnConfirm = entry.onConfirm.Invoke;
		gameObject.SetActive(true);
	}

	public void OnConfirmButton()
	{
		OnConfirm?.Invoke();
		gameObject.SetActive(false);
	}

	public void ShowDisconnect() => Show(DisconnectEntry);
}
