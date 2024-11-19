using UnityEngine;
using UnityEngine.Events;

public class SplitBooleanEvent : MonoBehaviour
{
	public UnityEvent<bool> OnTrue;
	public UnityEvent<bool> OnFalse;

	public void SendEvent(bool value)
	{
		(value ? OnTrue : OnFalse)?.Invoke(value);
	}
}