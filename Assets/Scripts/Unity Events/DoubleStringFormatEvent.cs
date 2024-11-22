using UnityEngine;
using UnityEngine.Events;

public class DoubleStringFormatEvent : MonoBehaviour
{
	public string format = "{0} - {1}";
	public UnityEvent<string> OnChange;

	[SerializeField] private string m_value0;
	[SerializeField] private string m_value1;

	public void SetValue0(string value)
	{
		m_value0 = value;
		OnChange?.Invoke(string.Format(format, m_value0, m_value1));
	}

	public void SetValue1(string value)
	{
		m_value1 = value;
		OnChange?.Invoke(string.Format(format, m_value0, m_value1));
	}

	private void OnValidate()
	{
		OnChange?.Invoke(string.Format(format, m_value0, m_value1));
	}
}