using UnityEngine;
using UnityEngine.Events;

public class Color32ToColorEvent : MonoBehaviour
{
	public UnityEvent<Color> OnChange;

	public void SendEvent(Color32 value)
	{
		OnChange?.Invoke(value);
	}
}