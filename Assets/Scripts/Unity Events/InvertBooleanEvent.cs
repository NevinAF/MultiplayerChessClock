using UnityEngine;
using UnityEngine.Events;

public class InvertBooleanEvent : MonoBehaviour
{
	public UnityEvent<bool> OnChange;

	public void SendEvent(bool value)
	{
		OnChange?.Invoke(!value);
	}
}