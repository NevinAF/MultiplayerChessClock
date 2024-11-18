using UnityEngine;

public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
	protected static T m_instance;
	public static T Instance
	{
		get
		{
			if (m_instance == null)
				m_instance = FindObjectOfType<T>(true);
			if (m_instance == null)
				Debug.LogError(typeof(T).Name + " not available in the current scene.");
			return m_instance;
		}
	}

	protected virtual void Awake()
	{
		m_instance = (T)this;
	}
}