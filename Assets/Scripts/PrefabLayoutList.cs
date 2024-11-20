using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PrefabLayoutList<T> where T : Component
{
	public T Prefab;
	public Transform Layout;

	[NonSerialized] private List<T> m_list;
	public List<T> List => m_list ??= new List<T>();
	public int Count => List.Count;
	public T this[int index] => List[index];

	public T Initialize()
	{
		T instance = UnityEngine.Object.Instantiate(Prefab, Layout);
		List.Add(instance);
		return instance;
	}
}