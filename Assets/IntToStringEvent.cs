using UnityEngine;
using UnityEngine.Events;

public class IntToStringEvent : MonoBehaviour
{
	public string format = "{0}";
	public UnityEvent<string> OnChange;

	public void SendEvent(int value)
	{
		OnChange?.Invoke(string.Format(format, value));
	}
}