using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CrossFadeGroupImage : Image
{
	public Graphic[] targets;

	public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
	{
		base.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB);

		for (int i = 0; targets != null && i < targets.Length; i++)
			if (targets[i] != null)
				targets[i].CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB);
	}
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(CrossFadeGroupImage))]
public class CrossFadeGroupImageEditor : UnityEditor.UI.ImageEditor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		UnityEditor.EditorGUI.BeginChangeCheck();
		var targetsProperty = serializedObject.FindProperty("targets");
		UnityEditor.EditorGUILayout.PropertyField(targetsProperty);
		if (UnityEditor.EditorGUI.EndChangeCheck())
			serializedObject.ApplyModifiedProperties();
	}
}
#endif