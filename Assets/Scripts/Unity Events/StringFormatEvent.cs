using UnityEngine;
using UnityEngine.Events;

public class StringFormatEvent : MonoBehaviour
{
	public string format = "{0}";
	public UnityEvent<string> OnChange;

	public void SendEvent(string value)
	{
		OnChange?.Invoke(string.Format(format, value));
	}
}