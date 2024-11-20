using UnityEngine;
using UnityEngine.Events;

public class IntToBoolEvent : MonoBehaviour
{
	public enum Comparison
	{
		Equals,
		NotEquals,
		GreaterThan,
		LessThan
	}

	public Comparison comparison;
	public int value;
	public UnityEvent<bool> OnChange;

	public void SendEvent(int value)
	{
		switch (comparison)
		{
			case Comparison.Equals:
				OnChange?.Invoke(value == this.value);
				break;
			case Comparison.NotEquals:
				OnChange?.Invoke(value != this.value);
				break;
			case Comparison.GreaterThan:
				OnChange?.Invoke(value > this.value);
				break;
			case Comparison.LessThan:
				OnChange?.Invoke(value < this.value);
				break;
		}
	}
}